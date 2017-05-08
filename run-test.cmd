@echo off
::cd test\Dict.Models.UnitTest
::dotnet xunit
::cd ..\..

FOR %%a IN (%*) do (
	cd %%a
	dotnet xunit
	cd %~dp0
)