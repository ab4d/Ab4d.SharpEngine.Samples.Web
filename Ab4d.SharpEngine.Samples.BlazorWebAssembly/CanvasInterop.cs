using Ab4d.SharpEngine.Browser;
using Ab4d.SharpEngine.Common;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

// IMPORTANT:
// If you change this namespace, then you also need to change the code in sharp-engine.js: interop = exports.Ab4d.SharpEngine.WebGL.CanvasInterop;
// For example, if you change the namespace to MyBlazor, then you need to change the line in sharp-engine.js to interop = exports.MyBlazor.CanvasInterop;
// ReSharper disable once CheckNamespace
namespace Ab4d.SharpEngine.WebGL;

//#pragma warning disable CA1416 // prevented: This call site is reachable on all platforms. 'JSHost.ImportAsync(string, string, CancellationToken)' is only supported on: 'browser'.

[SupportedOSPlatform("browser")]
public partial class CanvasInterop : ICanvasInterop
{
    private static readonly bool IsLoggingInteropEvents = false; // When set to true, then all interop events are logged to the console

    // Update the version in the url to the latest version
    private const string SpectorScriptUrl = "https://cdn.jsdelivr.net/npm/spectorjs@0.9.30/dist/spector.bundle.js";
    
    private static bool _isInitializeCalled;
    
    private static CanvasInterop? _initialInterop;
    private static List<CanvasInterop>? _additionalInteropObjects;

    private Dictionary<string, Action<RawImageData?>> _imageBytesLoadedCallbacks = new();
    
    /// <summary>
    /// Returns true when the <see cref="InitializeInterop"/> method was called and successfully initialized the browser interop.
    /// </summary>
    public static bool IsInteropInitialized { get; private set; }

    
    /// <summary>
    /// Gets the id of the canvas element that is defined in the browser DOM (html or razor file).
    /// </summary>
    public string CanvasId { get; }
    
    /// <summary>
    /// Returns true after the <see cref="InitWebGL"/> method was called, the WebGL context was created and connection with the canvas elements was successfully established.
    /// </summary>
    public bool IsWebGLInitialized { get; private set; }

    /// <summary>
    /// Returns true when WebGL 2.0 is used. When false, then WebGL 1.0 is used. In this case some features may not be available.
    /// </summary>
    public bool IsWebGL2 { get; private set; }
    
    /// <summary>
    /// Gets the width of the canvas in pixels (defines width of the back buffer that is used for rendering).
    /// </summary>
    public int Width { get; private set; }
    
    /// <summary>
    /// Gets the height of the canvas in pixels (defines width of the back buffer that is used for rendering).
    /// </summary>    
    public int Height { get; private set; }

    /// <summary>
    /// Gets the dpi scale of the canvas.
    /// </summary>    
    public float DpiScale { get; private set; } = 1;

    /// <summary>
    /// Returns true when the canvas element is using MSAA (Multisample Anti-Aliasing).
    /// MSAA is can be disabled when the <see cref="InitWebGL"/> is called and the useMultisampleAntiAliasing parameters is set to false.
    /// </summary>
    public bool IsUsingMultisampleAntiAliasing { get; private set; }
    
    /// <summary>
    /// Returns true when pointer (mouse, pointer and touch) events are subscribed in javascript.
    /// </summary>
    public bool ArePointerEventsSubscribed { get; private set; }
    
    public event MouseButtonEventHandler? PointerDown;
    public event MouseButtonEventHandler? PointerUp;
    public event MouseMoveEventHandler? PointerMoved;
    public event MouseWheelEventHandler? MouseWheelChanged;
    
    public event PinchZoomEventHandler? PinchZoomStarted;
    public event EventHandler? PinchZoomEnded;
    public event PinchZoomEventHandler? PinchZoomed;
    
    public event EventHandler? BrowserAnimationFrameUpdated;
    public event CanvasResizedEventHandler? CanvasResized;

