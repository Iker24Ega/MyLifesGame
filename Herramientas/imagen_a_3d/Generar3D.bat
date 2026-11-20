@echo off
chcp 65001 >nul
title My Life's Game - Imagen a 3D
cd /d "%~dp0"

rem --- Buscar Python ---
set "PY="
py -3 --version >nul 2>nul && set "PY=py -3"
if not defined PY python --version >nul 2>nul && set "PY=python"
if not defined PY (
  echo No encuentro Python en este ordenador.
  echo Instalalo con este comando en una terminal y vuelve a abrir este archivo:
  echo     winget install -e --id Python.Python.3.12
  echo O descargalo de python.org marcando "Add python.exe to PATH".
  pause
  exit /b 1
)

rem --- Entorno propio (solo la primera vez) ---
if not exist ".venv\Scripts\python.exe" (
  echo Preparando el entorno la primera vez...
  %PY% -m venv .venv
)
".venv\Scripts\python.exe" -m pip install -q --disable-pip-version-check --upgrade gradio_client huggingface_hub pillow

rem --- Token de Hugging Face (solo la primera vez) ---
if not exist "hf_token.txt" (
  echo.
  echo Pega tu token de Hugging Face ^(huggingface.co/settings/tokens, tipo "Read"^).
  echo Con token tienes mas usos gratis al dia. Pulsa Enter sin escribir nada para seguir sin el.
  set /p "TOKEN=Token: "
)
if not exist "hf_token.txt" if defined TOKEN (echo %TOKEN%> hf_token.txt)

echo.
".venv\Scripts\python.exe" imagen_a_3d.py %*
echo.
pause
