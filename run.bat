@echo off
echo This script must be run from the solution folder.
echo Running "dotnet watch" in new command window, which should in turn start the browser...

start cmd /c "dotnet watch --verbose --project ./TradeCars/Server"
:end