    public event EventHandler? Disposing;

    private bool _isSpectorScriptLoaded;
    private bool _isSpectorCaptureStarted;
    private bool _subscribePointerEventsOnInitialize;

    public CanvasInterop(string canvasId, bool subscribePointerEvents = true)
    {
        CanvasId = canvasId;
        _subscribePointerEventsOnInitialize = subscribePointerEvents;
    }
    
    #region static Initialize method and GetCanvasInterop
    public static async Task InitializeInterop(string? sharpEngineJsFileUrl = null)
    {
        if (_isInitializeCalled)
            return;

        _isInitializeCalled = true;

        if (!OperatingSystem.IsBrowser())
            throw new SharpEngineException("Ab4d.SharpEngine.Web can be used only in Blazor WebAssembly.");

        if (IsLoggingInteropEvents)
            Console.WriteLine("Initializing CanvasInterop");

        try
        {
            sharpEngineJsFileUrl ??= "../sharp-engine.js"; // by default the sharp-engine.js is in the wwwroot folder
            await JSHost.ImportAsync(moduleName: "sharp-engine.js", moduleUrl: sharpEngineJsFileUrl);
        }
        catch (Exception ex)
        {
            if (ex is JSException && ex.Message.Contains("Failed to fetch dynamically imported module"))
                throw new SharpEngineException("Cannot load sharp-engine.js. This file is required to setup the SharpEngine for Blazor WebAssembly. Make sure that the file is added to the wwwroot folder and that in csproj the following line exist: <WasmExtraFilesToDeploy Include=\"sharp-engine.js\" />.", ex);
            
            if (ex is JSException && ex.Message.StartsWith("SyntaxError:"))
                throw new SharpEngineException("Error parsing sharp-engine.js file. Please revert to the original file content or check your changes. Error message: " + ex.Message, ex);
            
            throw new SharpEngineException("Error loading sharp-engine.js file: " + ex.Message, ex);
        }
        
        try
        {
            await InitInteropAsync(); // Set interop field so javascript can call functions that are exported from CanvasInterop class (using JSExport attribute)
            
            IsInteropInitialized = true;
        }
        catch (Exception ex)
        {
            throw new SharpEngineException("Error initializing javascript interop: " + ex.Message, ex);
        }
    }
    
    private static CanvasInterop GetCanvasInterop(string? canvasId)
    {
        return GetCanvasInterop(canvasId, throwExceptionIfNotFound: true)!;
    }
    
    private static CanvasInterop? GetCanvasInterop(string? canvasId, bool throwExceptionIfNotFound)
    {
        if (canvasId == null)
            throw new Exception("Cannot find CanvasInterop because canvasId that was provided by the javascript is null");
        
        // Optimize for the most common case: we use only one WebGL canvas 
        // But we support also more than one cases (by using _additionalInteropObjects)
        
        if (_initialInterop != null && _initialInterop.CanvasId.Equals(canvasId, StringComparison.Ordinal))
            return _initialInterop;
        
        
        CanvasInterop? foundInterop = null;
        
        if (_additionalInteropObjects != null)
        {
            foreach (var sharpEngineBrowserInterop in _additionalInteropObjects)
            {
                if (sharpEngineBrowserInterop.CanvasId.Equals(canvasId, StringComparison.Ordinal))
                {
                    foundInterop = sharpEngineBrowserInterop;
                    break;
                }
            }
        }

        if (throwExceptionIfNotFound && foundInterop == null)
            throw new Exception($"Cannot find any CanvasInterop object that was initialized with the canvasId: '{canvasId}'");
        
        return foundInterop;
    }
    #endregion

