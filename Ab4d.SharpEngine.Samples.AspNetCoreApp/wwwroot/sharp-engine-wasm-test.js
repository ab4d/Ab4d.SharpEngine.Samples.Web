// The code in this file does the following:
// - loads and initializes the dotnet runtime
// - calls .Net method JavaScriptInterop.InitSharpEngineJSCallback that initializes the Ab4d.SharpEngine
// - provides functions that can call .Net methods and are exported to javascript on the main web page

console.log("js: Starting sharp-engine-wasm-test.js");

// Set up the .NET WebAssembly runtime
import { dotnet } from './_framework/dotnet.js'

// Get exported methods from the .NET assembly
const { getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .create();

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

// Initialize the WebAssembly code that use SharpEngine
console.log("js: calling JavaScriptInterop.InitSharpEngineJSCallback");
exports.WebGLWebAssemblyTest.JavaScriptInterop.InitSharpEngineJSCallback();


// The following functions are called from the javascript on the index.html page

export function renderScene() {
    console.log("js: renderScene");
    exports.WebGLWebAssemblyTest.JavaScriptInterop.RenderSceneJSCallback();
}

export function toggleCameraRotation() {
    console.log("js: toggleCameraRotation");
    exports.WebGLWebAssemblyTest.JavaScriptInterop.ToggleCameraRotationJSCallback();
}

export function dumpSceneInfo() {
    console.log("js: dumpSceneInfo");
    exports.WebGLWebAssemblyTest.JavaScriptInterop.DumpSceneInfoJSCallback();
}
