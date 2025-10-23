@echo off

set PUBLISH_DIR=..\Ab4d.SharpEngine.Samples.WebAssemblyDemo\bin\Release\net9.0-browser\browser-wasm\AppBundle\_framework

IF NOT EXIST "%PUBLISH_DIR%" (
  echo Publish folder bin\Release\net9.0-browser\browser-wasm\AppBundle\_framework does not exist
  echo Call compile_publish_version.bat to generate the files
  pause
  exit 1
)

xcopy ..\Ab4d.SharpEngine.Samples.AspNetCoreApp\wwwroot\*.* .\wwwroot\ /Y
xcopy %PUBLISH_DIR%\*.* _framework\ /Y /S

start "" http:/localhost:8000/index.html
python serve.py

