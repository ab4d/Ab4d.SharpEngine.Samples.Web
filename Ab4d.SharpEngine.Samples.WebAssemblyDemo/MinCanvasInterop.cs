using Ab4d.SharpEngine.Browser;
using Ab4d.SharpEngine.Common;
using System;
using System.Threading.Tasks;

namespace WebGLWebAssemblyTest;

public class MinCanvasInterop : ICanvasInterop
{
    public MinCanvasInterop(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void InitWebGL(bool useMultisampleAntiAliasing)
    {
        throw new NotImplementedException("InitWebGL");
    }

    /// <inheritdoc />
    public void LoadTextFile(string fileName, Action<string> onLoadedCallback, Action<string>? onLoadErrorCallback)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void LoadBinaryFile(string fileName, Action<byte[]> onLoadedCallback, Action<string>? onLoadErrorCallback)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void LoadImageBytes(string fileName, Action<RawImageData> onTextureLoadedCallback, Action<string>? onLoadErrorCallback)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<string> LoadTextFileAsync(string fileName)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<byte[]> LoadBinaryFileAsync(string fileName)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<RawImageData> LoadImageBytesAsync(string fileName)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void SubscribePointerEvents()
    {
        throw new NotImplementedException("SubscribePointerEvents");
    }

    /// <inheritdoc />
    public void UnsubscribePointerEvents()
    {
        throw new NotImplementedException("UnsubscribePointerEvents");
    }

    /// <inheritdoc />
    public void SetCursorStyle(string cursorStyle)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void SetPointerCapture(int pointerId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void ReleasePointerCapture(int pointerId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public string CanvasId => "webGLCanvas";

    /// <inheritdoc />
    public bool IsWebGLInitialized => true;

    /// <inheritdoc />
    public int Width { get; set; }

    /// <inheritdoc />
    public int Height { get; set; }

    /// <inheritdoc />
    public float DpiScale => 1.25f;

    /// <inheritdoc />
    public bool IsUsingMultisampleAntiAliasing => true;

    /// <inheritdoc />
    public bool ArePointerEventsSubscribed => false;

    /// <inheritdoc />
    public event EventHandler? WebGLInitialized;

    /// <inheritdoc />
    public event MouseButtonEventHandler? PointerDown;

    /// <inheritdoc />
    public event MouseButtonEventHandler? PointerUp;

    /// <inheritdoc />
    public event MouseMoveEventHandler? PointerMoved;

    /// <inheritdoc />
    public event MouseWheelEventHandler? MouseWheelChanged;

    /// <inheritdoc />
    public event PinchZoomEventHandler? PinchZoomStarted;

    /// <inheritdoc />
    public event EventHandler? PinchZoomEnded;

    /// <inheritdoc />
    public event PinchZoomEventHandler? PinchZoomed;

    /// <inheritdoc />
    public event EventHandler? BrowserAnimationFrameUpdated;

    /// <inheritdoc />
    public event CanvasResizedEventHandler? CanvasResized;

    /// <inheritdoc />
    public event EventHandler? ContextLost;

    /// <inheritdoc />
    public event EventHandler? Disposing;

#if !NUGET_SHARPENGINE
    public bool IsWebGL2 => true;

    public void LoadImageBytes(string fileName, Action<RawImageData?> onTextureLoadedAction)
    {
        throw new NotImplementedException("LoadImageBytes");
    }
#endif
}