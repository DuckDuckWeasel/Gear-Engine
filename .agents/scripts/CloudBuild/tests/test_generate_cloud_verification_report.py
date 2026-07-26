import argparse
import hashlib
import importlib.util
import json
import tempfile
import unittest
from pathlib import Path


SCRIPT_PATH = Path(__file__).parents[1] / "GenerateCloudVerificationReport.py"
SPECIFICATION = importlib.util.spec_from_file_location("cloud_verification_report", SCRIPT_PATH)
REPORT = importlib.util.module_from_spec(SPECIFICATION)
assert SPECIFICATION.loader is not None
SPECIFICATION.loader.exec_module(REPORT)

FILTER_PATH = Path(__file__).parents[1] / "BuildCloudVerificationFilter.py"
FILTER_SPECIFICATION = importlib.util.spec_from_file_location("cloud_verification_filter", FILTER_PATH)
FILTER = importlib.util.module_from_spec(FILTER_SPECIFICATION)
assert FILTER_SPECIFICATION.loader is not None
FILTER_SPECIFICATION.loader.exec_module(FILTER)


class GenerateCloudVerificationReportTests(unittest.TestCase):
    def test_build_manifest_preserves_test_metadata_and_evidence_hash(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            output = Path(temporary_directory)
            artifact = output / "Evidence" / "TutorialFocus.png"
            artifact.parent.mkdir()
            artifact.write_bytes(b"image-evidence")
            (artifact.with_name(f"{artifact.name}.evidence.json")).write_text(
                json.dumps(
                    {
                        "test": "GearEngine.GearEngine.Tests.Editor.TutorialFocusLayoutTests.DirectionOffset_UsesTheSameScreenDistanceAsThePreview",
                        "artifact": "TutorialFocus.png",
                        "scenario": "Tutorial focus offset",
                        "criteria": "Indicator matches the configured offset.",
                    }
                ),
                encoding="utf-8",
            )
            results = output / "NUnitResults.xml"
            results.write_text(
                """<test-run><test-suite><test-case name=\"DirectionOffset_UsesTheSameScreenDistanceAsThePreview\" fullname=\"GearEngine.GearEngine.Tests.Editor.TutorialFocusLayoutTests.DirectionOffset_UsesTheSameScreenDistanceAsThePreview\" result=\"Passed\" duration=\"0.125\"><properties><property name=\"Category\" value=\"CloudVerification\" /><property name=\"Category\" value=\"Tutorial\" /><property name=\"CloudVerificationTargets\" value=\"Android,MacOS\" /></properties></test-case><test-case name=\"PositionOffset_IsAppliedDirectlyInScreenSpace\" fullname=\"GearEngine.GearEngine.Tests.Editor.TutorialFocusLayoutTests.PositionOffset_IsAppliedDirectlyInScreenSpace\" result=\"Failed\" duration=\"0.25\"><failure><message>Offset mismatch</message></failure></test-case></test-suite></test-run>""",
                encoding="utf-8",
            )
            log = output / "Editor.log"
            log.write_text("Unity test log", encoding="utf-8")
            arguments = argparse.Namespace(
                results=results,
                log=log,
                output=output,
                build_target="Android",
                execution_host="MAC",
                build_number="42",
                branch="develop",
                commit="abc123",
                unity_version="6000.5.3f1",
                mode="Blocking",
                test_platform="EditMode",
                unity_exit_code=2,
                catalog=None,
                selected_target=None,
            )

            manifest = REPORT.build_manifest(arguments)
            REPORT.write_report(manifest, output)

            self.assertEqual("Tutorial", manifest["tests"][0]["category"])
            self.assertEqual(["Android", "MacOS"], manifest["tests"][0]["supportedTargets"])
            self.assertEqual("Failed", manifest["tests"][1]["status"])
            self.assertEqual("Offset mismatch", manifest["tests"][1]["failureMessage"])
            self.assertEqual(hashlib.sha256(b"image-evidence").hexdigest(), manifest["tests"][0]["evidence"][0]["sha256"])
            self.assertTrue((output / "verification-manifest.json").is_file())
            self.assertTrue((output / "index.html").is_file())

    def test_catalog_marks_unsupported_target_as_not_applicable(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            output = Path(temporary_directory)
            catalog = output / "test-catalog.json"
            catalog.write_text(
                json.dumps(
                    {
                        "tests": [
                            {
                                "fullName": "Tests.AndroidOnly",
                                "name": "AndroidOnly",
                                "category": "Tutorial",
                                "categories": ["Tutorial"],
                                "targets": ["Android"],
                            },
                            {
                                "fullName": "Tests.MacOnly",
                                "name": "MacOnly",
                                "category": "Tutorial",
                                "categories": ["Tutorial"],
                                "targets": ["MacOS"],
                            },
                        ]
                    }
                ),
                encoding="utf-8",
            )
            test_cases = []

            REPORT.apply_catalog(test_cases, catalog, "Android")

            self.assertEqual("Blocked", test_cases[0]["status"])
            self.assertEqual("NotApplicable", test_cases[1]["status"])
            self.assertEqual(["Tests.AndroidOnly"], FILTER.select_test_names(json.loads(catalog.read_text()), "Android"))


if __name__ == "__main__":
    unittest.main()
