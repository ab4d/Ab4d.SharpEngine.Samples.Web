using System.Runtime.InteropServices.JavaScript;

namespace WebGLWebAssemblyTest;

public static partial class TestInterop
{
    // Make the method accessible from JS
    [JSExport]
    internal static int Add(int a, int b)
    {
        return a + b;
    }

    [JSExport]
    public static string SayHello(string name)
    {
        return $"Hello from WASM, {name}!";
    }
}
