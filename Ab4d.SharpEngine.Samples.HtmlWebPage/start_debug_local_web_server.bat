@echo off

set DEBUG_DIR=..\Ab4d.SharpEngine.Samples.WebAssemblyDemo\bin\Debug\net9.0-browser\browser-wasm\AppBundle\_framework

IF NOT EXIST "%DEBUG_DIR%" (
  echo Debug folder bin\Debug\net9.0-browser\browser-wasm\AppBundle\_framework does not exist
  echo Call compile_debug_version.bat or compile Ab4d.SharpEngine.Samples.WebAssemblyDemo for DEBUG configuration in the IDE to generate the files.
  pause
  exit 1
)

xcopy ..\Ab4d.SharpEngine.Samples.AspNetCoreApp\wwwroot\*.* .\wwwroot\ /Y
xcopy %DEBUG_DIR%\*.* .\wwwroot\_framework\ /Y /S

start "" http:/localhost:8000/index.html
python serve.py
