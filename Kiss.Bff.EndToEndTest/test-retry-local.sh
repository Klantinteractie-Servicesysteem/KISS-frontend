#!/bin/bash

cd "$(dirname "$0")"

echo "=== Building project ==="
dotnet build

echo ""
echo "=== Running tests (first run) ==="
dotnet test --logger "trx;LogFileName=first-run.trx" --results-directory ./TestResults --verbosity normal &

TEST_PID=$!

# Wait for 30 minutes (1800 seconds) or until test completes
TIMEOUT=1800
ELAPSED=0
while kill -0 $TEST_PID 2>/dev/null && [ $ELAPSED -lt $TIMEOUT ]; do
  sleep 10
  ELAPSED=$((ELAPSED + 10))
  if [ $((ELAPSED % 300)) -eq 0 ]; then
    echo "⏱️  Still running... ($((ELAPSED / 60)) minutes elapsed)"
  fi
done

# Check if process is still running (timed out)
if kill -0 $TEST_PID 2>/dev/null; then
  echo ""
  echo "❌ Tests timed out after 30 minutes! Killing process..."
  kill -9 $TEST_PID 2>/dev/null
  wait $TEST_PID 2>/dev/null
  FIRST_RUN_EXIT=1
else
  # Process completed normally
  wait $TEST_PID
  FIRST_RUN_EXIT=$?
fi

echo ""
echo "⚠️ Looking for TRX files..."

# Find the TRX file (it might be in a subdirectory)
TRX_FILE=$(find ./TestResults -name "first-run.trx" -o -name "*.trx" | head -1)

if [ -z "$TRX_FILE" ]; then
  echo "❌ No TRX file found in TestResults directory"
  echo "TestResults contents:"
  ls -la ./TestResults/
  exit 1
fi

echo "✅ TRX file found at: $TRX_FILE"

if [ $FIRST_RUN_EXIT -eq 0 ]; then
  echo "✅ All tests passed!"
  exit 0
fi

echo ""

# Check if xmlstarlet is installed (macOS)
if ! command -v xmlstarlet &> /dev/null; then
  echo "Installing xmlstarlet..."
  brew install xmlstarlet
fi

echo "=== Parsing failed tests from: $TRX_FILE ==="
FAILED_TESTS=$(xmlstarlet sel -t -m "//ns:UnitTestResult[@outcome='Failed']" -v "@testName" -n \
  -N ns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010" \
  "$TRX_FILE" 2>/dev/null | sort -u)

if [ -z "$FAILED_TESTS" ]; then
  echo "❌ No failed tests found in TRX (but dotnet test exited with error code $FIRST_RUN_EXIT)"
  
  # Check for other outcomes
  echo ""
  echo "Checking TRX for test outcomes..."
  xmlstarlet sel -t -m "//ns:UnitTestResult" -v "concat(@testName, ' - ', @outcome)" -n \
    -N ns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010" \
    "$TRX_FILE" 2>/dev/null | tail -20
  
  exit 1
fi

FAILED_COUNT=$(echo "$FAILED_TESTS" | wc -l | tr -d ' ')
echo "Found $FAILED_COUNT failed test(s):"
echo "$FAILED_TESTS"
echo ""

RETRY_PASSED=0
RETRY_FAILED=0

echo "=== Retrying failed tests (10-minute timeout per test) ==="
while IFS= read -r test_name; do
  if [ -n "$test_name" ]; then
    echo "🔄 Retrying: $test_name"
    
    SAFE_NAME=$(echo "$test_name" | sed 's/[^a-zA-Z0-9]/_/g')
    
    # Run retry in background with timeout
    dotnet test \
      --no-build \
      --filter "FullyQualifiedName~${test_name}" \
      --logger "trx;LogFileName=retry-${SAFE_NAME}.trx" \
      --results-directory ./TestResults \
      --verbosity quiet &
    
    RETRY_PID=$!
    
    # Wait for 10 minutes or until test completes
    RETRY_TIMEOUT=600
    RETRY_ELAPSED=0
    while kill -0 $RETRY_PID 2>/dev/null && [ $RETRY_ELAPSED -lt $RETRY_TIMEOUT ]; do
      sleep 5
      RETRY_ELAPSED=$((RETRY_ELAPSED + 5))
    done
    
    # Check if process is still running (timed out)
    if kill -0 $RETRY_PID 2>/dev/null; then
      echo "   ⏱️ TIMEOUT (hung after 10 minutes)"
      kill -9 $RETRY_PID 2>/dev/null
      wait $RETRY_PID 2>/dev/null
      ((RETRY_FAILED++))
    else
      wait $RETRY_PID
      RETRY_EXIT=$?
      
      if [ $RETRY_EXIT -eq 0 ]; then
        echo "   ✅ PASSED on retry"
        ((RETRY_PASSED++))
      else
        echo "   ❌ Still FAILED"
        ((RETRY_FAILED++))
      fi
    fi
    echo ""
  fi
done <<< "$FAILED_TESTS"

echo "═══════════════════════════════════════════"
echo "📊 Summary"
echo "═══════════════════════════════════════════"
echo "Total failed on first run: $FAILED_COUNT"
echo "Passed on retry:          $RETRY_PASSED"
echo "Still failed:             $RETRY_FAILED"
echo "═══════════════════════════════════════════"

if [ $RETRY_FAILED -gt 0 ]; then
  echo ""
  echo "❌ $RETRY_FAILED test(s) still failing after retry"
  exit 1
else
  echo ""
  echo "✅ All tests passed after retry!"
  exit 0
fi