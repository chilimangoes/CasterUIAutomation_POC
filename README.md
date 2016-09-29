
CasterUIAutomation_POC
===============================

POC project for experimentnig with using Microsoft's UI Automation API to develop a helper service to facilitate Caster interaction with system UI elements (e.g. task bar, menus, etc.) and programs running as administrator, as discussed in [this issue](https://github.com/synkarius/caster/issues/114) and in [this conversation on Gitter](https://gitter.im/synkarius/caster?at=57df75b9aabc89857fb32558). We can also use this README and the repository's issues to gather links to relevant documentation and articles, collect ideas, etc.

Getting Started
-------------------------------

* Install the [Windows SDK](http://go.microsoft.com/fwlink/p/?LinkID=271979).
* Install Visual Studio if you don't already have it. You can download Visual Studio Community edition for free [here](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx).
* Open the `CasterUIAutomation.sln' file. This is a skeleton project that we can use for prototyping.


Code signing
--------------

If you want to test this POC app interacting with applications that are *not* running as administrator, you can do so by simply running it from within Visual Studio, e.g. by pressing F5. However, in order to interact with applications that are running as administrator, it needs to be set up as an accessibility app. Among other requirements, this means that it needs to be code signed and installed to the Program Files directory.

### Generating Self-Signed Certificates

Before code signing the application, you first need to generate self-signed code signing certificates and a PFX file. To do this:

1) Run the Visual Studio command prompt as administrator, by clicking the start menu, typing developer command prompt, right click the icon and select run as administrator.

2) Navigate to the 'certs' folder in the Visual Studio Developer Command Prompt and run the 'MAKE_CERTS.bat' script.

3) Follow the prompts to generate certificate files for code signing.

IMPORTANT: At the end of the certificate version process, a warning like the following will be displayed. *Please* take this warning seriously and protect your private key and the password(s) used to protect it and your PFX file.

```
===============================================================

*** WARNING *** WARNING *** WARNING *** WARNING *** WARNING ***

The public certificate file 'ChilimangoesCA.cer' has been added
to your root certificate store as a trusted Certificate 
Authority. This means that your computer will automatically
trust ANY certificates issued using your private key file
'ChilimangoesCA.pvk'.

You should store these certificates in a SAFE and PROTECTED 
location. You should store the password used to protect the 
private key in a (preferably separate) safe location, like 
an encrypted password database.

Failure to protect your private key and password could open up
your computer to extremely dangerous and targeted attacks, such
as Man In The Middle attacks and malware that your computer 
thinks is from a trusted source.

If at any time you want to remove the certificate from your 
trusted root store, run the command 'certmgr.exe' from a
Visual Studio Command Prompt window and delete the certificate
named 'Chilimangoes CA' from the 'Intermediate Certification 
Authorities' and 'Trusted Root Certification Authorities' tabs.

*** WARNING *** WARNING *** WARNING *** WARNING *** WARNING ***

===============================================================
```

#### Signing The Assembly

To code sign your application:

1) After building the app, copy the PFX file created above into the output directory (generally either 'CasterUIAutomation\bin\Debug' or 'CasterUIAutomation\bin\Release', depending on what configuration you built).

2) Rename the PFX file 'CasterSPC.PFX'.

3) Open the Visual Studio Developer Command Prompt. You do *not* need to run it as administrator for this step.

4) In the Developer Command Prompt, CD into the output folder identified in step 1.

5) Run the 'SIGN.bat' script and follow the prompts.

You should see something like the following output after the code signing is completed:

```
The following certificate was selected:
    Issued to: Chilimangoes SPC
    Issued by: Chilimangoes CA
    Expires:   Sat Dec 31 16:59:59 2039
    SHA1 hash: EEA1802EC32AE0C0DB7C6F304B33377F6850C942

Done Adding Additional Store
Successfully signed and timestamped: CasterUIAutomation.exe

Number of files successfully Signed: 1
Number of warnings: 0
Number of errors: 0
```

After signing the application, you can then copy the application files from the output folder to a directory of your choosing in the 'C:\Program Files (x86)' folder and run it.

NOTE: If you run the application and get an error stating "A referral was returned from the server", this probably means that either the application isn't signed correctly, or that your public cert has not been added to the list of trusted Certification Authorities (this should be taken care of by the script you ran under 'Generating Self-Signed Certificates', but you may need to do it manually if using the same certs on a different machine).


Resources
-------------------------------

* [Main UI Automation Reference](https://msdn.microsoft.com/en-us/library/windows/desktop/ee684009(v=vs.85).aspx)
* [An Introduction to UI Automation](http://blog.functionalfun.net/2009/06/introduction-to-ui-automation-with.html)

#### Security

* [Security Considerations for Assistive Technologies](https://msdn.microsoft.com/en-us/library/windows/desktop/ee671610.aspx)
* [Code Signing for Developers - An Authenticode How-To](http://www.tech-pro.net/code-signing-for-developers.html)
* [Code Signing With Self-Signed Cert](http://stackoverflow.com/questions/84847/how-do-i-create-a-self-signed-certificate-for-code-signing-on-windows)
* [Ease of Access – Assistive Technology Registration](https://msdn.microsoft.com/en-us/library/windows/desktop/bb879984.aspx)
* [Code Signing Cert for OSS Projects (14 EUR / yr)](https://www.certum.eu/certum/cert,offer_en_open_source_cs.xml)

#### Grids Over Popups and Menus

* [Stack Overflow: Windows 8 Layered Windows Over Metro Apps](https://stackoverflow.com/questions/11232450/windows-8-layered-windows-over-metro-apps/13497452#13497452)


#### Manipulating Text

Might be useful later on for more advanced scenarios.

* [UI Automation Text Patterns](https://msdn.microsoft.com/en-us/library/ms752355(v=vs.110).aspx)
* [Find and Highlight Text Using UI Automation](https://msdn.microsoft.com/en-us/library/ms752287(v=vs.110).aspx)
* [Working with Text Ranges](https://msdn.microsoft.com/en-us/library/windows/desktop/hh298427(v=vs.85).aspx)