    public void InitWebGL(bool useMultisampleAntiAliasing = true)
    {
        CheckIsInitialized(checkIfConnectedToCanvas: false);

        var result = InitWebGLCanvasJs(this.CanvasId, useMultisampleAntiAliasing, _subscribePointerEventsOnInitialize, subscribeRequestAnimationFrame: true);

        if (string.IsNullOrEmpty(result) || !result.StartsWith("OK:"))
        {
            // canvasId not found or WebGL not available
            if (!string.IsNullOrEmpty(result))
                Console.WriteLine("Error initializing WebGL Canvas: " + result);

            return;
        }

        var dataParts = result.Substring(3).Split(';'); // Skip "OK:" and then split
        this.IsWebGL2 = dataParts[0] == "v2";
        this.Width    = int.Parse(dataParts[1]);
        this.Height   = int.Parse(dataParts[2]);
        this.DpiScale = float.Parse(dataParts[3], CultureInfo.InvariantCulture);
        this.IsUsingMultisampleAntiAliasing = useMultisampleAntiAliasing;
        
        // Set static instances of CanvasInterop so that the static callback functions from javascript can find the target CanvasInterop
        if (_initialInterop == null)
        {
            _initialInterop = this;
        }
        else
        {
            _additionalInteropObjects ??= new List<CanvasInterop>();
            _additionalInteropObjects.Add(this);
        }
        
        this.ArePointerEventsSubscribed = _subscribePointerEventsOnInitialize;
        this.IsWebGLInitialized = true;

        if (IsLoggingInteropEvents)
            Console.WriteLine($"Initialized WebGL for '{this.CanvasId}': {this.Width} x {this.Height}; dpiScale: {this.DpiScale}");

    }

    private void CheckIsInitialized(bool checkIfConnectedToCanvas = true, [CallerMemberName] string? methodName = null)
    {
        if (!_isInitializeCalled)
            throw new SharpEngineException($"Cannot call {methodName} because InitializeInterop method was not called yet.");

        if (!IsInteropInitialized)
            throw new SharpEngineException($"Cannot call {methodName} because the WebGL canvas was not correctly initialized when calling InitializeInterop method.");
        
        if (checkIfConnectedToCanvas && !IsWebGLInitialized)
            throw new SharpEngineException($"Cannot call {methodName} because the Connect method was not called or it failed to connect to the canvas element.");
    }
    
    public void LoadImageBytes(string fileName, Action<RawImageData?>? onTextureLoadedAction)
    {
        CheckIsInitialized();

        if (onTextureLoadedAction != null)
            _imageBytesLoadedCallbacks.Add(fileName, onTextureLoadedAction);

        LoadImageBytesJs(this.CanvasId, fileName);
    }
    
    public void SetCursorStyle(string cursorStyle)
    {
        CheckIsInitialized();
        
        SetCursorStyleJs(this.CanvasId, cursorStyle);
    }
    
    public void SubscribePointerEvents()
    {
        CheckIsInitialized();
        
        SubscribeBrowserEventsJs(this.CanvasId, subscribePointerEvents: true, subscribeRequestAnimationFrame: true);
        ArePointerEventsSubscribed = true;
    }
    
    public void UnsubscribePointerEvents()
    {
        CheckIsInitialized();
        
        UnsubscribeBrowserEventsJs(this.CanvasId, unsubscribePointerEvents: true, unsubscribeRequestAnimationFrame: false);
        ArePointerEventsSubscribed = false;
    }

    public void SetPointerCapture(int pointerId)
    {
        CheckIsInitialized();
        SetPointerCaptureJs(this.CanvasId, pointerId);
    }

    public void ReleasePointerCapture(int pointerId)
    {
        CheckIsInitialized();
        ReleasePointerCaptureJs(this.CanvasId, pointerId);
    }
    
    public async Task<bool> StartSpectorCapture()
    {
        CheckIsInitialized();
        
        if (_isSpectorCaptureStarted)
            return true;
        
        if (!_isSpectorScriptLoaded)
        {
            await JSHost.ImportAsync(moduleName: "spector", moduleUrl: SpectorScriptUrl);
            _isSpectorScriptLoaded = true;
        }
        
        bool success = StartSpectorCaptureJs(this.CanvasId);

        if (success)
            _isSpectorCaptureStarted = true;
        
        return success;
    }

