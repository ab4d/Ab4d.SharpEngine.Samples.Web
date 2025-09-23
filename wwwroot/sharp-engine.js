let initialCanvas;
let interop;
let spector;
let resizeObserver;
let canvasToDisplaySizeMap;
let isLogging = true;
let isUpdating = false;
let isPinchZooming = false;

export async function initInterop() {
    log("js: initInterop");

    const dotnet = globalThis.getDotnetRuntime(0);

    var config = dotnet.getConfig();
    var exports = await dotnet.getAssemblyExports(config.mainAssemblyName);

    //IMORTANT:
    //If we change the namespace or class name of the Ab4d.SharpEngine.WebGL.CanvasInterop,
    //then we also need to change the following line:
    interop = exports.Ab4d.SharpEngine.WebGL.CanvasInterop;

    log("js: .Net interop with CanvasInterop initialized");
}

export function initWebGLCanvas(canvasId, useMSAA, subscribeMouseEvents, subscribeRequestAnimationFrame) {
    log("js: initWebGLCanvas canvasId:" + canvasId);

    const canvas = globalThis.document.getElementById(canvasId);

    if (canvas)
    {
        if (!initialCanvas)
            initialCanvas = canvas;

        var context = canvas.getContext('webgl2', { antialias: useMSAA });

        if (!context)
        {
            var errorMessage = "WebGL2 is not supported";
            console.error(errorMessage);
            return errorMessage;
        }

        const dotnet = globalThis.getDotnetRuntime(0);
        dotnet.Module["canvas"] = canvas; // This is requried to be able to create WebGL context (call EGL.CreateContext)

        var dpi = window.devicePixelRatio || 1.0;
        var displayWidth, displayHeight;

        // When user did not manually set the size of the WebGL back buffers (by setting width and height properties of canvas),
        // then we need to set the size of the back buffers to match the displayed size of the canvas. This is done by ResizeObserver.
        // NOTE that the default width and height of canvas is 300 x 150 pixels.
        if (canvas.width == 300 && canvas.height == 150)
        {
            var rect = canvas.getBoundingClientRect(); // This will get the size in float values without rounding (as with clientWidth/clientHeight)
            displayWidth  = Math.round(rect.width * dpi);
            displayHeight = Math.round(rect.height * dpi);

            // Set initial size of the back buffers by setting the width and height properties of canvas
            canvas.width  = displayWidth;
            canvas.height = displayHeight;

            // Start observing the canvas size changes
            // ResizeObserver is more accurate than other methods - see https://webglfundamentals.org/webgl/lessons/webgl-resizing-the-canvas.html
            if (!resizeObserver)
                resizeObserver = new ResizeObserver(onResize);

            resizeObserver.observe(canvas); // use default: { box: 'content-box' });
        }
        else
        {
            // If we subscribe to ResizeObserver when user has set the size of the back buffers,
            // then this will recursively call resizeCanvasToDisplaySize increasing the size each time until max size is reached and the browser throws an error.
            displayWidth  = canvas.width;
            displayHeight = canvas.height;
            dpi = 1; // When back buffer size is manually set, we must not apply any dpi scale (this would break hit-testing)
        }

        // We use a Map to store the current display size of the canvas.
        if (!canvasToDisplaySizeMap)
            canvasToDisplaySizeMap = new Map();

        canvasToDisplaySizeMap.set(canvas, [displayWidth, displayHeight]); // Set initial size

        subscribeBrowserEventsInt(canvas, subscribeMouseEvents, subscribeRequestAnimationFrame);

        // Return the size as a string in format: "OK:width;height;dpiScale"
        // It is not possible (at least in .Net 9) to pass an objects for JS to .Net
        // It was possible to encode width and height into an int, but we also need dpiScale,
        // so we need to pass it as a string
        return "OK:" + displayWidth + ";" + displayHeight + ";" + dpi;
    }
    else
    {
        var errorMessage = "Canvas not found: " + canvasId;
        console.error(errorMessage);
        return errorMessage;
    }
}

// NOTE:
// We cannot subscribe to mouse events in SetWebGLCanvas because this requires async method
// because we need to call "await getAssemblyExports" (without awit the exports are empyt).
// But if SetWebGLCanvas is async then it returns no result to .Net and we get an error that "resut is not a string".
// Therefore we require two methods: SetWebGLCanvas and SubscribeBrowserEvents
export function subscribeBrowserEvents(canvasId, subscribeMouseEvents, subscribeRequestAnimationFrame) {
    log("js: subscribeBrowserEvents canvasId:", canvasId);

    let canvas = getCanvas(canvasId);
    subscribeBrowserEventsInt(canvas, subscribeMouseEvents, subscribeRequestAnimationFrame);
}

