using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Ab4d.SharpEngine.WebGL;

namespace Ab4d.SharpEngine.Samples.WebAssemblyDemo;

// This class must be partial for the code generator to be able to generate JS interop code from methods marked as JSExport
public static partial class JavaScriptInterop
{
    // The following method is called from javascript from sharp-engine-wasm-test
    [JSExport]
    public async static Task InitSharpEngineJSExport()
    {
        await CanvasInterop.InitializeInterop();

        var canvasInterop = new CanvasInterop("webGLCanvas");

        // Try to connect to the canvas and get the WebGL context.
        // We can also skip this call. In this case InitWebGL will be called from the Scene or SceneView Initialized method.
        // But by calling this by ourselves, we can immediately check if the WebGL context is available (checking IsWebGLInitialized).
        canvasInterop.InitWebGL();

        if (!canvasInterop.IsWebGLInitialized)
            return; // Skip creating Scene and SceneView objects; error message was already written to console in the InitWebGL method


        SharpEngineTest.Instance.InitSharpEngine(canvasInterop);
    }

    [JSExport]
    public static void ToggleCameraRotationJSExport()
    {
        SharpEngineTest.Instance.ToggleCameraRotation();
    }
    
    [JSExport]
    public static void ChangeMaterialJSExport(string? colorText)
    {
        SharpEngineTest.Instance.ChangeMaterial(colorText);
    }
    
    [JSExport]
    public static void AddObjectsJSExport(int objectsCount)
    {
        SharpEngineTest.Instance.AddObjects(objectsCount);
    }
}