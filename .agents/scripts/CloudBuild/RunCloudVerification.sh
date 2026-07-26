#!/usr/bin/env bash
set -euo pipefail

mode="${CLOUD_VERIFICATION_MODE:-Blocking}"
test_platform="${CLOUD_VERIFICATION_TEST_PLATFORM:-EditMode}"
test_category="${CLOUD_VERIFICATION_CATEGORY:-CloudVerification}"
project_directory="${PROJECT_DIRECTORY:-$(pwd)}"
output_directory="${OUTPUT_DIRECTORY:-$project_directory/Artifacts/CloudVerification}"
unity_exe="${UNITY_EXE:-}"
verification_directory="$output_directory/Verification"
results_path="$verification_directory/NUnitResults.xml"
log_path="$verification_directory/Editor.log"
catalog_path="$verification_directory/test-catalog.json"
catalog_log_path="$verification_directory/CatalogExport.log"
script_directory="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ "$mode" != "Blocking" && "$mode" != "ReportOnly" ]]; then
    echo "CLOUD_VERIFICATION_MODE must be Blocking or ReportOnly." >&2
    exit 64
fi

if [[ "$test_platform" != "EditMode" && "$test_platform" != "PlayMode" ]]; then
    echo "CLOUD_VERIFICATION_TEST_PLATFORM must be EditMode or PlayMode." >&2
    exit 64
fi

if [[ -z "$unity_exe" || ! -x "$unity_exe" ]]; then
    echo "UNITY_EXE must point to an executable Unity Editor." >&2
    exit 64
fi

rm -rf "$verification_directory"
mkdir -p "$verification_directory"

export CLOUD_VERIFICATION_CATALOG_PATH="$catalog_path"
"$unity_exe" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$project_directory" \
    -executeMethod GearEngine.GearEngine.Editor.CloudVerificationCatalogExporter.Export \
    -logFile "$catalog_log_path"

test_filter="$(python3 "$script_directory/BuildCloudVerificationFilter.py" --catalog "$catalog_path" --build-target "${BUILD_TARGET:-Unknown}")"

set +e
"$unity_exe" \
    -batchmode \
    -nographics \
    -projectPath "$project_directory" \
    -runTests \
    -testPlatform "$test_platform" \
    -testCategory "$test_category" \
    -testFilter "$test_filter" \
    -testResults "$results_path" \
    -logFile "$log_path"
unity_exit_code=$?
set -e

python3 "$script_directory/GenerateCloudVerificationReport.py" \
    --results "$results_path" \
    --log "$log_path" \
    --output "$verification_directory" \
    --build-target "${BUILD_TARGET:-Unknown}" \
    --execution-host "${BUILDER_OS:-Unknown}" \
    --build-number "${BUILD_NUMBER:-Unknown}" \
    --branch "${GIT_BRANCH:-Unknown}" \
    --commit "${GIT_COMMIT:-Unknown}" \
    --unity-version "${UNITY_VERSION:-Unknown}" \
    --mode "$mode" \
    --test-platform "$test_platform" \
    --unity-exit-code "$unity_exit_code" \
    --catalog "$catalog_path" \
    --selected-target "${BUILD_TARGET:-Unknown}"

if [[ "$mode" == "Blocking" && "$unity_exit_code" -ne 0 ]]; then
    exit "$unity_exit_code"
fi

exit 0
