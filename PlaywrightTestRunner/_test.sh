#!/bin/bash
set -e

# test project's directory name
TestProjectDirName="BrowserWasmDemo"
export TestProjectDirName="$TestProjectDirName"

# The page that serves the unit tests
UnitTestPage="tests"
export UnitTestPage="$UnitTestPage"

# save the original directory
OriginalDir=$(pwd)

# save this script's directory
ScriptDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# switch to the script's directory
cd "$ScriptDir"

# TestRunner publishes the test project itself (dotnet publish -c Release) before serving it

echo "Preparing tests"
dotnet restore

echo "Testing Chromium"
dotnet test ./PlaywrightTestRunner.csproj --no-restore -- Playwright.BrowserName=chromium

echo "Testing Firefox"
dotnet test ./PlaywrightTestRunner.csproj --no-restore -- Playwright.BrowserName=firefox

echo "Success"

# return to original directory
cd "$OriginalDir"
