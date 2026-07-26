# Implement Cross-Platform Cloud Verification

This ExecPlan is a living document.

## Purpose / Big Picture

Provide a repeatable Unity Build Automation verification stage for selected Unity Test Framework tests. A test declares a functional category and the player targets it supports. The stage writes an inspectable report and evidence manifest into the build output so a developer can diagnose the result without replaying the build log.

## Progress

- [x] Define the Android and macOS matrix, category semantics, report interaction, and Blocking/ReportOnly policy.
- [x] Add test metadata and opt the TutorialFocus layout tests into CloudVerification.
- [x] Add the UBA runner and static report generator.
- [x] Add deterministic report-generator coverage and project documentation.
- [x] Validate the local Android flow: catalog export, target filtering, Unity category run (2 passed), manifest, and static HTML report.
- [ ] Configure Android and macOS UBA targets, execute passing and intentional-failure canaries, then enable Blocking mode.

## Surprises & Discoveries

- Unity exits before starting command-line tests when `-quit` is passed with `-runTests`; the cloud runner intentionally omits that flag for its test invocation.
- The generated root solution has duplicate package project names. C# formatting uses the affected generated `.csproj` instead of the root solution.

## Decision Log

- Category describes the product area (`Tutorial`), never the platform.
- `CloudVerificationTargets` carries platform eligibility. Ineligible tests are reported as `NotApplicable`, never as failures.
- The UBA Dashboard owns build configurations and Discord integration because neither is versioned in this repository.
- Version one creates one report per UBA build. Historical cross-build aggregation and physical-device testing are deferred.

## Context and Orientation

The UBA pre-build entry point is `.agents/scripts/CloudBuild/RunCloudVerification.sh`. It writes all generated files under `$OUTPUT_DIRECTORY/Verification`. `GenerateCloudVerificationReport.py` transforms NUnit XML, the Editor log, metadata properties, and evidence sidecars into `verification-manifest.json` and `index.html`.

Tests opt in with NUnit's `CloudVerification` category and `CloudVerificationTargetsAttribute`. The initial fixture is `TutorialFocusLayoutTests` in the GearEngine Editor test assembly.

## Plan of Work

1. Add target metadata and categorize TutorialFocus tests.
2. Run one Unity test job selected by `CloudVerification`, retain NUnit XML and Editor log, then generate a normalized manifest and report.
3. Configure `CloudVerificationAndroid` and `CloudVerificationMacOS` in UBA to invoke the shell script as a pre-build hook with native UBA tests disabled.
4. Run report-only canaries before turning either target into a blocking gate.

## Validation and Acceptance

- The report generator unit test proves category, target eligibility, evidence hashing, failed-test detail, and manifest generation.
- A local selected test run writes NUnit XML, log, `verification-manifest.json`, and `index.html`.
- In UBA, a passing and intentional-failure run on each target retain the Verification directory and cause the existing Discord integration to report the final build state.
- The report list filters Category, Mode, Status, and Evidence immediately; its test details replace the list and Previous/Next retain the current filtered order.

## Idempotence and Recovery

The runner recreates only its own `$OUTPUT_DIRECTORY/Verification` directory. `ReportOnly` always returns success after report generation; `Blocking` returns the Unity test failure code. If the UBA pre-build hook cannot retain output from a blocking failure, configure the documented post-build fallback before enabling Blocking mode.

## Interfaces and Dependencies

- `CloudVerificationTarget`: `Android` or `MacOS`.
- `CloudVerificationTargetsAttribute`: NUnit test metadata property named `CloudVerificationTargets` with a comma-separated target list.
- Environment variables: `PROJECT_DIRECTORY`, `OUTPUT_DIRECTORY`, `UNITY_EXE`, `BUILD_TARGET`, `BUILD_NUMBER`, `GIT_BRANCH`, `GIT_COMMIT`, `UNITY_VERSION`, `BUILDER_OS`, `CLOUD_VERIFICATION_MODE`, and `CLOUD_VERIFICATION_TEST_PLATFORM`.
