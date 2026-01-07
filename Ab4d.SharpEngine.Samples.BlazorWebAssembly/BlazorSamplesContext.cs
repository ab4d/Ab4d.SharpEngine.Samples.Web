using Ab4d.SharpEngine.Common;
using Ab4d.SharpEngine.Materials;
using System.Drawing;
using System.Numerics;
using Ab4d.SharpEngine.Samples.BlazorWebAssembly;
using Ab4d.SharpEngine.WebGL;

namespace Ab4d.SharpEngine.Samples.Common;

public class BlazorSamplesContext : ICommonSamplesContext
{
    public static readonly BlazorSamplesContext Current = new BlazorSamplesContext();

    public WebGLDevice? GpuDevice => CurrentSharpEngineSceneView?.GpuDevice ?? null;

    private BrowserBitmapIO _bitmapIO;
    public IBitmapIO BitmapIO => _bitmapIO;

    //private TextBlockFactory? _textBlockFactory;

    public ISharpEngineSceneView? CurrentSharpEngineSceneView { get; private set; }

    public event EventHandler? CurrentSharpEngineSceneViewChanged;


    public void RegisterCurrentSharpEngineSceneView(SharpEngineSceneView? sharpEngineSceneView)
    {
        if (ReferenceEquals(CurrentSharpEngineSceneView, sharpEngineSceneView))
            return;

        //CurrentSharpEngineSceneView = sharpEngineSceneView;

        OnCurrentSharpEngineSceneViewChanged();
    }

    protected void OnCurrentSharpEngineSceneViewChanged()
    {
        CurrentSharpEngineSceneViewChanged?.Invoke(this, EventArgs.Empty);
    }
}
