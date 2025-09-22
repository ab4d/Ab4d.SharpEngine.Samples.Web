# Ab4d.SharpEngine.Samples.Web

![Ab4d.SharpEngine logo](doc/sharp-engine-logo.png)

Welcome to the Browser samples for Ab4d.SharpEngine.

**Ab4d.SharpEngine is a cross-platform 3D rendering engine for desktop, mobile and browser .Net applications.**

To check the Ab4d.SharpEngine for desktop and mobile devices see the [Ab4d.SharpEngine.Samples on GitHub](https://github.com/ab4d/Ab4d.SharpEngine.Samples).

> [!IMPORTANT]
> Ab4d.SharpEngine for browser (Ab4d.SharpEngine.Web assembly) is in beta and is not yet ready for production.

### Quick start guide

To start this samples project, just open it in any .Net IDE and run it. You can also start it from CLI by using `dotnet run .` or similar command.

To use the Ab4d.SharpEngine.Samples.Web library **in your own project**, follow those steps:
- Create a new "Blazor WebAssembly Standalone App" (use .Net 9 or newer).
- Add reference to Ab4d.SharpEngine.Web NuGet package.
- Copy the following files from this samples project to your project:
    - `CanvasInterop.cs` (copy to root folder of your project)
    - `wwwroot/sharp-engine.js` (copy to wwwroot folder)
    - `Native/libEGL.c` (create a new Native folder in your project and copy the libEGL.c file there)
- Open Blazor's csproj file and add the following:
    - Into the first `PropertyGroup`:
      ```
      <!-- unsafe code is required to use JSExport in CanvasInterop -->
      <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
     
      <!-- The following emscripten flags are required for Ab4d.SharpEngine to use WebGL from the browser -->
      <EmccFlags>-lGL -s FULL_ES3=1 -sMAX_WEBGL_VERSION=2</EmccFlags>

      <!-- Blazor WebAssembly supports SIMD instructions (when supported by the browser), so it is recommended to enbale that -->
      <WasmEnableSIMD>true</WasmEnableSIMD>
      ```
    - Optionally you can set additional properties for DEBUG and RELEASE build. See the csproj file in this sample for example PropertyGroup blocks.
    - Add the following two ItemGroups to the csproj file:
      ```
      <ItemGroup>
        <!--The following NativeFileReference is required for Ab4d.SharpEngine.Web to use WebGL from the browser -->
        <NativeFileReference Include="Native/libEGL.c" ScanForPInvokes="true" />
      </ItemGroup>
      
      <ItemGroup>
        <!--The following javascript file is required for CanvasInterop class to be able to communicate with the browser -->
        <WasmExtraFilesToDeploy Include="sharp-engine.js" />
      </ItemGroup>      
      ```
- Open the razor page that will host the 3D scene and add the following:
    - Add the following usings and inject to the start of the razor file:
      ```
      @using System.Numerics
      @using Ab4d.SharpEngine
      @using Ab4d.SharpEngine.Common
      @using Ab4d.SharpEngine.Utilities
      @using Ab4d.SharpEngine.WebGL
      @inject IJSRuntime JS      
      ```
    - Define the canvas element in your razor file. For example, add the following to Home.razor:
      `<canvas id="webGLCanvas" style="width: 70%; height: 500px"></canvas>`
      
> [!NOTE]
> The id of the canvas will be used in the code to connect to the canvas element.
    
> [!NOTE]
> Set the size of the canvas by setting width and height in the style and not by setting width and height properties. Setting width and height properties would set the size of the back-buffers and is not recommended for web.

    - Override the OnInitializedAsync method and in the method's body call the static `CanvasInterop.InitializeInterop` method. 
    - If the `CanvasInterop.IsInteropInitialized` value is true, then you can create an instance of CanvasInterop (pass the canvas id to the constructor) and then create the WebGLDevice, Scene and SceneView objects. For example:
      ```
      @code
      {
          protected override async Task OnInitializedAsync()
          {
              //Console.WriteLine($"Hello from dotnet!");

              Log.LogLevel = LogLevels.All;
              Log.WriteSimplifiedLogMessage = true;
              Log.IsLoggingToConsole = true; // This will write to Browser's Console


              // Initialize the browser interop (load sharp-engine.js file and from javascript get access to exported methods in the CanvasInterop class)
              await CanvasInterop.InitializeInterop();

              if (!CanvasInterop.IsInteropInitialized)
                  return; // Cannot load sharp-engine.js or initialize the interop (see Browser's Console for more info)


              // After global browser interop was initialized, 
              // create the CanvasInterop that will connect Blazor app with the canvas in the browser's DOM
              // canvasId is the id of the canvas that shows WebGL graphics (see the html part of the code above)
              var canvasInterop = new CanvasInterop(canvasId: "webGLCanvas"); // if we do not need pointer events, we can also add: subscribePointerEvents: false

              // Try to connect to the canvas and get the WebGL context.
              // We can also skip this call. In this case InitWebGL will be called from the Scene or SceneView Initialized method.
              // But by calling this by ourselves, we can immediately check if the WebGL context is available (checking IsWebGLInitialized).
              canvasInterop.InitWebGL();

              if (!canvasInterop.IsWebGLInitialized)
                  return; // Skip creating Scene and SceneView objects; error message was already written to console in the InitWebGL method


              var gpuDevice = WebGLDevice.Create(canvasInterop); // We can also pass an EngineCreateOptions object to the Create method

              if (!gpuDevice.IsInitialized)
                  return; // Blazor cannot use the WebGL context

              var scene = new Scene(gpuDevice, "MainScene"); // Create Scene object and also initialize it with the gpuDevice.
              var sceneView = new SceneView(scene, "MainSceneView"); // SceneView will be automatically initialized and its initial size will be set.


              // You can also create the Scene and SceneView objects (and also add SceneNodes to the RootNode)
              // before initializing the WebGL device:
              //
              // var scene = new Scene("MainScene");
              // var sceneView = new SceneView(scene, "MainSceneView");

              // Later (even after adding some SceneNodes to the Scene), you can initialize the Scene and SceneView,
              // by one of the following options:

              // 1:
              //sceneView.Initialize(canvasInterop); // This will also call WebGLDevice.Create and will also initialize the Scene

              // 2:
              // var gpuDevice = WebGLDevice.Create(canvasInterop);
              // scene.Initialize(gpuDevice); // This will also initialize the SceneView and set its initial size

              // 3:
              // var gpuDevice = WebGLDevice.Create(canvasInterop);
              // sceneView.Initialize(gpuDevice); // This will also initialize the Scene
          }
      }      
      ```
    - After that you can start using the Scene and SceneView objects, for example:
      ```
      sceneView.BackgroundColor = Colors.SkyBlue;

      sceneView.Camera = new TargetPositionCamera()
          {
              Heading = 30,
              Attitude= -20,
              Distance = 300
          };

      var boxNode = new BoxModelNode(centerPosition: new Vector3(0, 0, 0), size: new Vector3(100, 40, 80), material: StandardMaterials.Green);
      scene.RootNode.Add(boxNode);
      
      var pointerCameraController = new PointerCameraController(sceneView)
      {
          RotateCameraConditions = PointerAndKeyboardConditions.LeftPointerButtonPressed,
          MoveCameraConditions = PointerAndKeyboardConditions.LeftPointerButtonPressed | PointerAndKeyboardConditions.ControlKey,
          ZoomMode = CameraZoomMode.PointerPosition,
          RotateAroundPointerPosition = true,
          IsPinchZoomEnabled = true, // zoom with touch pinch gesture
          IsPinchMoveEnabled = true  // move camera with two fingers
      };
      ```

### Troubleshooting

In case of problems, please check the Console in the browser's DevTools (F12). Usually error messages are dispayed there.

You can enable additional logging by setting `CanvasInterop.IsLoggingInteropEvents` and `isLogging` in `sharp-engine.js` to true.

By default (in Ab4d.SharpEngine.Web beta version) the `Log.LogLevel` is set to `Warn`. Also `Log.IsLoggingToConsole` is set to true to display the engine's log messages in the browser's Console.


Please report the problems by creating a new Issue on the samples GitHub page. You can also use the [Feedback form](https://www.ab4d.com/Feedback.aspx) or [Ab4d.SharpEngine Forum](https://forum.ab4d.com/forumdisplay.php?fid=12).
