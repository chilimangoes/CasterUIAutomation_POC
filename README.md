
CasterUIAutomation_POC
===============================

POC project for experimentnig with using Microsoft's UI Automation API to develop a helper service to facilitate Caster interaction with system UI elements (e.g. task bar, menus, etc.) and programs running as administrator, as discussed in [this issue](https://github.com/synkarius/caster/issues/114) and in [this conversation on Gitter](https://gitter.im/synkarius/caster?at=57df75b9aabc89857fb32558). We can also use this README and the repository's issues to gather links to relevant documentation and articles, collect ideas, etc.

Getting Started
-------------------------------

* Install the [Windows SDK](http://go.microsoft.com/fwlink/p/?LinkID=271979).
* Install Visual Studio if you don't already have it. You can download Visual Studio Community edition for free [here](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx).
* Open the `CasterUIAutomation.sln' file. This is a skeleton project that we can use for prototyping.


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


