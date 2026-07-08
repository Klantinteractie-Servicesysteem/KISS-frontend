#!/usr/bin/env python3
"""
Generates a consolidated HTML test report from TRX files (first-run + retry).
This ensures the GitHub Pages report reflects ALL tests and their final outcomes.
"""

import xml.etree.ElementTree as ET
import os
import sys
from datetime import datetime
from html import escape
from pathlib import Path

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}

def parse_trx(trx_path):
    """Parse a TRX file and return test results."""
    tree = ET.parse(trx_path)
    root = tree.getroot()
    
    results = {}
    
    # Map test IDs to class/method names
    test_definitions = {}
    for ut in root.findall('.//t:UnitTest', NS):
        test_id = ut.get('id')
        tm = ut.find('t:TestMethod', NS)
        if tm is not None:
            class_name = tm.get('className', 'Unknown')
            method_name = tm.get('name', 'Unknown')
            # Extract just the class name without namespace
            short_class = class_name.split('.')[-1] if '.' in class_name else class_name
            test_definitions[test_id] = {
                'className': short_class,
                'fullClassName': class_name,
                'methodName': method_name
            }
    
    # Get results
    for r in root.findall('.//t:UnitTestResult', NS):
        test_id = r.get('testId')
        test_name = r.get('testName', 'Unknown')
        outcome = r.get('outcome', 'Unknown')
        duration = r.get('duration', '00:00:00')
        
        # Get error info if failed
        error_message = ''
        error_stacktrace = ''
        output_elem = r.find('t:Output', NS)
        if output_elem is not None:
            err = output_elem.find('t:ErrorInfo', NS)
            if err is not None:
                msg_elem = err.find('t:Message', NS)
                stack_elem = err.find('t:StackTrace', NS)
                if msg_elem is not None and msg_elem.text:
                    error_message = msg_elem.text
                if stack_elem is not None and stack_elem.text:
                    error_stacktrace = stack_elem.text
        
        definition = test_definitions.get(test_id, {})
        
        results[test_name] = {
            'testName': test_name,
            'outcome': outcome,
            'duration': duration,
            'className': definition.get('className', 'Unknown'),
            'fullClassName': definition.get('fullClassName', 'Unknown'),
            'methodName': definition.get('methodName', test_name),
            'errorMessage': error_message,
            'errorStacktrace': error_stacktrace
        }
    
    return results


def get_friendly_class_name(class_name):
    """Convert class name to friendly display name."""
    mapping = {
        'AfhandelingFormScenarios': 'Afhandeling Form Tests',
        'AnonymousContactmomentScenarios': 'Anonymous Contactmoment Tests',
        'Unknown': 'Miscellaneous Tests',
    }
    if class_name in mapping:
        return mapping[class_name]
    return class_name.replace('Scenarios', ' Tests').replace('Test', ' Tests').replace('  ', ' ').strip()


def find_trace_file(traces_dir, test_name):
    """Try to find a matching trace zip for a test."""
    if not traces_dir or not os.path.isdir(traces_dir):
        return None
    
    # The trace file name typically matches the fully qualified test name
    safe_name = test_name.replace(' ', '_').replace('/', '_')
    
    for f in os.listdir(traces_dir):
        if f.endswith('.zip') and (test_name in f or safe_name in f):
            return f
    
    return None