export function unsubscribeBrowserEvents(canvasId, unsubscribeMouseEvents, unsubscribeRequestAnimationFrame) {
    log("js: unsubscribeBrowserEvents canvasId:", canvasId);

    if (!interop)
        return;

    if (unsubscribeMouseEvents)
    {
        const canvas = getCanvas(canvasId);

        if (canvas) {
            canvas.removeEventListener("pointermove", pointerMove, false);
            canvas.removeEventListener("pointerdown", pointerDown, false);
            canvas.removeEventListener("pointerup", pointerUp, false);
            canvas.removeEventListener("touchstart", touchStart, false);
            canvas.removeEventListener("touchmove", touchMove, false);
            canvas.removeEventListener("touchend", touchEnd, false);
            canvas.removeEventListener("wheel", mouseWheel, false);

            log("js: mouse events unsubscribed");
        }
    }

    if (unsubscribeRequestAnimationFrame)
        isUpdating = false;
}

export function startSpectorCapture(canvasId) {
    log("js: startSpectorCapture:" + canvasId);

    const canvas = getCanvas(canvasId);

    if (!spector) {
        if (typeof SPECTOR == "undefined")
            return false;

        spector = new SPECTOR.Spector();
    }

    spector.displayUI();
    spector.startCapture(canvas, 100000); // 100000 is the max amount of commands to capture
    return true;
}

export function stopSpectorCapture() {
    log("js: stopSpectorCapture");

    if (spector)
        spector.stopCapture();
}

export function setCursorStyle(canvasId, cursorStyle) {
    log("js: setCursorStyle canvasId:" + canvasId + " to " + cursorStyle);

    const canvas = getCanvas(canvasId);

    if (canvas)
        canvas.style.cursor = cursorStyle;
}

export function setPointerCapture(canvasId, pointerId) {
    log("js: setPointerCapture canvasId:" + canvasId);

    const canvas = getCanvas(canvasId);

    if (canvas) {
        try {
            canvas.setPointerCapture(pointerId);
        }
        catch { } // prevent an error when pointerId event was already finished and does not exist anymore when this is called
    }
}

export function releasePointerCapture(canvasId, pointerId) {
    log("js: releasePointerCapture canvasId:" + canvasId);

    const canvas = getCanvas(canvasId);

    if (canvas) {
        try {
            canvas.releasePointerCapture(pointerId);
        }
        catch { } // prevent an error when pointerId event was already finished and does not exist anymore when this is called
    }
}

export function disconnectWebGLCanvas(canvasId) {
    log("js: disconnectWebGLCanvas canvasId:" + canvasId);

    if (!resizeObserver) {
        resizeObserver.observe(canvas);
        resizeObserver = null;
    }

    unsubscribeBrowserEvents(canvasId, true, true);

    const canvas = getCanvas(canvasId);

    if (canvas) {
        const dotnet = globalThis.getDotnetRuntime(0);
        if (dotnet && dotnet.Module["canvas"] === canvas)
            dotnet.Module["canvas"] = null;

        canvasToDisplaySizeMap.delete(canvas)
    }
}


function onFrameUpdate()
{
    if (!interop || !isUpdating)
        return;

    interop.OnFrameUpdateJsCallback();

    requestAnimationFrame(onFrameUpdate);
}

// From https://webglfundamentals.org/webgl/lessons/webgl-resizing-the-canvas.html
function onResize(entries) {
    for (const entry of entries) {
        let width;
        let height;
        let dpr = window.devicePixelRatio;
        if (entry.devicePixelContentBoxSize) {
            // NOTE: Only this path gives the correct answer
            // The other 2 paths are an imperfect fallback
            // for browsers that don't provide anyway to do this
            width = entry.devicePixelContentBoxSize[0].inlineSize;
            height = entry.devicePixelContentBoxSize[0].blockSize;
            dpr = 1; // it's already in width and height
        } else if (entry.contentBoxSize) {
            if (entry.contentBoxSize[0]) {
                width = entry.contentBoxSize[0].inlineSize;
                height = entry.contentBoxSize[0].blockSize;
            } else {
                // legacy
                width = entry.contentBoxSize.inlineSize;
                height = entry.contentBoxSize.blockSize;
            }
        } else {
            // legacy
            width = entry.contentRect.width;
            height = entry.contentRect.height;
        }

        const canvas = entry.target;
        const displayWidth = Math.round(width * dpr);
        const displayHeight = Math.round(height * dpr);

        const [oldWidth, oldHeight] = canvasToDisplaySizeMap.get(canvas) || [0, 0];

        if (displayWidth !== oldWidth || displayHeight !== oldHeight) {
            canvasToDisplaySizeMap.set(canvas, [displayWidth, displayHeight]);

            // Make the size of canvas back buffers the corect size
            canvas.width = displayWidth;
            canvas.height = displayHeight;

            if (interop)
                interop.OnCanvasResizedJsCallback(canvas.id, displayWidth, displayHeight, window.devicePixelRatio); // report size change to render the next frame
        }
    }
}

function subscribeBrowserEventsInt(canvas, subscribeMouseEvents, subscribeRequestAnimationFrame) {
    if (subscribeMouseEvents && canvas) {
        canvas.addEventListener("pointermove", pointerMove, false);
        canvas.addEventListener("pointerdown", pointerDown, false);
        canvas.addEventListener("pointerup", pointerUp, false);
        canvas.addEventListener("touchstart", touchStart, false);
        canvas.addEventListener("touchmove", touchMove, false);
        canvas.addEventListener("touchend", touchEnd, false);
        canvas.addEventListener("wheel", mouseWheel, false);

        log("js: mouse events subscribed");
    }

    if (subscribeRequestAnimationFrame && !isUpdating) {
        isUpdating = true;
        requestAnimationFrame(onFrameUpdate);
    }
}

