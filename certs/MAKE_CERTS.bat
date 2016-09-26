echo off
cls

:: Adapted from:
:: http://stackoverflow.com/questions/84847/how-do-i-create-a-self-signed-certificate-for-code-signing-on-windows

echo.
echo This script will generate self-signed public/private keys and
echo a PFX file suitable for code signing assemblies. It will also
echo add the public key to your computer's list of trusted root
echo Certificate Authorities, which will allow you to run 
echo applications that you sign on your computer as being from a 
echo trusted publisher (i.e. you). For this reason, you should take
echo care to store your certificate files and passwords in a SECURE
echo location after they are generated.

:: Check to see if code signing tools are available...
for /f %%i in ('makecert') do set OUTPUT=%%i
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

echo.
echo --------------------------------------------------------------
echo.
echo Enter a name to prefix your certificate files. For example, if
echo you enter 'BobSmith', then your files would be named 
echo 'BobSmithCA.cer', 'BobSmithSPC.pfx', etc.
echo.
echo NOTE: When entering the prefix, do not use spaces.
echo.
set /P CERT_NAME=Certificate Prefix: 
echo.

echo --------------------------------------------------------------
echo    Certificate Authority (CA) Files
echo --------------------------------------------------------------
echo.
echo Creating private (PVK) and public (CER) CA files...
echo.
echo NOTE: You will be prompted to enter a password which will be used
echo       to protect your private key. You should enter a strong
echo       password and store the password in a secure location, such
echo       as in a password database.
echo.
pause
makecert -r -pe -n "CN=%CERT_NAME% CA" -ss CA -sr CurrentUser -a sha256 -cy authority -sky signature -sv "%CERT_NAME%CA.pvk" "%CERT_NAME%CA.cer"
echo.

echo --------------------------------------------------------------
echo.
echo Adding public CA certificate to trusted root store...
echo.
pause
certutil -user -addstore Root "%CERT_NAME%CA.cer"
:: NOTE: to verify this step, run 'certmgr.exe' and check 
::       the Trusted Root Certification Authorities tab for 
::       a cert named '%CERT_NAME% CA'.
echo.


echo --------------------------------------------------------------
echo    Code Signing (SPC) Files
echo --------------------------------------------------------------
echo.
echo Creating private (PVK) and public (CER) SPC files...
echo.
echo NOTE: You will be prompted to enter the password for the private
echo       key that you created in the first step.
echo.
pause
makecert -pe -n "CN=%CERT_NAME% SPC" -a sha256 -cy end -sky signature -ic "%CERT_NAME%CA.cer" -iv "%CERT_NAME%CA.pvk" -sv "%CERT_NAME%SPC.pvk" "%CERT_NAME%SPC.cer"
echo.

echo --------------------------------------------------------------
echo.
echo Creating private (PVK) and public (CER) SPC files...
echo.
echo Enter a password to protect your PFX file for code signing.
echo This can be the same as the password used to protect your 
echo private key, but it does not have to be.
echo.
set /P PFX_PWD=PFX Password: 
pvk2pfx -pvk "%CERT_NAME%SPC.pvk" -spc "%CERT_NAME%SPC.cer" -pfx "%CERT_NAME%SPC.pfx"
set PFX_PWD=

echo.
echo DONE
echo.
echo.
echo.
echo ===============================================================
echo.
echo *** WARNING *** WARNING *** WARNING *** WARNING *** WARNING ***
echo.
echo The public certificate file '%CERT_NAME%CA.cer' has been added
echo to your root certificate store as a trusted Certificate 
echo Authority. This means that your computer will automatically
echo trust ANY certificates issued using your private key file
echo '%CERT_NAME%CA.pvk'.
echo.
echo You should store these certificates in a SAFE and PROTECTED 
echo location. You should store the password used to protect the 
echo private key in a (preferably separate) safe location, like 
echo an encrypted password database.
echo.
echo Failure to protect your private key and password could open up
echo your computer to extremely dangerous and targeted attacks, such
echo as Man In The Middle attacks and malware that your computer 
echo thinks is from a trusted source.
echo.
echo If at any time you want to remove the certificate from your 
echo trusted root store, run the command 'certmgr.exe' from a
echo Visual Studio Command Prompt window and delete the certificate
echo named '%CERT_NAME% CA' from the 'Intermediate Certification 
echo Authorities' and 'Trusted Root Certification Authorities' tabs.
echo.
echo *** WARNING *** WARNING *** WARNING *** WARNING *** WARNING ***
echo.
echo ===============================================================
echo.