def generate_html(all_results, retry_results, traces_dir, base_url):
    """Generate the consolidated HTML report."""
    
    # Merge: retry results override first-run results
    final_results = dict(all_results)
    retried_tests = set()
    for name, result in retry_results.items():
        if name in final_results:
            retried_tests.add(name)
        final_results[name] = result
    
    # Group by class
    classes = {}
    for name, result in final_results.items():
        cls = result['className']
        if cls not in classes:
            classes[cls] = []
        classes[cls].append(result)
    
    total = len(final_results)
    passed = sum(1 for r in final_results.values() if r['outcome'] == 'Passed')
    failed = sum(1 for r in final_results.values() if r['outcome'] == 'Failed')
    
    # Generate class sections
    class_sections = []
    for cls_name in sorted(classes.keys()):
        tests = classes[cls_name]
        friendly_name = get_friendly_class_name(cls_name)
        cls_total = len(tests)
        cls_passed = sum(1 for t in tests if t['outcome'] == 'Passed')
        cls_failed = sum(1 for t in tests if t['outcome'] == 'Failed')
        
        # Group by outcome
        passed_tests = [t for t in tests if t['outcome'] == 'Passed']
        failed_tests = [t for t in tests if t['outcome'] == 'Failed']
        other_tests = [t for t in tests if t['outcome'] not in ('Passed', 'Failed')]
        
        test_items_html = ''
        
        if failed_tests:
            test_items_html += f'<div class="outcome-group"><div class="group-header"><h3>❌ Failed Tests ({len(failed_tests)})</h3></div><div class="outcome-content">'
            for t in failed_tests:
                test_items_html += generate_test_item(t, traces_dir, base_url, t['testName'] in retried_tests)
            test_items_html += '</div></div>'
        
        if passed_tests:
            test_items_html += f'<div class="outcome-group"><div class="group-header"><h3>✅ Passed Tests ({len(passed_tests)})</h3></div><div class="outcome-content">'
            for t in passed_tests:
                test_items_html += generate_test_item(t, traces_dir, base_url, t['testName'] in retried_tests)
            test_items_html += '</div></div>'
        
        if other_tests:
            test_items_html += f'<div class="outcome-group"><div class="group-header"><h3>🔍 Other Tests ({len(other_tests)})</h3></div><div class="outcome-content">'
            for t in other_tests:
                test_items_html += generate_test_item(t, traces_dir, base_url, t['testName'] in retried_tests)
            test_items_html += '</div></div>'
        
        class_sections.append(f'''
    <section class="class-group">
        <div class="class-header">
            <h2>📋 {escape(friendly_name)}</h2>
            <div class="class-stats">
                <span class="stat stat-total">Total: {cls_total}</span>
                <span class="stat stat-passed">Passed: {cls_passed}</span>
                <span class="stat stat-failed">Failed: {cls_failed}</span>
            </div>
        </div>
        <div class="class-content">
            {test_items_html}
        </div>
    </section>''')
    
    retry_note = ''
    if retried_tests:
        retry_note = f'<p><strong>🔄 Retried:</strong> {len(retried_tests)} test(s) were retried</p>'
    
    html = f'''<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>KISS E2E Test Report</title>
    <link rel="stylesheet" href="https://unpkg.com/simpledotcss@2.3.3/simple.min.css">
    <style>
        [data-outcome=Failed] {{ border-left: 4px solid #dc3545; }}
        [data-outcome=Passed] {{ border-left: 4px solid #28a745; }}
        .class-group {{ margin-bottom: 3rem; border: 2px solid var(--border); border-radius: 10px; overflow: hidden; }}
        .class-header {{ 
            background: linear-gradient(135deg, var(--accent), var(--accent-bg)); 
            color: var(--accent-text);
            padding: 1.5rem; margin: 0; border-bottom: 2px solid var(--border);
        }}
        .class-header h2 {{ margin: 0; color: inherit; }}
        .class-content {{ padding: 1.5rem; }}
        .outcome-group {{ margin-bottom: 2rem; }}
        .group-header {{ background: var(--accent-bg); padding: 1rem; border-radius: 8px; margin-bottom: 1rem; }}
        .test-item {{ background: var(--bg); border: 1px solid var(--border); border-radius: 6px; padding: 1rem; margin-bottom: 0.5rem; }}
        .test-header {{ display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; }}
        .test-header h4 {{ margin: 0; font-size: 1.1rem; }}
        .outcome-badge {{ padding: 0.25rem 0.5rem; border-radius: 4px; font-size: 0.8rem; font-weight: bold; }}
        .outcome-passed {{ background: #d4edda; color: #155724; }}
        .outcome-failed {{ background: #f8d7da; color: #721c24; }}
        .retry-badge {{ background: #fff3cd; color: #856404; padding: 0.25rem 0.5rem; border-radius: 4px; font-size: 0.75rem; margin-left: 0.5rem; }}
        .error-section {{ background: #f8d7da; border: 1px solid #f5c6cb; border-radius: 4px; padding: 0.75rem; margin-top: 0.5rem; }}
        .error-section pre {{ white-space: pre-wrap; word-break: break-all; font-size: 0.8rem; }}
        .class-stats {{ display: flex; gap: 1rem; margin-top: 0.5rem; }}
        .stat {{ padding: 0.25rem 0.75rem; border-radius: 4px; font-size: 0.85rem; font-weight: bold; }}
        .stat-total {{ background: #e2e3e5; color: #383d41; }}
        .stat-passed {{ background: #d4edda; color: #155724; }}
        .stat-failed {{ background: #f8d7da; color: #721c24; }}
        .summary {{ text-align: center; padding: 2rem; background: var(--accent-bg); border-radius: 10px; margin-bottom: 2rem; }}
    </style>
</head>
<body>
    <header>
        <h1>🧪 KISS End-to-End Test Report</h1>
        <p>Generated on {datetime.now().strftime("%Y-%m-%d %H:%M:%S")}</p>
    </header>
    
    <main>
        <section class="summary">
            <h2>📊 Test Summary</h2>
            <p><strong>Total:</strong> {total} | <strong style="color: #28a745;">Passed:</strong> {passed} | <strong style="color: #dc3545;">Failed:</strong> {failed}</p>
            <p><strong>Test Classes:</strong> {len(classes)}</p>
            {retry_note}
        </section>

        {"".join(class_sections)}
    </main>
</body>
</html>'''
    
    return html


