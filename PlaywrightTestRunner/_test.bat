@echo off

REM test project's directory name
SET "TestProjectDirName=BrowserWasmDemo"

REM The page that serves the unit tests
SET "UnitTestPage=tests"

REM save the original directory
SET "OriginalDir=%CD%"

REM switch to the script's directory
cd "%~dp0"

REM TestRunner publishes the test project itself (dotnet publish -c Release) before serving it
echo Preparing tests
dotnet restore || goto :ERROR

echo Testing Chromium
dotnet test ./PlaywrightTestRunner.csproj --no-restore -- Playwright.BrowserName=chromium || goto :ERROR

echo Testing Firefox
dotnet test ./PlaywrightTestRunner.csproj --no-restore -- Playwright.BrowserName=firefox || goto :ERROR

echo Success
REM return to original directory
CD /D "%OriginalDir%"
exit /b 0

:ERROR
echo Failed
REM return to original directory
CD /D "%OriginalDir%"
exit /b 1
