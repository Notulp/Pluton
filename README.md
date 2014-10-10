Pluton
======

Pluton is a mod for the active development branch of the survival sandbox game Rust

######Compatible server version: <b>1309, 1370, 1394</b>

Get started:
------------
1. Grab the files.
2. Download the server files from steamcmd with:

  ```
  app_update 258550 validate
  ```
3. Copy these files into the ref folder next to the 'Pluton' and 'Pluton.Patcher' folders

  ```
  Assembly-CSharp.dll
  Facepunch.dll
  UnityEngine.dll
  ```
4. Compile Pluton and Pluton.Patcher
5. Copy Pluton.Patcher and Mono.Cecil from Pluton.Patcher/bin/debug to the downloaded server's managed folder
6. Copy Pluton.dll from Pluton/bin/debug to the downloaded server's managed folder
7. Copy System.Reactive.dll, Jint.dll and IronPython.Deps.dll from Pluton/ref to the downloaded server's managed folder
8. Run the Pluton.Patcher.exe inside the managed folder
9. Copy Pluton.cfg from Pluton/ to %serverroot%/server/_your_server's_identity_/Pluton/
10. Have fun!

Q.A.
----

Q: Where the plugins go?

A: Into: %publicfolder%/Plugins/_Plugin'sName_/_Plugin'sName.py_


Q: Which is the public folder?

A: That's: %identityfolder%/Pluton.


Q: Uhm, identityfolder?

A: %serverroot%/server/_your_server's_identity_


Our sponsor:
------------

www.streamline-servers.com