def generate_test_item(test, traces_dir, base_url, was_retried):
    """Generate HTML for a single test item."""
    outcome = test['outcome']
    display_name = test['testName']
    
    # Badge
    badge_class = 'outcome-passed' if outcome == 'Passed' else 'outcome-failed'
    badge_text = outcome
    
    retry_html = '<span class="retry-badge">🔄 Retried</span>' if was_retried else ''
    
    # Trace link
    trace_html = ''
    trace_file = find_trace_file(traces_dir, test.get('fullClassName', '') + '.' + test['methodName'])
    if not trace_file:
        trace_file = find_trace_file(traces_dir, test['testName'])
    if trace_file:
        trace_url = f"https://trace.playwright.dev/?trace={base_url}/{trace_file}"
        trace_html = f'<p><strong>Trace:</strong> <a href="{escape(trace_url)}">View Playwright Trace</a></p>'
    
    # Error section
    error_html = ''
    if test['errorMessage']:
        error_html = f'''
        <div class="error-section">
            <strong>Error:</strong>
            <pre>{escape(test["errorMessage"][:2000])}</pre>
        </div>'''
    
    return f'''
        <div class="test-item" data-outcome="{outcome}">
            <div class="test-header">
                <h4>{escape(display_name)}</h4>
                <span class="outcome-badge {badge_class}">{badge_text}</span>
                {retry_html}
            </div>
            {trace_html}
            {error_html}
        </div>'''


def main():
    test_results_dir = sys.argv[1] if len(sys.argv) > 1 else './TestResults'
    traces_dir = sys.argv[2] if len(sys.argv) > 2 else './bin/Debug/net8.0/playwright-traces'
    base_url = sys.argv[3] if len(sys.argv) > 3 else 'https://klantinteractie-servicesysteem.github.io/KISS-frontend'
    
    # Find all TRX files
    trx_files = sorted(Path(test_results_dir).rglob('*.trx'))
    
    if not trx_files:
        print("⚠️ No TRX files found, skipping consolidated report generation")
        return
    
    print(f"📄 Found {len(trx_files)} TRX file(s):")
    for f in trx_files:
        print(f"   - {f}")
    
    # Parse first-run TRX (contains all tests)
    first_run_results = {}
    retry_results = {}
    
    for trx_file in trx_files:
        results = parse_trx(str(trx_file))
        filename = trx_file.name.lower()
        
        if 'retry' in filename:
            retry_results.update(results)
            print(f"   📋 Retry TRX: {len(results)} test(s)")
        else:
            first_run_results.update(results)
            print(f"   📋 First-run TRX: {len(results)} test(s)")
    
    print(f"\n📊 First run: {len(first_run_results)} tests")
    print(f"📊 Retry: {len(retry_results)} tests")
    
    # Generate consolidated report
    html = generate_html(first_run_results, retry_results, traces_dir, base_url)
    
    # Write to traces dir
    os.makedirs(traces_dir, exist_ok=True)
    output_path = os.path.join(traces_dir, 'index.html')
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(html)
    
    total = len(first_run_results)  # Use first run as base for total
    # Merge for final count
    final = dict(first_run_results)
    final.update(retry_results)
    passed = sum(1 for r in final.values() if r['outcome'] == 'Passed')
    failed = sum(1 for r in final.values() if r['outcome'] == 'Failed')
    
    print(f"\n✅ Consolidated report generated: {output_path}")
    print(f"   Total: {len(final)} | Passed: {passed} | Failed: {failed} | Retried: {len(retry_results)}")


if __name__ == '__main__':
    main()