    public void StopSpectorCapture()
    {
        CheckIsInitialized();
        
        if (_isSpectorCaptureStarted)
        {
            StopSpectorCaptureJs();
            _isSpectorCaptureStarted = false;
        }
    }
    
    public void Dispose()
    { 
        Disposing?.Invoke(this, EventArgs.Empty);

        DisconnectWebGLCanvasJs(CanvasId);
        ArePointerEventsSubscribed = false;
        IsWebGLInitialized = false;

        _imageBytesLoadedCallbacks.Clear();

        if (_initialInterop == this)
        {
            _initialInterop = null;

            if (_additionalInteropObjects != null && _additionalInteropObjects.Count > 0)
            {
                // Move first CanvasInterop from _additionalInteropObjects to _initialInterop so it is found faster
                _initialInterop = _additionalInteropObjects[0];
                _additionalInteropObjects.RemoveAt(0);
            }
        }
        else if (_additionalInteropObjects != null)
        {
            _additionalInteropObjects.Remove(this);
        }
        
        if (_additionalInteropObjects != null && _additionalInteropObjects.Count == 0) 
            _additionalInteropObjects = null;
    }

    #region OnPointerButtonPressed and other On... methods
    protected void OnPointerButtonPressed(int changedButton, int pressedButtons, int pointerId, int keyboardModifiers)
    {
        if (PointerDown != null)
            PointerDown(this, new MouseButtonEventArgs((MouseButton)changedButton, (PointerButtons)pressedButtons, pointerId, (KeyboardModifiers)keyboardModifiers));
    }
    
    protected void OnPointerButtonReleased(int changedButton, int pressedButtons, int pointerId, int keyboardModifiers)
    {
        if (PointerUp != null)
            PointerUp(this, new MouseButtonEventArgs((MouseButton)changedButton, (PointerButtons)pressedButtons, pointerId, (KeyboardModifiers)keyboardModifiers));
    }
    
    protected void OnPointerMoved(float x, float y, int buttons, int keyboardModifiers)
    {
        if (PointerMoved != null)
            PointerMoved(this, new MouseMoveEventArgs(x, y, (PointerButtons)buttons, (KeyboardModifiers)keyboardModifiers));
    }
    
    protected void OnMouseWheelChanged(float deltaX, float deltaY, int keyboardModifiers)
    {
        if (MouseWheelChanged != null)
            MouseWheelChanged(this, new MouseWheelEventArgs(deltaX, deltaY, (KeyboardModifiers)keyboardModifiers));
    }
    
    protected void OnPinchZoomStarted(float distance, float centerX, float centerY)
    {
        if (PinchZoomStarted != null)
            PinchZoomStarted(this, new PinchZoomEventArgs(distance, new Vector2(centerX, centerY)));
    }
    
    protected void OnPinchZoomEnded()
    {
        if (PinchZoomEnded != null)
            PinchZoomEnded(this, EventArgs.Empty);
    }
    
    protected void OnPinchZoomed(float distance, float centerX, float centerY)
    {
        if (PinchZoomed != null)
            PinchZoomed(this, new PinchZoomEventArgs(distance, new Vector2(centerX, centerY)));
    }
    
    protected void OnBrowserAnimationFrameUpdated()
    {
        if (BrowserAnimationFrameUpdated != null)
            BrowserAnimationFrameUpdated(this, EventArgs.Empty);
    }
    
    protected void OnCanvasResized(float width, float height, float devicePixelRatio)
    {
        if (CanvasResized != null)
            CanvasResized(this, new CanvasResizedEventArgs(width, height, devicePixelRatio));
    }
    #endregion

