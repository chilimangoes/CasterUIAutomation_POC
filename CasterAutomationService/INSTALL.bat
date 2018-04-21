@echo off

:: Make sure we are running out of the directory where the batch file is being run from
cd %~dp0

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe "CasterAutomationService.exe"

if ERRORLEVEL 1 goto error

Echo Installation complete, press enter key to continue
pause
exit

:error
echo There was a problem, press enter key to exit
pause