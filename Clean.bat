@FOR /F "delims=" %%i IN ('dir bin /s/b/ad') DO DEL /F/S/Q "%%i" >nul & RD /S/Q "%%i"
@FOR /F "delims=" %%i IN ('dir obj /s/b/ad') DO DEL /F/S/Q "%%i" >nul & RD /S/Q "%%i"
@REM Question mark is here to prevent an issue with 'dir' not listing the subfolders if there is a folder with the same name in the project root folder.
@FOR /F "delims=" %%i IN ('dir TestResult? /s/b/ad') DO DEL /F/S/Q "%%i" >nul & RD /S/Q "%%i"
IF EXIST packages DEL /F/S/Q packages >nul
IF EXIST packages RD /S/Q packages
