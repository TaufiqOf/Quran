#!/usr/bin/env bash

set -euo pipefail

# ============================================================
# Quran - Windows single EXE build script
#
# Run this script from Linux:
#     ./build-windows.sh
#
# Result:
#     dist/windows/Quran.exe
#
# The executable is:
#   - Windows x64
#   - Self-contained
#   - Single file
#   - Includes the application icon
# ============================================================

APP_NAME="Quran"
PROJECT_FILE="Quran.csproj"
RUNTIME="win-x64"
CONFIGURATION="Release"
FRAMEWORK="net10.0"

# Application icon
ICON_FILE="Assets/Icons/quran.ico"

OUTPUT_DIR="dist/windows"
PUBLISH_DIR="$OUTPUT_DIR/publish"
EXE_FILE="$OUTPUT_DIR/$APP_NAME.exe"

echo
echo "============================================================"
echo " Building $APP_NAME for Windows"
echo "============================================================"
echo

# ------------------------------------------------------------
# Check prerequisites
# ------------------------------------------------------------

if ! command -v dotnet >/dev/null 2>&1; then
    echo "ERROR: dotnet was not found."
    echo "Install the .NET SDK first."
    exit 1
fi

if [ ! -f "$PROJECT_FILE" ]; then
    echo "ERROR: $PROJECT_FILE was not found."
    echo "Run this script from the Quran project directory."
    exit 1
fi

# ------------------------------------------------------------
# Check icon
# ------------------------------------------------------------

if [ ! -f "$ICON_FILE" ]; then
    echo
    echo "ERROR: Application icon was not found:"
    echo "  $ICON_FILE"
    echo
    echo "Create the icon first."
    exit 1
fi

# ------------------------------------------------------------
# Clean previous build
# ------------------------------------------------------------

echo "==> Cleaning previous Windows build..."

rm -rf "$OUTPUT_DIR"

mkdir -p "$PUBLISH_DIR"

# ------------------------------------------------------------
# Restore
# ------------------------------------------------------------

echo
echo "==> Restoring dependencies..."

dotnet restore "$PROJECT_FILE" \
    -r "$RUNTIME"

# ------------------------------------------------------------
# Publish
# ------------------------------------------------------------

echo
echo "==> Publishing application..."
echo
echo "    Application   : $APP_NAME"
echo "    Configuration : $CONFIGURATION"
echo "    Runtime       : $RUNTIME"
echo "    Framework     : $FRAMEWORK"
echo "    Self-contained: true"
echo "    Single file   : true"
echo "    Icon          : $ICON_FILE"
echo

dotnet publish "$PROJECT_FILE" \
    -c "$CONFIGURATION" \
    -f "$FRAMEWORK" \
    -r "$RUNTIME" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:EnableCompressionInSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -p:ApplicationIcon="$ICON_FILE" \
    -o "$PUBLISH_DIR"

# ------------------------------------------------------------
# Find the generated executable
# ------------------------------------------------------------

GENERATED_EXE="$PUBLISH_DIR/$APP_NAME.exe"

if [ ! -f "$GENERATED_EXE" ]; then
    echo
    echo "ERROR: $GENERATED_EXE was not created."
    echo
    echo "Files generated:"
    find "$PUBLISH_DIR" -maxdepth 1 -type f -printf '  %f\n'
    exit 1
fi

# ------------------------------------------------------------
# Move the single EXE to dist/windows
# ------------------------------------------------------------

mv "$GENERATED_EXE" "$EXE_FILE"

# Remove the publish directory.
# The final output is intentionally only the EXE.
rm -rf "$PUBLISH_DIR"

# ------------------------------------------------------------
# Show result
# ------------------------------------------------------------

echo
echo "============================================================"
echo " Build completed successfully!"
echo "============================================================"
echo

echo "Windows executable:"
echo "  $EXE_FILE"

echo
echo "Application icon:"
echo "  $ICON_FILE"

echo

ls -lh "$EXE_FILE"

echo
echo "The executable is:"
echo "  ✓ Self-contained"
echo "  ✓ Single-file"
echo "  ✓ Windows x64"
echo "  ✓ Application icon embedded"
echo
echo "The executable does not require"
echo "the .NET runtime to be installed on Windows."
echo
