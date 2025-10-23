using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();



// Serve default wwwroot
app.UseFileServer();


// Serve also the output folder of the compiled WebGLWebAssemblyTest project:

#if DEBUG
string build = "Debug";
#else
string build = "Release";
#endif

string path = Path.Combine(builder.Environment.ContentRootPath, $"../Ab4d.SharpEngine.Samples.WebAssemblyDemo/bin/{build}/net9.0-browser/browser-wasm/AppBundle");

var options = new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(path),
    EnableDefaultFiles = true
};

// Add .pdb MIME type (if this is not added then pdb files are not served and this produces an error when checking integrity)
// NOTE: This still does not provide debugging support (to be able to put a breakpoint into the c# code)
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".pdb"] = "application/octet-stream";
options.StaticFileOptions.ContentTypeProvider = provider;

app.UseFileServer(options);

app.Run();

// ADDITIONAL INFO:
// To compile the WebAssembly project (Ab4d.SharpEngine.Samples.WebAssemblyDemo) when stating this Asp.Net Core project,
// we need to manually add project dependency to Ab4d.SharpEngine.Samples.WebAssemblyDemo.
//
// This is done in Visual Studio by clicking the little down arrow right to the start "https" button.
// Then select "Configure startup projects..." and select "Project dependencies".
// In the Project dropdown select the Asp.Net project and check the Ab4d.SharpEngine.Samples.WebAssemblyDemo project.
// This will compile (if not already up to date) the Ab4d.SharpEngine.Samples.WebAssemblyDemo project before starting this Asp.Net Core project.
