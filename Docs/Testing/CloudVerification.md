# Cloud Verification

Cloud Verification is the Unity Build Automation test stage for a small, intentional set of Unity Test Framework tests. It produces a download-ready report for each cloud build at `Verification/index.html`.

## Reading the report

The report begins with build configuration, branch, full commit, Unity version, player target, cloud builder, run mode, and build number. The compact list is the entry point:

- Category, Mode, Status, and Evidence are immediate header dropdown filters.
- Test name and Duration toggle their sort direction with one button.
- Reset restores action-needed-first ordering.
- Selecting a test replaces the list with its detail, available evidence, integrity hash, failure message, and Editor-log tail. Back, Previous, and Next preserve the filtered result set.

The build’s target is distinct from its builder host. An Android player can be built by a macOS cloud builder; this stage records both values. It does not prove behavior on a physical device.

## Adding a test

Use `CloudVerification` to opt a test into cloud execution, a functional category for report filtering, and `CloudVerificationTargets` for player support:

```csharp
[Category("CloudVerification")]
[Category("Tutorial")]
[CloudVerificationTargets(CloudVerificationTarget.Android, CloudVerificationTarget.MacOS)]
[Test]
public void FocusLayout_WhenDirectionOffsetIsSet_UsesPreviewDistance()
{
    // Arrange, act, assert.
}
```

Do not encode a platform inside a functional category. A test can target Android, macOS, or both. The runner executes only target-eligible tests and reports the remainder as `NotApplicable`.

## Evidence

Place generated screenshots or videos under `Verification/Evidence/`. Every media file needs an adjacent `.evidence.json` sidecar containing its fully qualified test name, relative artifact path, scenario, and criteria. The report attaches the file to the test and records its SHA-256 hash.

Visual evidence is additional to assertions. A test that declares a screenshot still needs deterministic functional assertions. Follow `Docs/Testing/AutomatedTesting.md` for test design and the Unity batch-test contract for sidecar shape.

## UBA operation

Configure `.agents/scripts/CloudBuild/RunCloudVerification.sh` as the pre-build script for `CloudVerificationAndroid` and `CloudVerificationMacOS`. Use `ReportOnly` during the canary period, then `Blocking` only after passing and intentional-failure builds retain the Verification output. Keep native UBA tests disabled for these two configurations to avoid duplicate execution and retain the existing Discord build status integration.

The runner records standard UBA environment values in `verification-manifest.json`. The UBA dashboard is the source of truth for configuration secrets, build settings, artifact retention, and Discord configuration; none are committed to the project.
