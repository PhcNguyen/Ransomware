@echo off

REM Thiết lập các biến môi trường
set PROJECT_NAME=ransomeware
set BUILD_CONFIG=Release
set PROJECT_PATH="C:\Coding\Cs\Ransomware"  REM Đường dẫn tới thư mục chứa file csproj

REM Mảng chứa các hệ điều hành và tên thư mục đầu ra
setlocal EnableDelayedExpansion
set platforms[win-x64]=windows
set platforms[linux-x64]=linux
set platforms[osx-x64]=macos
set platforms[linux-arm]=raspberry

echo Bắt đầu quá trình xây dựng...

for %%p in (win-x64 linux-x64 osx-x64 linux-arm) do (
    call dotnet publish %PROJECT_PATH% -c %BUILD_CONFIG% -r %%p --self-contained
    if ERRORLEVEL 1 (
        echo Lỗi khi xây dựng cho !platforms[%%p]!
    ) else (
        echo Build cho !platforms[%%p]! hoàn tất!
    )
)

echo Tất cả quá trình xây dựng đã hoàn tất!
pause
