#!/usr/bin/env bash
# What /commit runs before it will draft a commit plan here. This fork commits straight to its
# `local` branch and never opens a pull request against it, so nothing else reads a change on its
# way in — upstream CI only ever sees what is cherry-picked onto a topic branch off upstream/main.
#
# The steps mirror .github/workflows/ci.yml in CI's own order, stopping before anything that drives
# a window. Each is the same command that workflow runs, so the two cannot disagree about what a
# change has to pass.
#
# What a pass here does NOT cover, all of it still CI's:
#   - The ten pwsh conformance and integration suites. They launch Agwinterm.Win32.exe and drive its
#     real window, so running them from a commit gate would seize the desktop on every commit; the
#     six hud-ui.ps1 suites cannot run here at all, since the script reads a suite-token helper that
#     exists only on the maintainer's machine and throws on any other local desktop.
#   - tools/test-ralphex-revmux.sh. CI pins ralphex v1.6.1 and installs it per run; whatever version
#     this machine has is a different fixture, so a pass or a failure here would mean neither.
#   - Packaging. installer/build.ps1 publishes win-x64 self-contained, asserts the staged payload and
#     compiles the .iss — none of which `dotnet build` can see. It is a deliberate omission: it
#     roughly doubles the runtime and rewrites installer/Output, where `deploy` looks for the setup
#     it is about to install.
#   - src/Agwinterm.App and tools/iconGen, which are tracked but sit in neither Agwinterm.slnx nor
#     installer/build.ps1, so nothing compiles them.
#
# Prerequisite is `python3` on PATH. Its absence fails rather than skips: a check that cannot tell
# "passed" from "never ran" turns an open problem into a closed-looking one. The .NET, Rust and pwsh
# steps are Windows-only — this is a Win32 application — and on any other host they print SKIPPED
# with that reason rather than a silent pass.
set -uo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT" || exit 2

status=0

# Print one step's verdict from its buffered output. Detail appears only on a failure, so a wall of
# build output at every commit does not train the gate away — but a silent pass is the other
# failure, so a line always shows.
#
# Without a pattern the line is the last NON-EMPTY line: cargo ends its output with a blank one, and
# a bare `tail -n 1` printed a label with nothing after it. With one, every matching line is shown,
# for a step whose result is one line per project rather than one line in total — and a zero exit
# with no match at all is a failure, because `dotnet test --no-build` exits 0 when it matches no
# test assembly, which would otherwise print a bare label and read as a pass.
report() {
  local label="$1" out="$2" rc="$3" pattern="${4-}"
  if [ "$rc" -ne 0 ]; then
    echo "==> $label — FAILED"
    cat "$out"
    status=1
  elif [ -z "$pattern" ]; then
    echo "==> $label — $(grep -v '^[[:space:]]*$' "$out" | tail -n 1)"
  elif grep -qE "$pattern" "$out"; then
    echo "==> $label"
    grep -E "$pattern" "$out" | sed 's/^/      /'
  else
    echo "==> $label — NO SUMMARY (exited 0 and reported nothing; treating as a failure)"
    cat "$out"
    status=1
  fi
}

run() {
  local label="$1" out rc
  shift
  out="$(mktemp)"
  "$@" >"$out" 2>&1
  rc=$?
  report "$label" "$out" "$rc"
  rm -f "$out"
}

skip() { echo "==> $1 — SKIPPED ($2)"; }

# The .NET suite, with the one retry ci.yml carries. The .NET 10 test host has a known pathology
# under named-pipe churn — issue #118, as that workflow's comment calls it — which surfaces two ways:
# a native crash that aborts the run, or a deadlock that hangs it forever. Observed here on a clean
# tree: Pty aborted at 967 of 1106 with an AccessViolationException, having passed 1106 twice
# minutes earlier.
#
# Retrying matches CI rather than diverging from it, which is the point — a gate and a workflow that
# disagree about what the branch requires leave the stricter one to whoever happened to run it. The
# retry is loud, so it is never the invisible kind that makes a result depend on which attempt you
# read, and it fires only on #118's own two signatures: a genuine test failure exits non-zero with
# neither, and fails on the first attempt.
#
# The timeout is the deadlock half. The suite is about twenty seconds, so six minutes only trips on
# a hang, and `timeout` reports it as 124.
dotnet_tests() {
  local out rc
  out="$(mktemp)"
  for attempt in 1 2; do
    timeout 360 dotnet test Agwinterm.slnx -c Release --no-build >"$out" 2>&1
    rc=$?
    [ "$rc" -eq 0 ] && break
    if [ "$attempt" -eq 1 ] && { [ "$rc" -eq 124 ] || grep -qE 'Test Run Aborted|host process crashed' "$out"; }; then
      echo "==> .NET tests — RETRYING once: test-host crash or hang (issue #118), not a test failure"
      continue
    fi
    break
  done
  report ".NET tests" "$out" "$rc" '^(Passed|Failed)!'
  rm -f "$out"
}

# Every convention this repo has taken on, re-measured. Nothing else invokes the checker, so without
# this line the rules would have been measured once by the /adopt walk and never looked at again.
run "Conventions" python3 ~/.claude/conventions/check.py .

case "$(uname -s)" in
  MINGW*|MSYS*|CYGWIN*)
    # The C ABI is declared on both sides of the boundary — a Rust const and a C# RequiredAbi — and a
    # drift is otherwise caught at load, by whoever runs the mismatched pair. agliteterm declares it a
    # third time and now lives in its own repository, so the check here sees two.
    run "ABI agreement" pwsh -NoProfile -ExecutionPolicy Bypass -File tools/check-abi.ps1

    # The real type check. It covers all six projects in the solution, the two test projects
    # included, which is what makes it stricter than the publish installer/build.ps1 runs.
    run ".NET build" dotnet build Agwinterm.slnx -c Release

    # Arms the C#<->Rust differential oracles below: RustParityTests skip silently when the crate
    # dll is absent, so without this the parity suites would pass by not running.
    run "Rust build" cargo build --release --manifest-path native/Cargo.toml

    dotnet_tests

    run "Rust resize tests" cargo test --manifest-path native/agwinterm-ptyhost/Cargo.toml resize::tests
    ;;
  *)
    reason="Windows-only: agwinterm is a Win32 application"
    for label in "ABI agreement" ".NET build" "Rust build" ".NET tests" "Rust resize tests"; do
      skip "$label" "$reason"
    done
    ;;
esac

exit "$status"
