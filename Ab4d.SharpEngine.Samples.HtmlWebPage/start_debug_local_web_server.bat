@echo off

set DEBUG_DIR=..\Ab4d.SharpEngine.Samples.WebAssemblyDemo\bin\Debug\net9.0-browser\browser-wasm\AppBundle\_framework

IF NOT EXIST "%DEBUG_DIR%" (
  echo Debug folder bin\Debug\net9.0-browser\browser-wasm\AppBundle\_framework does not exist
  echo Compile Ab4d.SharpEngine.Samples.WebAssemblyDemo for DEBUG configuration to generate the files.
  pause
  exit 1
)

xcopy %DEBUG_DIR%\*.* _framework\ /Y /S

start "" http:/localhost:8000/index.html
python serve.py
