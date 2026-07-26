#!/usr/bin/env python3
"""Create a self-contained Unity Build Automation verification report."""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import html
import json
from pathlib import Path
from typing import Any
from xml.etree import ElementTree


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--results", required=True, type=Path)
    parser.add_argument("--log", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--build-target", required=True)
    parser.add_argument("--execution-host", required=True)
    parser.add_argument("--build-number", required=True)
    parser.add_argument("--branch", required=True)
    parser.add_argument("--commit", required=True)
    parser.add_argument("--unity-version", required=True)
    parser.add_argument("--mode", choices=("Blocking", "ReportOnly"), required=True)
    parser.add_argument("--test-platform", choices=("EditMode", "PlayMode"), required=True)
    parser.add_argument("--unity-exit-code", required=True, type=int)
    parser.add_argument("--catalog", type=Path)
    parser.add_argument("--selected-target")
    return parser.parse_args()


def get_properties(test_case: ElementTree.Element) -> dict[str, list[str]]:
    properties: dict[str, list[str]] = {}
    for property_node in test_case.findall("./properties/property"):
        name = property_node.get("name")
        value = property_node.get("value")
        if name is None or value is None:
            continue

        properties.setdefault(name, []).append(value)

    return properties


def read_evidence(output: Path) -> dict[str, list[dict[str, str]]]:
    evidence_by_test: dict[str, list[dict[str, str]]] = {}
    for sidecar in output.rglob("*.evidence.json"):
        try:
            payload = json.loads(sidecar.read_text(encoding="utf-8"))
            test_name = payload["test"]
            artifact_relative_path = payload["artifact"]
            scenario = payload["scenario"]
            criteria = payload["criteria"]
        except (KeyError, OSError, json.JSONDecodeError) as error:
            evidence_by_test.setdefault("__unassociated__", []).append(
                {
                    "path": str(sidecar.relative_to(output)),
                    "type": "Invalid",
                    "scenario": "Invalid evidence sidecar",
                    "criteria": str(error),
                    "sha256": "",
                }
            )
            continue

        artifact_path = sidecar.parent / artifact_relative_path
        evidence_type = artifact_path.suffix.lstrip(".").upper() or "File"
        sha256 = ""
        if artifact_path.is_file():
            sha256 = hashlib.sha256(artifact_path.read_bytes()).hexdigest()

        evidence_by_test.setdefault(test_name, []).append(
            {
                "path": str(artifact_path.relative_to(output)),
                "type": evidence_type,
                "scenario": scenario,
                "criteria": criteria,
                "sha256": sha256,
            }
        )

    return evidence_by_test


def normalize_status(result: str | None) -> str:
    if result == "Passed":
        return "Passed"
    if result == "Failed":
        return "Failed"
    if result in {"Skipped", "Inconclusive"}:
        return "Skipped"
    return "Blocked"


def parse_test_cases(results: Path, evidence_by_test: dict[str, list[dict[str, str]]], test_platform: str) -> list[dict[str, Any]]:
    if not results.is_file():
        return []

    root = ElementTree.parse(results).getroot()
    test_cases: list[dict[str, Any]] = []
    for test_case in root.findall(".//test-case"):
        properties = get_properties(test_case)
        full_name = test_case.get("fullname") or test_case.get("name") or "Unknown test"
        categories = [category for category in properties.get("Category", []) if category != "CloudVerification"]
        failure_node = test_case.find("./failure/message")
        failure_message = failure_node.text.strip() if failure_node is not None and failure_node.text else ""
        test_evidence = evidence_by_test.pop(full_name, [])
        duration = float(test_case.get("duration", "0"))
        test_cases.append(
            {
                "name": test_case.get("name", full_name),
                "fullName": full_name,
                "category": categories[0] if categories else "Uncategorized",
                "categories": categories,
                "mode": test_platform,
                "status": normalize_status(test_case.get("result")),
                "durationSeconds": duration,
                "supportedTargets": properties.get("CloudVerificationTargets", ["Unknown"])[0].split(","),
                "failureMessage": failure_message,
                "evidence": test_evidence,
            }
        )

    return test_cases


def normalize_target(build_target: str) -> str:
    normalized = build_target.lower()
    if "android" in normalized:
        return "Android"
    if "osx" in normalized or "macos" in normalized:
        return "MacOS"
    return "Unknown"


def apply_catalog(test_cases: list[dict[str, Any]], catalog_path: Path | None, selected_target: str | None) -> None:
    if catalog_path is None or not catalog_path.is_file():
        return

    catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    catalog_tests = {test["fullName"]: test for test in catalog.get("tests", [])}
    selected_target_name = normalize_target(selected_target or "Unknown")
    observed_test_names = {test_case["fullName"] for test_case in test_cases}

    for test_case in test_cases:
        catalog_test = catalog_tests.get(test_case["fullName"])
        if catalog_test is None:
            continue

        test_case["category"] = catalog_test["category"]
        test_case["categories"] = catalog_test["categories"]
        test_case["supportedTargets"] = catalog_test["targets"]

    for catalog_test in catalog_tests.values():
        if catalog_test["fullName"] in observed_test_names:
            continue

        is_eligible = selected_target_name in catalog_test["targets"]
        test_cases.append(
            {
                "name": catalog_test["name"],
                "fullName": catalog_test["fullName"],
                "category": catalog_test["category"],
                "categories": catalog_test["categories"],
                "mode": "Not run" if not is_eligible else "Unknown",
                "status": "NotApplicable" if not is_eligible else "Blocked",
                "durationSeconds": 0,
                "supportedTargets": catalog_test["targets"],
                "failureMessage": "Target is not supported." if not is_eligible else "Selected test did not produce an NUnit result.",
                "evidence": [],
            }
        )


def summarize(test_cases: list[dict[str, Any]], unity_exit_code: int) -> dict[str, int]:
    counts = {"total": len(test_cases), "passed": 0, "failed": 0, "skipped": 0, "blocked": 0, "notApplicable": 0}
    for test_case in test_cases:
        status = test_case["status"]
        if status == "NotApplicable":
            counts["notApplicable"] += 1
        elif status.lower() in counts:
            counts[status.lower()] += 1

    if unity_exit_code != 0 and counts["failed"] == 0:
        counts["blocked"] += 1

    return counts


def build_manifest(arguments: argparse.Namespace) -> dict[str, Any]:
    evidence_by_test = read_evidence(arguments.output)
    test_cases = parse_test_cases(arguments.results, evidence_by_test, arguments.test_platform)
    apply_catalog(test_cases, arguments.catalog, arguments.selected_target)
    log_tail = ""
    if arguments.log.is_file():
        log_tail = "\n".join(arguments.log.read_text(encoding="utf-8", errors="replace").splitlines()[-120:])

    generated_at = dt.datetime.now(dt.timezone.utc).isoformat()
    return {
        "schemaVersion": 1,
        "generatedAtUtc": generated_at,
        "build": {
            "configuration": "CloudVerification",
            "buildNumber": arguments.build_number,
            "branch": arguments.branch,
            "commit": arguments.commit,
            "unityVersion": arguments.unity_version,
            "playerTarget": arguments.build_target,
            "executionHost": arguments.execution_host,
            "mode": arguments.mode,
            "testPlatform": arguments.test_platform,
            "unityExitCode": arguments.unity_exit_code,
        },
        "summary": summarize(test_cases, arguments.unity_exit_code),
        "tests": test_cases,
        "unassociatedEvidence": evidence_by_test.get("__unassociated__", []),
        "logTail": log_tail,
    }


def report_document(manifest: dict[str, Any]) -> str:
    serialized_manifest = json.dumps(manifest, ensure_ascii=False).replace("</", "<\\/")
    return f"""<!doctype html>
<html lang=\"en\">
<head>
  <meta charset=\"utf-8\">
  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">
  <title>Cloud Verification Report</title>
  <style>
    :root {{ color-scheme: dark; font-family: Inter, ui-sans-serif, system-ui, sans-serif; background: #0b1020; color: #e6edf7; }}
    * {{ box-sizing: border-box; }} body {{ margin: 0; padding: 28px; }} main {{ max-width: 1320px; margin: auto; }}
    .context, .panel {{ background: #121a2f; border: 1px solid #273454; border-radius: 12px; padding: 18px; }}
    .context {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: 14px; margin-bottom: 20px; }}
    .label {{ display:block; color:#96a6c9; font-size:12px; margin-bottom:4px; }} h1 {{ margin:0 0 16px; font-size:24px; }} h2 {{ margin:0; font-size:18px; }}
    .summary {{ display:flex; gap:10px; flex-wrap:wrap; margin:16px 0; }} .pill {{ padding:6px 10px; border-radius:999px; background:#1e2b49; font-size:13px; }}
    table {{ width:100%; border-collapse:collapse; margin-top:14px; }} th, td {{ padding:12px 10px; border-bottom:1px solid #273454; text-align:left; vertical-align:top; }} th {{ color:#aebcda; font-size:12px; }}
    select, button {{ background:#1a2745; color:#e6edf7; border:1px solid #3a4a70; border-radius:7px; padding:7px 9px; font:inherit; }} button {{ cursor:pointer; }} tr[data-test-index] {{ cursor:pointer; }} tr[data-test-index]:hover {{ background:#192645; }}
    .status-Passed {{ color:#6ee7aa; }} .status-Failed, .status-Blocked {{ color:#ff8c8c; }} .status-Skipped {{ color:#f7ca7d; }} .evidence {{ color:#9fc4ff; }}
    .hidden {{ display:none; }} .detail-head {{ display:flex; justify-content:space-between; gap:12px; align-items:center; margin-bottom:16px; }} pre {{ white-space:pre-wrap; overflow-wrap:anywhere; background:#0b1020; border-radius:8px; padding:12px; max-height:320px; overflow:auto; }}
    .actions {{ display:flex; gap:8px; }} img, video {{ max-width:100%; max-height:480px; border-radius:8px; border:1px solid #33415f; margin-top:8px; }} .muted {{ color:#96a6c9; }}
  </style>
</head>
<body><main>
  <h1>Cloud Verification</h1>
  <section class=\"context\" id=\"build-context\"></section>
  <section class=\"panel\" id=\"list-view\">
    <div class=\"detail-head\"><h2>Tests</h2><button id=\"reset\">Reset</button></div>
    <div class=\"summary\" id=\"summary\"></div>
    <table><thead><tr>
      <th><button id=\"test-sort\">Test ↕</button></th>
      <th>Category<br><select id=\"category-filter\"></select></th>
      <th>Mode<br><select id=\"mode-filter\"></select></th>
      <th>Status<br><select id=\"status-filter\"></select></th>
      <th><button id=\"duration-sort\">Duration ↕</button></th>
      <th>Evidence<br><select id=\"evidence-filter\"></select></th>
      <th>Targets</th>
    </tr></thead><tbody id=\"tests\"></tbody></table>
  </section>
  <section class=\"panel hidden\" id=\"detail-view\"></section>
</main><script>
const report = {serialized_manifest};
const state = {{ filters: {{ category: 'All', mode: 'All', status: 'Action needed first', evidence: 'All' }}, sort: {{ key: 'attention', direction: 1 }}, selected: null }};
const byId = id => document.getElementById(id);
const escapeHtml = value => String(value).replace(/[&<>\"]/g, c => ({{'&':'&amp;','<':'&lt;','>':'&gt;','\"':'&quot;'}}[c]));
const evidenceLabel = test => test.evidence.length ? test.evidence.map(item => item.type).join(', ') : 'None';
function selectOptions(id, values, selected) {{
  const element = byId(id); element.innerHTML = values.map(value => `<option>${{escapeHtml(value)}}</option>`).join(''); element.value = selected;
}}
function filteredTests() {{
  const attention = {{ Failed: 0, Blocked: 0, Skipped: 1, Passed: 2, NotApplicable: 3 }};
  const tests = report.tests.filter(test =>
    (state.filters.category === 'All' || test.category === state.filters.category) &&
    (state.filters.mode === 'All' || test.mode === state.filters.mode) &&
    (state.filters.status === 'All' || state.filters.status === 'Action needed first' || test.status === state.filters.status) &&
    (state.filters.evidence === 'All' || evidenceLabel(test) === state.filters.evidence));
  return tests.sort((left, right) => {{
    if (state.sort.key === 'attention') return (attention[left.status] - attention[right.status]) * state.sort.direction || left.name.localeCompare(right.name);
    if (state.sort.key === 'name') return left.name.localeCompare(right.name) * state.sort.direction;
    return (left.durationSeconds - right.durationSeconds) * state.sort.direction;
  }});
}}
function renderContext() {{
  const build = report.build;
  const fields = [['Configuration', build.configuration], ['Build', build.buildNumber], ['Branch', build.branch], ['Commit', build.commit], ['Unity', build.unityVersion], ['Player target', build.playerTarget], ['Cloud builder', build.executionHost], ['Run mode', build.mode]];
  byId('build-context').innerHTML = fields.map(([label, value]) => `<div><span class=\"label\">${{label}}</span><strong>${{escapeHtml(value)}}</strong></div>`).join('');
}}
function renderList() {{
  const visible = filteredTests();
  byId('summary').innerHTML = [['Total', report.summary.total], ['Passed', report.summary.passed], ['Failed', report.summary.failed], ['Skipped', report.summary.skipped], ['Blocked', report.summary.blocked], ['Not applicable', report.summary.notApplicable], ['Visible', visible.length]].map(([label, value]) => `<span class=\"pill\">${{label}}: ${{value}}</span>`).join('');
  byId('tests').innerHTML = visible.map(test => {{ const index = report.tests.indexOf(test); return `<tr data-test-index=\"${{index}}\"><td>${{escapeHtml(test.name)}}</td><td>${{escapeHtml(test.category)}}</td><td>${{test.mode}}</td><td class=\"status-${{test.status}}\">${{test.status}}</td><td>${{test.durationSeconds.toFixed(3)}}s</td><td class=\"evidence\">${{escapeHtml(evidenceLabel(test))}}</td><td>${{escapeHtml(test.supportedTargets.join(', '))}}</td></tr>`; }}).join('') || '<tr><td colspan=\"7\" class=\"muted\">No tests match the active filters.</td></tr>';
  document.querySelectorAll('[data-test-index]').forEach(row => row.addEventListener('click', () => showDetail(Number(row.dataset.testIndex))));
}}
function showDetail(index) {{
  state.selected = index; const test = report.tests[index]; const visible = filteredTests(); const visibleIndex = visible.indexOf(test);
  const evidence = test.evidence.map(item => {{ const path = encodeURI(item.path); const media = item.type === 'PNG' || item.type === 'JPG' || item.type === 'JPEG' ? `<img src=\"${{path}}\" alt=\"${{escapeHtml(item.scenario)}}\">` : ''; return `<article><strong>${{escapeHtml(item.type)}}</strong><p>${{escapeHtml(item.scenario)}}</p><p class=\"muted\">${{escapeHtml(item.criteria)}}</p><a href=\"${{path}}\">${{escapeHtml(item.path)}}</a>${{media}}<p class=\"muted\">SHA-256: ${{escapeHtml(item.sha256 || 'missing')}}</p></article>`; }}).join('') || '<p class=\"muted\">No declared evidence was produced.</p>';
  byId('detail-view').innerHTML = `<div class=\"detail-head\"><button id=\"back\">← Back to tests</button><div class=\"actions\"><button id=\"previous\" ${{visibleIndex <= 0 ? 'disabled' : ''}}>Previous</button><button id=\"next\" ${{visibleIndex >= visible.length - 1 ? 'disabled' : ''}}>Next</button></div></div><h2>${{escapeHtml(test.name)}}</h2><p><span class=\"pill\">${{escapeHtml(test.category)}}</span> <span class=\"pill status-${{test.status}}\">${{test.status}}</span> <span class=\"pill\">${{test.mode}}</span></p><p><strong>Supported targets:</strong> ${{escapeHtml(test.supportedTargets.join(', '))}} · <strong>Duration:</strong> ${{test.durationSeconds.toFixed(3)}}s</p><p>${{escapeHtml(test.failureMessage || 'No failure message.')}}</p><h3>Evidence</h3>${{evidence}}<h3>Editor log tail</h3><pre>${{escapeHtml(report.logTail || 'No log was captured.')}}</pre>`;
  byId('list-view').classList.add('hidden'); byId('detail-view').classList.remove('hidden');
  byId('back').onclick = showList; byId('previous').onclick = () => showDetail(report.tests.indexOf(visible[visibleIndex - 1])); byId('next').onclick = () => showDetail(report.tests.indexOf(visible[visibleIndex + 1]));
}}
function showList() {{ byId('detail-view').classList.add('hidden'); byId('list-view').classList.remove('hidden'); renderList(); }}
function initializeFilters() {{
  selectOptions('category-filter', ['All', ...new Set(report.tests.map(test => test.category))], state.filters.category);
  selectOptions('mode-filter', ['All', ...new Set(report.tests.map(test => test.mode))], state.filters.mode);
  selectOptions('status-filter', ['Action needed first', 'All', 'Failed', 'Blocked', 'Skipped', 'Passed', 'NotApplicable'], state.filters.status);
  selectOptions('evidence-filter', ['All', ...new Set(report.tests.map(evidenceLabel))], state.filters.evidence);
  [['category-filter', 'category'], ['mode-filter', 'mode'], ['status-filter', 'status'], ['evidence-filter', 'evidence']].forEach(([id, key]) => byId(id).onchange = event => {{ state.filters[key] = event.target.value; renderList(); }});
  byId('test-sort').onclick = () => {{ state.sort = {{ key: 'name', direction: state.sort.key === 'name' ? -state.sort.direction : 1 }}; renderList(); }};
  byId('duration-sort').onclick = () => {{ state.sort = {{ key: 'duration', direction: state.sort.key === 'duration' ? -state.sort.direction : -1 }}; renderList(); }};
  byId('reset').onclick = () => {{ state.filters = {{ category: 'All', mode: 'All', status: 'Action needed first', evidence: 'All' }}; state.sort = {{ key: 'attention', direction: 1 }}; initializeFilters(); renderList(); }};
}}
renderContext(); initializeFilters(); renderList();
</script></body></html>"""


def write_report(manifest: dict[str, Any], output: Path) -> None:
    output.mkdir(parents=True, exist_ok=True)
    (output / "verification-manifest.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    (output / "index.html").write_text(report_document(manifest), encoding="utf-8")


def main() -> int:
    arguments = parse_arguments()
    manifest = build_manifest(arguments)
    write_report(manifest, arguments.output)
    print(f"Cloud verification report: {arguments.output / 'index.html'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
