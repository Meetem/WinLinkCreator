@echo off
SETLOCAL
SET mystring=%~dp0
SET mystring=%mystring:\=\\%

echo Windows Registry Editor Version 5.00 > registerWinLink.reg

echo [HKEY_CLASSES_ROOT\Directory\shell\winlnk] >> registerWinLink.reg
echo @="Create Sym&Link To This" >> registerWinLink.reg

echo [HKEY_CLASSES_ROOT\Directory\shell\winlnk\command] >> registerWinLink.reg
echo @="\"%mystring%WinLinkCreator.exe\" \"%%1\"" >> registerWinLink.reg

start registerWinLink.reg