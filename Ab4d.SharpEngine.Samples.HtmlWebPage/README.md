# Simple HTML and JavaScript web page with Ab4d.SharpEngine

This project shows how to create a simple web page with Ab4d.SharpEngine that uses only HTML and JavaScript 
to start the WebAssembly that shows 3D graphics.


## Starting web server

A simple Python script is used to create a very simple web server (defined in `serve.py`). It serves all the files from the wwwroot folder.

Before the web server is started, the two bat scripts (`start_debug_local_web_server.bat` and `start_publish_local_web_server.bat`) from this folder copy the output folder to the wwwroot folder. First, static files (index.html, sharp-engine.js and favicon) are copied from the Ab4d.SharpEngine.Samples.AspNetCoreApp project. Then, WebAssembly files are copied from Ab4d.SharpEngine.Samples.BlazorWebAssembly output folder (debug or release folder).

You can use the `compile_debug_version.bat` and `compile_publish_version.bat` to compile the Ab4d.SharpEngine.Samples.BlazorWebAssembly and prepare the files in the output folders.

To see how to use **Asp.Net Core** project to serve as a web server, see the [Ab4d.SharpEngine.Samples.AspNetCoreApp project](../Ab4d.SharpEngine.Samples.AspNetCoreApp/README.md).

The following sections are the same as for the Asp.Net Core project.


## Create 3D scene with Ab4d.SharpEngine library

The 3D scene is created in the Ab4d.SharpEngine.Samples.WebAssemblyDemo project. This is a .Net project with TargetFramework set to `net9.0-browser` and RuntimeIdentifier set to `browser-wasm`. The project does not require any Blazor references. The project is compiled into WebAssembly (wasm) files that can be used on any web page (no need for Blazor).

To show 3D graphics, the Ab4d.SharpEngine.Samples.WebAssemblyDemo project references the Ab4d.SharpEngine.Web library. The WebAssemblyDemo project defines the 3D scene by adding SceneNodes objects to the Scene.RootNode. This is done in the `SharpEngineTest.cs` file.

To communicate with the web page, this project exports a few methods to JavaScript. The exported methods are defined in the `JavaScriptInterop.cs` file. For example, `ToggleCameraRotationJSExport` method that is defined in JavaScriptInterop.cs is called from the following html:
```
<a href="javascript:toggleCameraRotation();">Toggle camera rotation</a><br />
```

The WebAssemblyDemo project also subscribes to mouse and resize events on the canvas element. This is done by using the standard `CanvasInterop.cs` and `sharp-engine.js` files (those two files are also used in the Ab4d.SharpEngine.Samples.BlazorWebAssembly project - the Blazor WebAssembly project that is the standard sample for Ab4d.SharpEngine in the browser).


## Startup

In the standard Ab4d.SharpEngine.Samples.BlazorWebAssembly project,  Blazor handles the startup procedure. There, we only need to override the `OnAfterRender` or `OnAfterRenderAsync` method to start initialization of the SharpEngine.

When Blazor is not used, then we need to initialize the SharpEngine from JavaScript. This is done in the script block in the index.html file.

There, the code first initializes the .NET WebAssembly runtime by loading the `./_framework/dotnet.js`. Then the exported functions from .Net are retrieved and after that we can call the `InitSharpEngineJSExport` method that initializes the SharpEngine and generates the initial 3D scene.


## Debugging

When the web server is started by using Asp.Net Core, a python script or in some other way, it is not possible (at least to my knowledge) to debug the c# code that was used to generate the WebAssembly files.

Therefore, it is recommended to create a **Blazor WebAssembly web page** that uses linked .cs files from the main project (Ab4d.SharpEngine.Samples.WebAssemblyDemo) and can be started as a Blazor WebAssembly web page, allowing for full code debugging.

In this solution, this is done by the **Ab4d.SharpEngine.Samples.BlazorWebAssemblyTesterApp** project.