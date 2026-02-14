@echo off

IF EXIST wwwroot\_framework\ (
  del wwwroot\_framework\*.* /q
  del wwwroot\_framework\supportFiles\*.* /q
) ELSE (
  md wwwroot
  md wwwroot\_framework
)

cd ..\Ab4d.SharpEngine.Samples.WebAssemblyDemo
dotnet publish -c Release

cd ..\Ab4d.SharpEngine.Samples.HtmlWebPage

xcopy ..\Ab4d.SharpEngine.Samples.AspNetCoreApp\wwwroot\*.* wwwroot\ /Y
xcopy ..\Ab4d.SharpEngine.Samples.WebAssemblyDemo\bin\Release\net10.0-browser\browser-wasm\AppBundle\_framework\*.* wwwroot\_framework\ /Y /S

IF EXIST "..\ThirdParty\brotli\brotli.exe" (
  for %%f in (wwwroot\_framework\*.wasm) do (
    echo "Compressing: %%f"
    ..\ThirdParty\brotli\brotli -f %%f
  )

  for %%f in (wwwroot\_framework\*.js) do (
    echo "Compressing: %%f"
    ..\ThirdParty\brotli\brotli -f %%f
   )
)

pause

