using Ab4d.SharpEngine.Common;
using Ab4d.SharpEngine.Materials;
using System.Numerics;
using Ab4d.SharpEngine.Samples.BlazorWebAssembly;
using Ab4d.SharpEngine.Utilities;
using Ab4d.SharpEngine.WebGL;

namespace Ab4d.SharpEngine.Samples.Common;

public class BlazorSamplesContext : ICommonSamplesContext
{
    public static readonly BlazorSamplesContext Current = new BlazorSamplesContext();

    public WebGLDevice? GpuDevice => CurrentSharpEngineSceneView?.GpuDevice ?? null;

    private BrowserBitmapIO _bitmapIO;
    private BitmapTextCreator? _bitmapTextCreator;
    public IBitmapIO BitmapIO => _bitmapIO;

    private TextBlockFactory? _textBlockFactory;
    private Task<TextBlockFactory>? _textBlockFactoryLoadingTask;

    public ISharpEngineSceneView? CurrentSharpEngineSceneView { get; private set; }

    public event EventHandler? CurrentSharpEngineSceneViewChanged;


    public void RegisterCurrentSharpEngineSceneView(SharpEngineSceneView? sharpEngineSceneView)
    {
        if (ReferenceEquals(CurrentSharpEngineSceneView, sharpEngineSceneView))
            return;

        CurrentSharpEngineSceneView = sharpEngineSceneView;

        OnCurrentSharpEngineSceneViewChanged();
    }

    protected void OnCurrentSharpEngineSceneViewChanged()
    {
        CurrentSharpEngineSceneViewChanged?.Invoke(this, EventArgs.Empty);
    }

    public TextBlockFactory GetTextBlockFactory()
    {
        throw new NotSupportedException("Synchronous GetTextBlockFactory is not supported in Web samples. Use GetTextBlockFactoryAsync.");
    }

    public Task<TextBlockFactory> GetTextBlockFactoryAsync()
    {
        // If already loaded, return synchronously
        if (_textBlockFactory != null)
            return Task.FromResult(_textBlockFactory);
        
        // If loading already started, return the same task
        if (_textBlockFactoryLoadingTask != null)
            return _textBlockFactoryLoadingTask;

        // Start loading and store the task
        _textBlockFactoryLoadingTask = GetTextBlockFactoryIntAsync();

        return _textBlockFactoryLoadingTask;
    }
    
    private async Task<TextBlockFactory> GetTextBlockFactoryIntAsync()
    {
        if (CurrentSharpEngineSceneView == null)
            throw new InvalidOperationException("Cannot call GetTextBlockFactory when CurrentSharpEngineSceneView is not yet set.");

        if (_textBlockFactory != null && _textBlockFactory.Scene != CurrentSharpEngineSceneView.Scene)
        {
            _textBlockFactory.Dispose();
            _textBlockFactory = null;
        }

        if (_textBlockFactory == null)
        {
            if (GpuDevice == null)
                throw new Exception("Cannot create TextBlockFactory because GpuDevice is null");
            
            var bitmapFont = await BitmapFont.CreateAsync("fonts/roboto_64.fnt", GpuDevice);
            _bitmapTextCreator = await BitmapTextCreator.CreateAsync(CurrentSharpEngineSceneView.Scene, bitmapFont);

            // Create TextBlockFactory that will use the default BitmapTextCreator (get by BitmapTextCreator.GetDefaultBitmapTextCreator).
            _textBlockFactory = new TextBlockFactory(_bitmapTextCreator);
        }
        else
        {
            // Reset existing TextBlockFactory to default values:
            _textBlockFactory.TextColor = Color4.Black;
            _textBlockFactory.FontSize = 14;
            _textBlockFactory.BackgroundHorizontalPadding = 8;
            _textBlockFactory.BackgroundVerticalPadding = 4;
            _textBlockFactory.BackgroundColor = Color4.Transparent;
            _textBlockFactory.BorderThickness = 0;
            _textBlockFactory.BorderColor = Color4.Black;
            _textBlockFactory.BackMaterialColor = Color4.Black;
        }

        return _textBlockFactory;
    }
}