    #region JSExport methods: OnFrameUpdateJsCallback, OnPointerMovedJsCallback, ...
    [JSExport]
    private static void OnFrameUpdateJsCallback()
    {
        //if (IsLoggingInteropEvents)
        //    Console.WriteLine("OnFrameUpdate");

        _initialInterop?.OnBrowserAnimationFrameUpdated();
        
        if (_additionalInteropObjects != null)
        {
            foreach (var canvasInterop in _additionalInteropObjects)
                canvasInterop.OnBrowserAnimationFrameUpdated();
        }
    }
    
    [JSExport]
    private static void OnImageBytesLoaded(string canvasId, string imageUrl, int width, int height, byte[]? imageBytes)
    {
        // Console.WriteLine($"OnImageBytesLoaded '{imageUrl}': {width} x {height} = {imageBytes?.Length ?? 0:N0} bytes");

        var canvasInterop = GetCanvasInterop(canvasId);

        if (canvasInterop._imageBytesLoadedCallbacks.Remove(imageUrl, out var callbackAction))
        {
            RawImageData? rawImageData;
            if (imageBytes == null)
                rawImageData = null;
            else
                rawImageData = new RawImageData(width, height, width * 4, PixelFormat.Rgba, imageBytes, checkTransparency: true);
            
            callbackAction(rawImageData);
        }
    }

    [JSExport]
    private static void OnPointerMovedJsCallback(string? canvasId, float x, float y, int buttons, int keyboardModifiers)
    {
        if (IsLoggingInteropEvents)
            Console.WriteLine($"OnPointerMoved '{canvasId ?? ""}': {x} {y}  Buttons: {buttons}  KeyboardModifiers: {keyboardModifiers}");

        var canvasInterop = GetCanvasInterop(canvasId);
        canvasInterop.OnPointerMoved(x, y, buttons, keyboardModifiers);
    }

    [JSExport]
    private static void OnPointerDownJsCallback(string? canvasId, int changedButton, int pressedButtons, int pointerId, int keyboardModifiers)
    {
        if (IsLoggingInteropEvents)
            Console.WriteLine($"OnPointerDown button '{canvasId ?? ""}': {changedButton}  KeyboardModifiers: {keyboardModifiers}");

        var canvasInterop = GetCanvasInterop(canvasId);
        canvasInterop.OnPointerButtonPressed(changedButton, pressedButtons, pointerId, keyboardModifiers);
    }

    [JSExport]
    private static void OnPointerUpJsCallback(string? canvasId, int changedButton, int pressedButtons, int pointerId, int keyboardModifiers)
    {
        if (IsLoggingInteropEvents)
            Console.WriteLine($"OnPointerUp button '{canvasId ?? ""}': {changedButton}  KeyboardModifiers: {keyboardModifiers}");

        var canvasInterop = GetCanvasInterop(canvasId);
        canvasInterop.OnPointerButtonReleased(changedButton, pressedButtons, pointerId, keyboardModifiers);
    }

    [JSExport]
    private static void OnMouseWheelJsCallback(string? canvasId, float deltaX, float deltaY, int keyboardModifiers)
    {
        if (IsLoggingInteropEvents)
            Console.WriteLine($"OnMouseWheel '{canvasId ?? ""}': {deltaX} {deltaY}  KeyboardModifiers: {keyboardModifiers}");

        var canvasInterop = GetCanvasInterop(canvasId);
        canvasInterop.OnMouseWheelChanged(deltaX, deltaY, keyboardModifiers);
    }

    [JSExport]
    private static void OnPinchZoomStartedJsCallback(string? canvasId, float distance, float centerX, float centerY)
    {
        if (IsLoggingInteropEvents)
            Console.WriteLine($"OnPinchZoomStarted '{canvasId ?? ""}': distance: {distance} around ({centerX} {centerY})");

        var canvasInterop = GetCanvasInterop(canvasId);
        canvasInterop.OnPinchZoomStarted(distance, centerX, centerY);
    }

