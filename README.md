Pluton
======

<a href='http://jenkins.pluton-team.org/job/Pluton/'>
  <img src='http://jenkins.pluton-team.org/buildStatus/icon?job=Pluton'></a>
<a href='https://github.com/Notulp/Pluton/blob/master/LICENSE'>
  <img src='https://img.shields.io/github/license/notulp/pluton.svg'></a>
<a href='https://github.com/Notulp/Pluton/releases/latest'>
  <img src='https://img.shields.io/github/release/notulp/pluton.svg'></a>
<a href='https://github.com/Notulp/Pluton/network'>
  <img src='https://img.shields.io/github/forks/notulp/pluton.svg'></a>
<a href='https://github.com/Notulp/Pluton/stargazers'>
  <img src='https://img.shields.io/github/stars/notulp/pluton.svg'></a>
<a href='https://github.com/Notulp/Pluton/issues'>
  <img src='https://img.shields.io/github/issues/notulp/pluton.svg'></a>

Pluton is a server mod for the active branch of the survival sandbox game Rust.

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
  Assembly-CSharp-firstpass.dll
  UnityEngine.dll
  ```
4. Compile Pluton and Pluton.Patcher
5. Copy Pluton.Patcher and Mono.Cecil from Pluton.Patcher/bin/debug to the downloaded server's managed folder
6. Copy Pluton.dll from Pluton/bin/debug to the downloaded server's managed folder
7. You now 2 options. Copy all files from Pluton/ref to Managed folder or selectively choose the files you want based on what you will be using:

  ```
  Required = System.Reactive.dll, System.Threading.dll, and Mono.Cecil.dll
  Python = IronPython.Deps.dll
  Javascript = Jint.dll
  CSharp = mcs.exe
  Lua = MoonSharp.Interpreter.dll
  ```
8. Run the Pluton.Patcher.exe inside the managed folder
9. Copy Pluton.cfg from Pluton/ to %serverroot%/server/_your_server's_identity_/Pluton/
10. Have fun!

Q.A.
----

Q: Where the plugins go?

A: Into: %publicfolder%/Plugins/_Plugin'sName_/_Plugin'sName.**_ (** is the plugin's extenison, .py for python, .js for javascript)


Q: Which is the public folder?

A: That's: %identityfolder%/Pluton.


Q: Uhm, identityfolder?

A: %serverroot%/server/_your_server's_identity_


Our sponsors:
------------

[Streamlines Servers](http://www.streamline-servers.com)

[FPS Players](http://fpsplayers.com)

Links:
------

[Homepage](http://pluton-team.org), [Forum](http://forum.pluton-team.org), [Wiki](http://forum.pluton-team.org/wiki/index/), [IRC](http://webchat.freenode.net?channels=%23pluton)
