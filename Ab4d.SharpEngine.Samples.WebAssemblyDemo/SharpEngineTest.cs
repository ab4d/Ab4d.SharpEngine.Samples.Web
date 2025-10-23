using Ab4d.SharpEngine.Browser;
using System;
using System.Numerics;
using Ab4d.SharpEngine.Cameras;
using Ab4d.SharpEngine.Common;
using Ab4d.SharpEngine.Materials;
using Ab4d.SharpEngine.SceneNodes;

namespace Ab4d.SharpEngine.Samples.WebAssemblyDemo;

public class SharpEngineTest
{
    private ICanvasInterop? _canvasInterop;
    private Ab4d.SharpEngine.WebGL.WebGLDevice? _webGlDevice;
    private Scene? _scene;
    private SceneView? _sceneView;

    public static SharpEngineTest Instance = new SharpEngineTest();

    private SharpEngineTest()
    {

    }

    // The following method can be called from BlazorTesterApp project
    public void InitSharpEngine(ICanvasInterop canvasInterop)
    {
        Log("InitSharpEngine called");

        _canvasInterop = canvasInterop;

        var gpuDevice = Ab4d.SharpEngine.WebGL.WebGLDevice.Create(canvasInterop);

        if (!gpuDevice.IsInitialized)
        {
            Log("ERROR: WebGLDevice is not initialized");
            return;
        }

        _webGlDevice = gpuDevice;

        try
        {
            Ab4d.SharpEngine.Utilities.Log.LogLevel = LogLevels.Trace;
            Ab4d.SharpEngine.Utilities.Log.IsLoggingToConsole = true;

            _scene = new Scene(gpuDevice, "MainScene");         // Create Scene object and also initialize it with the gpuDevice.
            _sceneView = new SceneView(_scene, "MainSceneView");


            var boxNode = new BoxModelNode(centerPosition: new Vector3(0, 0, 0), size: new Vector3(100, 40, 80), material: StandardMaterials.Gold) { UseSharedBoxMesh = false };
            _scene.RootNode.Add(boxNode);


            
            var wireGridNode = new WireGridNode("Wire grid")
            {
                CenterPosition = new Vector3(0, -0.5f, -33),
                Size = new Vector2(200, 200),

                WidthDirection = new Vector3(1, 0, 0),   // this is also the default value
                HeightDirection = new Vector3(0, 0, -1), // this is also the default value

                WidthCellsCount = 30,
                HeightCellsCount = 30,

                MajorLineColor = Colors.Black,
                MajorLineThickness = 1,

                MinorLineColor = Colors.Gray,
                MinorLineThickness = 1,

                MajorLinesFrequency = 5,

                IsClosed = true,
            };
            _scene.RootNode.Add(wireGridNode);


            _sceneView.BackgroundColor = Colors.LightYellow;

            var camera = new TargetPositionCamera()
            {
                Heading = 30,
                Attitude = -20,
                Distance = 300,
                ShowCameraLight = ShowCameraLightType.Always
            };

            //camera.StartRotation(40);

            _sceneView.Camera = camera;

            _scene.SetAmbientLight(0.4f);

            var pointerCameraController = new PointerCameraController(_sceneView)
            {
                RotateCameraConditions = PointerAndKeyboardConditions.LeftPointerButtonPressed,
                MoveCameraConditions = PointerAndKeyboardConditions.LeftPointerButtonPressed | PointerAndKeyboardConditions.ControlKey,
                ZoomMode = CameraZoomMode.PointerPosition,
                RotateAroundPointerPosition = true,
                IsPinchZoomEnabled = true, // zoom with touch pinch gesture
                IsPinchMoveEnabled = true  // move camera with two fingers
            };

            _sceneView.Render();
        
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void RenderScene()
    {
        if (_sceneView == null)
        {
            Log("RenderScene - no sceneView");
            return;
        }

        try
        {
            //Log($"Render sceneView, size: {_sceneView.Width} x {_sceneView.Height}");
            bool isRendered = _sceneView.Render(forceRender: false, forceUpdateAll: false);
            if (isRendered)
                Log($"Scene rendered");
        }
        catch (Exception ex)
        {
            Log("RenderScene error: " + ex.Message);
            throw;
        }
    }

    public void ToggleCameraRotation()
    {
        if (_sceneView == null || _sceneView.Camera is not TargetPositionCamera targetPositionCamera)
            return;

        if (targetPositionCamera.IsRotating)
            targetPositionCamera.StopRotation();
        else
            targetPositionCamera.StartRotation(40);
    }
    
    public void OnCanvasResized(int width, int height, float dpiScale)
    {
        try
        {
            Log($"OnCanvasResized size: {width} x {height} (dpi: {dpiScale})");
            _sceneView?.Resize(width, height, dpiScale);
        }
        catch (Exception ex)
        {
            Log("OnCanvasResized error: " + ex.Message);
            throw;
        }
    }
    
    public void DumpSceneInfo()
    {
        if (_sceneView == null || _scene == null)
            return;

        if (_sceneView.Camera is TargetPositionCamera targetPositionCamera)
            Log($"Camera: pos: {targetPositionCamera.GetCameraPosition()} => TargetPosition: {targetPositionCamera.TargetPosition} (lookDir: {targetPositionCamera.GetLookDirection()}; aspect: {targetPositionCamera.AspectRatio}");

        var infoText = _scene.GetSceneNodesInfo();
        Log(infoText);
    }
    
    private void Log(string message)
    {
        Console.WriteLine("SharpEngineTest: " + message);
    }
}