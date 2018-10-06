@ECHO OFF

SETLOCAL EnableDelayedExpansion
FOR /F "tokens=1,2 delims=#" %%a IN ('"PROMPT #$H#$E# & ECHO on & for %%b in (1) do rem"') do (
  SET "DEL=%%a"
)

CALL :COLOR_TEXT 0E "WARNING Please use 'OneBuild.bat' to run OneBuild, 'Build.bat' will be removed from OneBuild 1.1.x onwards"
ECHO .

rem the following resets %ERRORLEVEL% to 0 prior to running powershell
VERIFY >NUL
ECHO. %ERRORLEVEL%

rem setting defaults
SET TASK=Invoke-Commit
SET CONFIGURATION=Debug
SET BUILDCOUNTER=999


:PARAM_LOOP_START
IF [%1] == [] GOTO PARAM_LOOP_END;

IF [%1] == [-task] (
	SET TASK=%2
	SHIFT /1
) ELSE IF [%1] == [-buildcounter] (
	SET BUILDCOUNTER=%2
) ELSE IF [%1] == [-configuration] (
	SET CONFIGURATION=%2
	SHIFT /1
)
SHIFT /1
GOTO PARAM_LOOP_START
:PARAM_LOOP_END


ECHO task = %TASK%
ECHO configuration = %CONFIGURATION%
ECHO buildcounter = %BUILDCOUNTER%

powershell -NoProfile -ExecutionPolicy bypass -command ".\packages\invoke-build.2.9.12\tools\Invoke-Build.ps1 %TASK% -configuration %CONFIGURATION% -buildCounter %BUILDCOUNTER% .\OneBuild.build.ps1"

IF %ERRORLEVEL% == 0 GOTO OK
ECHO ##teamcity[buildStatus status='FAILURE' text='{build.status.text} in execution']
EXIT /b %ERRORLEVEL%

:OK
EXIT

:COLOR_TEXT
ECHO OFF
<NUL SET /p ".=%DEL%" > "%~2"
findstr /v /a:%1 /R "^$" "%~2" NUL
DEL "%~2" > NUL 2>&1
