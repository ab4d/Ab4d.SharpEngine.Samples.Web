# Ab4d.SharpEngine.Samples.Web

![Ab4d.SharpEngine logo](doc/sharp-engine-logo.png)

Welcome to the Browser samples for Ab4d.SharpEngine.

**Ab4d.SharpEngine is a cross-platform 3D rendering engine for desktop, mobile and browser .Net applications.**

To check the Ab4d.SharpEngine for desktop and mobile devices, see the [Ab4d.SharpEngine.Samples on GitHub](https://github.com/ab4d/Ab4d.SharpEngine.Samples) (those samples also demonstrate all of the features of the Ab4d.SharpEngine library).

> [!IMPORTANT]
> Ab4d.SharpEngine for browser (Ab4d.SharpEngine.Web assembly) is in beta and is not yet ready for production (the current version will expire on 2026-01-31).

### Quick start guide

This repository contains two solutions:
- `Ab4d.SharpEngine.Samples.BlazorWebAssembly.sln` that shows how to use Ab4d.SharpEngine in a **Blazor WebAssembly** app. This provides the best integration and debugging experience. See [readme](Ab4d.SharpEngine.Samples.BlazorWebAssembly/README.md).
- `Ab4d.SharpEngine.Samples.NoBlazorBrowserDemo.sln` that uses 4 projects and shows how to use Ab4d.SharpEngine in a **Asp.Net Core** or **Simple Html** website. See [readme](Ab4d.SharpEngine.Samples.WebAssemblyDemo/README.md).

          
### Additional samples and documentation

This beta version of the samples only demonstrates a few of the features of the Ab4d.SharpEngine library.
For example, a simple 3D scene is created and the samples demonstrate how to use the camera controller and perform hit testing.

But the samples do not demonstrate all the features of the engine. This will be done in future versions of the project.

The Ab4d.SharpEngine.Web library is going to implement all the features of the Ab4d.SharpEngine for desktop and mobile apps.
Because it uses the same code (linked files), using the implemented features is almost always identical to the desktop version.

Therefore, for a full demonstration of the engine, please check the samples for desktop and mobile devices (https://github.com/ab4d/Ab4d.SharpEngine.Samples) and
online help (https://www.ab4d.com/help/SharpEngine/html/R_Project_Ab4d_SharpEngine.htm).

To see what features are already implemented, see the "Implementation details" below, or 
use IntelliSense to see what classes are available. 


### Current implementation details

Namespace implementation status (**compared to desktop and mobile Ab4d.SharpEngine**):
- **Animation**: 100% implemented :heavy_check_mark:
- **Cameras**: 100% implemented :heavy_check_mark:
- **Materials**:
    - StandardEffect - 100% implemented :heavy_check_mark:
    - ThickLineEffect - LineThickness, line patterns and line caps and hidden lines are not supported.   
      WebGL does not support thick lines or geometry shader so this requires a different approach (probably CPU based mesh generation). This will be supported after v1.0. Use TubeLineModelNode and TubePathModelNode with SolidColorMaterial for thick lines (here the line thickness in not in screen space values).
    - PixelEffect - planned for v1.0 :one:
    - SpriteEffect - planned for v1.0 :one:
    - VertexColorEffect - planned for v1.0 :one:
    - VolumeRenderingEffect - supported later :two:
- **Lights**: 100% implemented :heavy_check_mark:
- **Materials**: 
    - StandardMaterial - 100% implemented :heavy_check_mark:
    - SolidColorMaterial - (using StandardEffect) - 100% implemented :heavy_check_mark:
    - LineMaterial - Rendering colored lines with 1px line thickness. See comment with ThickLineEffect for more info.
    - PolyLineMaterial - Polylines are rendered as multiple individual lines. Because line thickness is limited to 1px, no mitered and beveled joints are required.
    - PositionColoredLineMaterial - supported later :two:
    - VertexColorMaterial - planned for v1.0 :one:
    - PrimitiveIdMaterial - planned after v1.0 :one:
    - DepthOnlyMaterial - supported later :two:
    - VolumeMaterial - supported later :two:
- **Meshes**: all supported except SubMesh (planned for v1.0) :one:
- **OverlayPanels**: CameraAxisPanel planned for v1.0 :one:
- **PostProcessing**: planned after v1.0 :one:
- **SceneNodes**: all supported except: height map, instancing, MultiMaterialModelNode and PixelsNode. All planned for v1.0 :one:
- **Transformations**: 100% implemented :heavy_check_mark:
- **Utilities**: implemented all except:
    - BezierCurve, BSpline - 100% implemented :heavy_check_mark:
    - BitmapTextCreator - 100% implemented :heavy_check_mark:
    - CameraController - 100% implemented :heavy_check_mark:
    - EdgeLinesFactory - 100% implemented :heavy_check_mark:
    - CameraUtils, LineUtils, MathUtils, MeshUtils, ModelUtils, TransformationUtils - 100% implemented :heavy_check_mark:
    - LineSelectorData (used for line selection) - 100% implemented :heavy_check_mark:
    - MeshBooleanOperations - 100% implemented :heavy_check_mark:
    - MeshOctree - 100% implemented :heavy_check_mark:
    - MeshTrianglesSorter - 100% implemented :heavy_check_mark:
    - ModelMover, ModelRotator and ModelScalar - planned for v1.0 :one:
    - ObjImporter - 100% implemented :heavy_check_mark:
    - ObjExporter - planned for v1.0 :one:
    - StlImporter, StlExporter - planned for v1.0 :one:
    - TextureLoader, TextureFactory - 100% implemented :heavy_check_mark:
    - Triangulator - 100% implemented :heavy_check_mark:
    - TrueTypeFontLoader, VectorFontFactory - 100% implemented :heavy_check_mark:
    - SpriteBatch - planned for v1.0 :one:
   
Other not implemented features:
- Super-sampling (planned for later)


### Roadmap 

**v1.0** is planned for release before the end of 2025.

 
 ### Tips and tricks

 - When you publish the Blazor app to a subfolder on your website, change the base href value in the wwwroot/index.html
   from "/" (used when the Blazor app is in the root folder) to your actual folder, for example, to "https://www.ab4d.com/sharp-engine-browser-demo/".


### Troubleshooting

In case of problems, please check the Console in the browser's DevTools (F12). Usually, error messages are displayed there.

You can enable additional logging by setting `CanvasInterop.IsLoggingInteropEvents` and `isLogging` in `sharp-engine.js` to true.

By default (in Ab4d.SharpEngine.Web beta version) the `Log.LogLevel` is set to `Warn`. Also, `Log.IsLoggingToConsole` is set to true to display the engine's log messages in the browser's Console.

Note that I am not an expert in Blazor and therefore some things may not be created optimally. Please create a PR or add a new issue if you know of any improvements.

Please report the problems or improvement ideas by creating a new [Issue on GitHub](https://github.com/ab4d/Ab4d.SharpEngine.Samples.Web/issues). You can also use the [Feedback form](https://www.ab4d.com/Feedback.aspx) or [Ab4d.SharpEngine Forum](https://forum.ab4d.com/forumdisplay.php?fid=12).
