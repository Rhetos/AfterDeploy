SETLOCAL
SET Version=1.2.0
SET Prerelease=auto

REM Find Visual Studio 2015 or newer. VS2017 uses vswhere.exe for locating, VS2015 uses %VS140COMNTOOLS%.
@IF DEFINED VisualStudioVersion GOTO EndVcvarsall
FOR /f "usebackq delims=" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -prerelease -latest -property installationPath`) do (
  IF EXIST "%%i\Common7\Tools\VsDevCmd.bat" (CALL "%%i\Common7\Tools\VsDevCmd.bat" && GOTO EndVcvarsall || GOTO Error0)
)
IF "%VS140COMNTOOLS%" NEQ "" (CALL "%VS140COMNTOOLS%VsDevCmd.bat" x86 && GOTO EndVcvarsall || GOTO Error0)
@ECHO ERROR: Cannot find Visual Studio.
@GOTO Error0
:EndVcvarsall
@ECHO ON

PowerShell .\ChangeVersion.ps1 %Version% %Prerelease% || GOTO Error0
WHERE /Q NuGet.exe || ECHO ERROR: Please download the NuGet.exe command line tool. && GOTO Error0
NuGet restore -NonInteractive || GOTO Error0
MSBuild /target:rebuild /p:Configuration=Debug /verbosity:minimal /fileLogger || GOTO Error0
IF NOT EXIST Install md Install
NuGet pack -OutputDirectory Install || GOTO Error0
REM Updating the version of all projects back to "dev" (internal development build), to avoid spamming git history with timestamped prerelease versions.
PowerShell .\ChangeVersion.ps1 %Version% dev || GOTO Error0

@REM ================================================

@ECHO.
@ECHO %~nx0 SUCCESSFULLY COMPLETED.
@EXIT /B 0

:Error0
@ECHO.
@ECHO %~nx0 FAILED.
@IF /I [%1] NEQ [/NOPAUSE] @PAUSE
@EXIT /B 1
