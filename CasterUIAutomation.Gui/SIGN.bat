echo off
cls

set PFX_FILE=CasterSPC.pfx
set APP_FILE=CasterUIAutomation.exe

:: Check to see if code signing tools are available...
for /f %%i in ('signtool') do set OUTPUT=%%i
if "%OUTPUT%"=="" (
  echo.
  echo.
  echo ==============================================================
  echo.
  echo NOTE: This script uses several command line tools that are
  echo       normally NOT in your PATH. As such, it should be run
  echo       from the Visual Studio Command Prompt rather than the
  echo       standard Command Prompt (CMD^). To do this, click the
  echo       Start Menu, type 'Visual Studio Command Prompt', 
  echo       right-click on the program, and choose 'Run as 
  echo       Administrator'. (Note: In some newer versions of Visual
  echo       Studio, it may be called 'Developer Command Prompt for
  echo       VS2015' or similar^)
  echo.
  echo       Then navigate to this folder and run this script again.
  echo.
  echo ==============================================================
  echo.
  echo.
  pause
  goto:EOF
)

if NOT EXIST %APP_FILE% (
  echo.
  echo ERROR: Unable to find %APP_FILE%
  echo.
  echo This script should be run from within the compiled output directory.
  echo See the Code Signing section in the README file for
  echo details.
  echo.
  pause
  goto:EOF
)

if NOT EXIST %PFX_FILE% (
  echo.
  echo ERROR: Unable to find '%PFX_FILE%' file
  echo.
  echo Please add the '%PFX_FILE%' file to the current folder and run the
  echo script again. See the Code Signing section in the README file for
  echo details.
  echo.
  pause
  goto:EOF
)

echo --------------------------------------------------------------
echo.
echo Code signing the application assembly...
echo.
echo Enter the password you used to protect your PFX file.
echo This may or may not be the same as the password used to protect 
echo your private key.
echo.
set /P PFX_PWD=PFX Password: 

signtool sign /v /f "%PFX_FILE%" /t http://timestamp.verisign.com/scripts/timstamp.dll /p %PFX_PWD% "%APP_FILE%"
echo.

echo DONE
echo.

pause

echo Cleaning up PFX and BAT files...
del /f %PFX_FILE%
del /f SIGN.bat
