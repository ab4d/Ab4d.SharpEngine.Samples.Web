@echo off

IF EXIST wwwroot\_framework\ (
  del wwwroot\_framework\*.* /q
  del wwwroot\_framework\supportFiles\*.* /q
) ELSE (
  md wwwroot
  md wwwroot\_framework
)

cd ..\Ab4d.SharpEngine.Samples.WebAssemblyDemo
dotnet build Ab4d.SharpEngine.Samples.WebAssemblyDemo.csproj -c Debug


cd ..\Ab4d.SharpEngine.Samples.HtmlWebPage

xcopy ..\Ab4d.SharpEngine.Samples.AspNetCoreApp\wwwroot\*.* wwwroot\ /Y
xcopy ..\Ab4d.SharpEngine.Samples.WebAssemblyDemo\bin\Debug\net10.0-browser\browser-wasm\AppBundle\_framework\*.* wwwroot\_framework\ /Y /S

pause

