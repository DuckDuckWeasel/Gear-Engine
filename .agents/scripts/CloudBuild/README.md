# Cloud Verification Build Automation

`RunCloudVerification.sh` runs the selected NUnit category before a Unity Build Automation player build and writes a self-contained report to `$OUTPUT_DIRECTORY/Verification`.

## Test contract

Use a functional NUnit category together with the pipeline category and targets:

```csharp
[Category("CloudVerification")]
[Category("Tutorial")]
[CloudVerificationTargets(CloudVerificationTarget.Android, CloudVerificationTarget.MacOS)]
[Test]
public void FocusLayout_WhenDirectionOffsetIsSet_UsesPreviewDistance()
```

`CloudVerification` selects the test for the pipeline. The other category is shown in the report. `CloudVerificationTargets` identifies Android, macOS, or both as supported player targets. The runner exports this metadata before execution, runs only tests eligible for the selected target, and records the remainder as `NotApplicable`.

For a screenshot or video, write the artifact under `Verification/Evidence/` and add `<artifact>.evidence.json` beside it. The sidecar requires `test`, `artifact`, `scenario`, and `criteria`. The report hashes the artifact and links it to the matching NUnit test.

## Unity Build Automation setup

Create two configurations from the existing player configuration:

| Configuration | Player target | Pre-build script |
| --- | --- | --- |
| `CloudVerificationAndroid` | Android ARM64 | `.agents/scripts/CloudBuild/RunCloudVerification.sh` |
| `CloudVerificationMacOS` | macOS | `.agents/scripts/CloudBuild/RunCloudVerification.sh` |

For both configurations:

1. Disable the native UBA Tests option to prevent duplicate execution.
2. Set `CLOUD_VERIFICATION_MODE=ReportOnly` during the ten-build canary period; set it to `Blocking` after both canaries retain artifacts.
3. Set `CLOUD_VERIFICATION_TEST_PLATFORM=EditMode` for the current TutorialFocus slice. Add a separate PlayMode job when runtime visual evidence is available.
4. Keep the existing Discord Build Success and Build Failure integration enabled. It remains the authoritative final outcome.

UBA supplies `PROJECT_DIRECTORY`, `OUTPUT_DIRECTORY`, `UNITY_EXE`, build identity, source metadata, builder OS, and player target. The runner records them in the report manifest.

## Recovery

If a deliberately failing Blocking canary does not retain `Verification/`, switch the configuration to `ReportOnly`, finish the player build, and add a post-build failure step that returns failure only after artifact upload. Do not enable Blocking until that behavior is verified.