    [JSExport]
    private static void OnPinchZoomEndedJsCallback(string? canvasId)
    {
        if (IsLoggingInteropEvents)
            Console.WriteLine($"OnPinchZoomEnded '{canvasId ?? ""}'");

        var canvasInterop = GetCanvasInterop(canvasId);
        canvasInterop.OnPinchZoomEnded();
    }

    [JSExport]
    private static void OnPinchZoomJsCallback(string? canvasId, float distance, float centerX, float centerY)
    {
        if (IsLoggingInteropEvents)
            Console.WriteLine($"OnPinchZoom '{canvasId ?? ""}': distance: {distance} around ({centerX} {centerY})");

        var canvasInterop = GetCanvasInterop(canvasId);
        canvasInterop.OnPinchZoomed(distance, centerX, centerY);
    }


    [JSExport]
    private static void OnCanvasResizedJsCallback(string? canvasId, float width, float height, float devicePixelRatio)
    {
        if (IsLoggingInteropEvents)
            Console.WriteLine($"OnCanvasResized '{canvasId ?? ""}': {width} {height} {devicePixelRatio}");

        var canvasInterop = GetCanvasInterop(canvasId);
        
        canvasInterop.Width    = (int)width;
        canvasInterop.Height   = (int)height;
        canvasInterop.DpiScale = devicePixelRatio;
        
        canvasInterop.OnCanvasResized(width, height, devicePixelRatio);
    }
    #endregion
    
    #region JSImport methods: InitInteropAsync, InitWebGLCanvasJs, SubscribeBrowserEventsJs, ...

    [JSImport("initInteropAsync", "sharp-engine.js")]
    private static partial Task InitInteropAsync();

    // Returns the string in the format: "OK:width;height;dpiScale" or error text (if text does not start with "OK:")
    // It is not possible (at least in .Net 9) to pass an objects from JS to .Net
    // It was possible to encode width and height into an int, but we also need dpiScale, so we need to pass it as a string.
    [JSImport("initWebGLCanvas", "sharp-engine.js")]
    private static partial string InitWebGLCanvasJs(string canvasId, bool useMSAA, bool subscribePointerEvents, bool subscribeRequestAnimationFrame);

    [JSImport("loadImageBytes", "sharp-engine.js")]
    private static partial void LoadImageBytesJs(string canvasId, string imageUrl);
    
    [JSImport("subscribeBrowserEvents", "sharp-engine.js")]
    private static partial void SubscribeBrowserEventsJs(string canvasId, bool subscribePointerEvents, bool subscribeRequestAnimationFrame);
    
    [JSImport("unsubscribeBrowserEvents", "sharp-engine.js")]
    private static partial void UnsubscribeBrowserEventsJs(string canvasId, bool unsubscribePointerEvents, bool unsubscribeRequestAnimationFrame);

    [JSImport("getCanvasSize", "sharp-engine.js")]
    private static partial int GetCanvasSizeJs(string canvasId, bool useDpiScale);
    
    [JSImport("setCursorStyle", "sharp-engine.js")]
    private static partial int SetCursorStyleJs(string canvasId, string cursorStyle);
    
    [JSImport("setPointerCapture", "sharp-engine.js")]
    private static partial int SetPointerCaptureJs(string canvasId, int pointerId);
    
    [JSImport("releasePointerCapture", "sharp-engine.js")]
    private static partial int ReleasePointerCaptureJs(string canvasId, int pointerId);

    [JSImport("startSpectorCapture", "sharp-engine.js")]
    private static partial bool StartSpectorCaptureJs(string canvasId);

    [JSImport("stopSpectorCapture", "sharp-engine.js")]
    private static partial void StopSpectorCaptureJs();
        
    [JSImport("disconnectWebGLCanvas", "sharp-engine.js")]
    public static partial bool DisconnectWebGLCanvasJs(string canvasId);
    #endregion    
}

//#pragma warning restore CA1416
