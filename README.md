# Ab4d.SharpEngine.Samples.Web

![Ab4d.SharpEngine logo](doc/sharp-engine-logo.png)

Welcome to the Browser samples for Ab4d.SharpEngine.

**Ab4d.SharpEngine is a cross-platform 3D rendering engine for desktop, mobile and browser .Net applications.**

To check the Ab4d.SharpEngine for desktop and mobile devices see the [Ab4d.SharpEngine.Samples on GitHub](https://github.com/ab4d/Ab4d.SharpEngine.Samples).

> [!IMPORTANT]
> Ab4d.SharpEngine for browser (Ab4d.SharpEngine.Web assembly) is in beta and is not yet ready for production (the current version will expire on 2025-12-31).

### Quick start guide

To start this samples project, open `Ab4d.SharpEngine.Samples.BlazorWebAssembly` solution or project in any .Net IDE and start it. 

You can also start it from CLI by executing `dotnet run .` or similar command in the `Ab4d.SharpEngine.Samples.BlazorWebAssembly` folder.

### Usage in your own project

To use the Ab4d.SharpEngine.Web library in your own project, follow those steps:
- Create a new "Blazor WebAssembly Standalone App" project (use .Net 9 or newer).
- Add reference to Ab4d.SharpEngine.Web NuGet package.
- Copy the following files from this samples project to your project:
    - `CanvasInterop.cs` (copy to the root folder of your project)
    - `SharpEngineSceneView.razor` (copy to the root folder of your project)
    - `wwwroot/sharp-engine.js` (copy to the wwwroot folder)
    - `Native/libEGL.c` (create a new Native folder in your project and copy the libEGL.c file there)
- Open the csproj file of your project and add the following:
    - Into the first `PropertyGroup`:
      ```
      <!-- unsafe code is required to use JSExport in CanvasInterop -->
      <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
     
      <!-- The following emscripten flags are required for Ab4d.SharpEngine to use WebGL from the browser -->
      <EmccFlags>-lGL -s FULL_ES3=1 -sMAX_WEBGL_VERSION=2</EmccFlags>

      <!-- Blazor WebAssembly supports SIMD instructions (when supported by the browser), so it is recommended to enable that -->
      <WasmEnableSIMD>true</WasmEnableSIMD>
      ```
    - Optionally, you can set additional properties for the DEBUG and RELEASE builds. See the csproj file in this sample for example PropertyGroup blocks.
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
- Open the razor page that will host the 3D scene (for example Home.razor) and add the following:
    - Add using, inject and implements to the start of the razor file:
      ```
      @using System.Numerics
      @using Ab4d.SharpEngine
      @using Ab4d.SharpEngine.Common
      @using Ab4d.SharpEngine.WebGL
      @inject IJSRuntime JS      
      @implements IDisposable
      ```
    - Define the canvas element in your razor file. For example, add the following to Home.razor (before the "@code"):
      ```
      <canvas id="webGLCanvas" style="width: 70%; height: 500px"></canvas>
      ```

      **NOTE:** The id of the canvas will be used in the code to connect to the canvas element.
      
      **TIP:** Set the size of the canvas by setting width and height in the style and not by setting width and height properties. Setting width and height properties would set the size of the back-buffers and is not recommended for web.


    - Override the OnInitializedAsync and OnAfterRender methods. 
      In the OnInitializedAsync method call the static `CanvasInterop.InitializeInterop` method. 
      In the OnAfterRender method create an instance of the `CanvasInterop` and then create the `WebGLDevice`, `Scene` and `SceneView` objects.
      For example:
      ```
      @code
      {
          private CanvasInterop? _canvasInterop;
          
          protected override async Task OnInitializedAsync()
          {
              // Initialize the browser interop (load sharp-engine.js file and from javascript get access to exported methods in the CanvasInterop class)
              // Because Blazor uses Single Page Applications style, this needs to be executed only once
              await CanvasInterop.InitializeInterop();
          }
          
          /// <inheritdoc />
          protected override void OnAfterRender(bool firstRender)
          {
              if (!CanvasInterop.IsInteropInitialized)
                  return;
          
          
              // In OnAfterRender method all the DOM elements (including our canvas) have been initialized, so we can connect to them.
              // So we can create an instance of CanvasInterop class that will connect the SharpEngine with the canvas in the DOM.
              // NOTE: canvasId is the id of the canvas that shows WebGL graphics (see the html part of the code above)
              _canvasInterop = new CanvasInterop(canvasId: "webGLCanvas"); // if we do not need pointer events, we can also add: subscribePointerEvents: false
          
              // Try to connect to the canvas and get the WebGL context.
              // We can also skip this call. In this case InitWebGL will be called from the Scene or SceneView Initialized method.
              // But by calling this by ourselves, we can immediately check if the WebGL context is available (checking IsWebGLInitialized).
              _canvasInterop.InitWebGL();
          
              if (!_canvasInterop.IsWebGLInitialized)
                  return; // Skip creating Scene and SceneView objects; error message was already written to console in the InitWebGL method
          
          
              var gpuDevice = WebGLDevice.Create(_canvasInterop); // We can also pass an EngineCreateOptions object to the Create method
          
              if (!gpuDevice.IsInitialized)
                  return; // Blazor cannot use the WebGL context
          
              var scene = new Scene(gpuDevice, "MainScene");         // Create Scene object and also initialize it with the gpuDevice.
              var sceneView = new SceneView(scene, "MainSceneView");
          
          
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
    - Add Dispose method (note that we added "@implements IDisposable" to the start of the file so Blazor will call the Dispose method).
      Dispose method is called when the user navigates away from our razor page.
      ```
      public void Dispose()
      {
          if (_canvasInterop != null)
          {
              _canvasInterop.Dispose(); // This will also dispose SceneView, Scene and WebGLDevice
              _canvasInterop = null;
          }
      }      
      ```
    - After that, you can start using the Scene and SceneView objects, for example:
      ```
      // Add a green 3D box to the scene      
      var boxNode = new BoxModelNode(centerPosition: new Vector3(0, 0, 0), size: new Vector3(100, 40, 80), material: StandardMaterials.Green);
      scene.RootNode.Add(boxNode);
      
      
      sceneView.BackgroundColor = Colors.SkyBlue;
      
      sceneView.Camera = new TargetPositionCamera()
      {
          Heading = 30,
          Attitude = -20,
          Distance = 300
      };
      
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
      
### Additional documentation

This version of the samples project only demonstrates how to initialize the SharpEngine for the Blazor app. 
It creates a very simple 3D scene and shows how to use camera controller and do some hit testing.

But it does not demonstrate all the features of the engine. This will be done in the future versions of the project.

The Ab4d.SharpEngine.Web library is going to implement all the features of the Ab4d.SharpEngine for desktop and mobile apps.
Because it uses the same code (linked files), using the implemented features is the same as with a desktop version.

Therefore, for the full demonstration of the engine, please check the samples for the desktop and mobile devices (https://github.com/ab4d/Ab4d.SharpEngine.Samples) and
online help (https://www.ab4d.com/help/SharpEngine/html/R_Project_Ab4d_SharpEngine.htm).

To see what features are already implemented see the "Implementation details" below or 
use IntelliSense to see what classes are available. 



### Implementation details and roadmap

**beta2** version is planned for October 2025.
**v1.0** is planned to be released before the end of 2025.

Namespace implementation status (compared to desktop and mobile Ab4d.SharpEngine):
- **Animation**: 100% implemented :heavy_check_mark:
- **Cameras**: 100% implemented :heavy_check_mark:
- **Effects**:
    - StandardEffect - missing texture support (planned for beta2)
    - ThickLineEffect - LineThickness, line patterns and line caps and hidden lines are not supported.
    
      WebGL does not support thick lines or geometry shader so this requires a different approach (probably CPU based mesh generation). This will be supported after v1.0. Use TubeLineModelNode and TubePathModelNode with SolidColorMaterial for thick lines (here the line thickness in not in screen space values).
    - SolidColorEffect - planned for beta2
    - PixelEffect - planned for v1.0 (probably only 1x1 pixels will be supported with v1.0)
    - SpriteEffect - planned for v1.0
    - VertexColorEffect - planned for in v1.0
    - VolumeRenderingEffect - supported later
- **Lights**: 100% implemented :heavy_check_mark:
- **Materials**: see supported Effects
- **Meshes**: all supported except SubMesh (planned for v1.0)
- **OverlayPanels**: CameraAxisPanel planned for v1.0
- **PostProcessing**: planned after v1.0
- **SceneNodes**: all supported except: height map, instancing, MultiMaterialModelNode and PixelsNode. All planned for v1.0.
- **Transformations**: 100% implemented :heavy_check_mark:
- **Utilities**: implemented all except:
    - BitmapTextCreator - planned for v1.0
    - ModelMover, ModelRotator and ModelScalar - planned for v1.0
    - ObjImporter, ObjExporter, StlImporter, StlExporter - planned for v1.0
    - SpriteBatch - planned for v1.0
    - TextureLoader, TextureFactory - planned for beta2
    - VectorFontFactory, TrueTypeFontLoader - planned for v1.0

 
### Troubleshooting

In case of problems, please check the Console in the browser's DevTools (F12). Usually, error messages are displayed there.

You can enable additional logging by setting `CanvasInterop.IsLoggingInteropEvents` and `isLogging` in `sharp-engine.js` to true.

By default (in Ab4d.SharpEngine.Web beta version) the `Log.LogLevel` is set to `Warn`. Also, `Log.IsLoggingToConsole` is set to true to display the engine's log messages in the browser's Console.

Note that I am not an expert for Blazor and therefore some things may not be created optimally. Please create a PR or add a new issue if you know for any improvements.

Please report the problems or improvement ideas by creating a new [Issue on GitHub](https://github.com/ab4d/Ab4d.SharpEngine.Samples.Web/issues). You can also use the [Feedback form](https://www.ab4d.com/Feedback.aspx) or [Ab4d.SharpEngine Forum](https://forum.ab4d.com/forumdisplay.php?fid=12).