function getCanvas(canvasId) {
    if (initialCanvas && (!canvasId || initialCanvas.id === canvasId))
        return initialCanvas;

    const canvas = globalThis.document.getElementById(canvasId);

    if (!canvas)
        console.error("Canvas not found: " + canvasId);

    return canvas;
}

function getKeyboardModifiers(e) {
    return (e.shiftKey ? 4 : 0) + (e.ctrlKey ? 2 : 0) + (e.altKey ? 1 : 0); // See Ab4d.SharpEngine.Common.KeyboardModifiers
}

function log(message) {
    if (isLogging)
        console.log(message);
}

function checkPinch(e, callPinchZoom) {
    var isPinchZoomStarted = false;

    if (e.touches.length === 2) {
        if (!isPinchZooming) {
            isPinchZooming = true;
            isPinchZoomStarted = true;
        }
    }
    else {
        if (isPinchZooming) {
            isPinchZooming = false;
            interop.OnPinchZoomEndedJsCallback(e.currentTarget.id);
        }
    }

    if (isPinchZooming) {
        var pinchDistance = Math.hypot(e.touches[0].pageX - e.touches[1].pageX, e.touches[0].pageY - e.touches[1].pageY);

        if (pinchDistance > 0) {
            var centerX = (e.touches[0].clientX + e.touches[1].clientX) * 0.5;
            var centerY = (e.touches[0].clientY + e.touches[1].clientY) * 0.5;

            const rect = e.touches[0].target.getBoundingClientRect();
            centerX -= rect.left;
            centerY -= rect.top;

            if (isPinchZoomStarted)
                interop.OnPinchZoomStartedJsCallback(e.currentTarget.id, pinchDistance, centerX, centerY);
            else if (callPinchZoom)
                interop.OnPinchZoomJsCallback(e.currentTarget.id, pinchDistance, centerX, centerY);
        }
    }
}


const pointerMove = (e) => {
    if (!interop)
        return;

    e.preventDefault(); // Prevent sending mouseMove

    if (isPinchZooming)
        return;

    interop.OnPointerMovedJsCallback(e.currentTarget.id, e.offsetX, e.offsetY, e.buttons, getKeyboardModifiers(e));
}

const pointerDown = (e) => {
    if (!interop)
        return;

    e.preventDefault(); // Prevent sending mouseDown

    if (isPinchZooming)
        return;

    interop.OnPointerDownJsCallback(e.currentTarget.id, e.button, e.buttons, e.pointerId, getKeyboardModifiers(e));
}

const pointerUp = (e) => {
    if (!interop)
        return;

    e.preventDefault(); // Prevent sending mouseUp

    if (isPinchZooming)
        return;

    interop.OnPointerUpJsCallback(e.currentTarget.id, e.button, e.buttons, e.pointerId, getKeyboardModifiers(e));
}

const mouseWheel = (e) => {
    if (!interop)
        return;

    e.preventDefault();

    interop.OnMouseWheelJsCallback(e.currentTarget.id, e.deltaX, e.deltaY, getKeyboardModifiers(e));
}

const touchStart = (e) => {
    if (!interop)
        return;

    e.preventDefault();

    if (e.touches.length === 1) {
        var button = 0;
        var touch = e.changedTouches[0];
        var bcr = e.target.getBoundingClientRect();
        var x = touch.clientX - bcr.x;
        var y = touch.clientY - bcr.y;
        var keyboardModifiers = getKeyboardModifiers(e);

        interop.OnPointerMovedJsCallback(e.currentTarget.id, x, y, e.buttons, keyboardModifiers);
        interop.OnPointerDownJsCallback(e.currentTarget.id, button, keyboardModifiers);
    }

    checkPinch(e, false);
}

const touchMove = (e) => {
    if (!interop)
        return;

    e.preventDefault();

    checkPinch(e, true);

     if (e.touches.length === 1) {
        var touch = e.changedTouches[0];
        var bcr = e.target.getBoundingClientRect();
        var x = touch.clientX - bcr.x;
        var y = touch.clientY - bcr.y;

        interop.OnPointerMovedJsCallback(e.currentTarget.id, x, y, 1, getKeyboardModifiers(e));
    }
}

const touchEnd = (e) => {
    if (!interop)
        return;

    e.preventDefault();

    if (e.touches.length === 0) {
        var button = 0;
        var touch = e.changedTouches[0];
        var bcr = e.target.getBoundingClientRect();
        var x = touch.clientX - bcr.x;
        var y = touch.clientY - bcr.y;
        var keyboardModifiers = getKeyboardModifiers(e);

        interop.OnPointerMovedJsCallback(e.currentTarget.id, x, y, e.buttons, keyboardModifiers);
        interop.OnPointerUpJsCallback(e.currentTarget.id, button, keyboardModifiers);
    }
    
    checkPinch(e, false);
}
