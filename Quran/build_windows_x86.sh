#!/usr/bin/env bash

set -euo pipefail

# ============================================================
# Quran - Windows single EXE build script
#
# Run this script from Linux:
#     ./build-windows.sh
#
# Result:
#     dist/windows/Quran-windows-x64.zip
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
EXE_FILE="$PUBLISH_DIR/$APP_NAME.exe"
ZIP_FILE="$OUTPUT_DIR/${APP_NAME}-windows-x64.zip"

echo
echo "============================================================"
echo " Building $APP_NAME for Windows"
echo "============================================================"
echo

# ------------------------------------------------------------
# Check prerequisites
# ------------------------------------------------------------

MISSING_DEPS=()
for cmd in dotnet wget unzip zip find cp rm mkdir; do
    if ! command -v "$cmd" >/dev/null 2>&1; then
        MISSING_DEPS+=("$cmd")
    fi
done

if [ ${#MISSING_DEPS[@]} -ne 0 ]; then
    echo "ERROR: Missing required utilities: ${MISSING_DEPS[*]}"
    echo "Install missing tools (e.g., sudo apt install zip unzip wget)"
    exit 1
fi

if [ ! -f "$PROJECT_FILE" ]; then
    echo "ERROR: $PROJECT_FILE was not found."
    echo "Run this script from the Quran project directory."
    exit 1
fi

if [ ! -f "$ICON_FILE" ]; then
    echo
    echo "ERROR: Application icon was not found:"
    echo "  $ICON_FILE"
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
# Check generated executable
# ------------------------------------------------------------

if [ ! -f "$EXE_FILE" ]; then
    echo
    echo "ERROR: $EXE_FILE was not created."
    echo
    echo "Files generated:"
    find "$PUBLISH_DIR" -maxdepth 1 -type f -printf '  %f\n'
    exit 1
fi

# ------------------------------------------------------------
# Download and Extract Native LibVLC DLLs into Publish Directory
# ------------------------------------------------------------

echo "==> Fetching native LibVLC Windows runtime binaries..."

LIBVLC_VERSION="3.0.21"
LIBVLC_ZIP="$OUTPUT_DIR/vlc-${LIBVLC_VERSION}-win64.zip"
LIBVLC_URL="https://get.videolan.org/vlc/${LIBVLC_VERSION}/win64/vlc-${LIBVLC_VERSION}-win64.zip"

if [ ! -f "$LIBVLC_ZIP" ]; then
    wget -q --show-progress -O "$LIBVLC_ZIP" "$LIBVLC_URL"
fi

# Extract native runtime DLLs and plugins directly into the publish folder alongside the .exe
unzip -q -o "$LIBVLC_ZIP" "vlc-${LIBVLC_VERSION}/libvlc.dll" -d "$PUBLISH_DIR"
unzip -q -o "$LIBVLC_ZIP" "vlc-${LIBVLC_VERSION}/libvlccore.dll" -d "$PUBLISH_DIR"
unzip -q -o "$LIBVLC_ZIP" "vlc-${LIBVLC_VERSION}/plugins/*" -d "$PUBLISH_DIR"

# Clean up nested directory if created by unzip
if [ -d "$PUBLISH_DIR/vlc-${LIBVLC_VERSION}" ]; then
    cp -r "$PUBLISH_DIR/vlc-${LIBVLC_VERSION}/." "$PUBLISH_DIR/"
    rm -rf "$PUBLISH_DIR/vlc-${LIBVLC_VERSION}"
fi

rm -f "$LIBVLC_ZIP"

# ------------------------------------------------------------
# Create Windows First-Run Shortcut Script (.vbs)
# ------------------------------------------------------------

echo "==> Creating Windows shortcut installer script..."

cp -f "$ICON_FILE" "$PUBLISH_DIR/quran.ico"

cat > "$OUTPUT_DIR/Create-Shortcut.vbs" <<'EOF'
Set WshShell = CreateObject("WScript.Shell")
strDesktop = WshShell.SpecialFolders("Desktop")
strCurrentDir = WshShell.CurrentDirectory

Set oShellLink = WshShell.CreateShortcut(strDesktop & "\Quran.lnk")
oShellLink.TargetPath = strCurrentDir & "\publish\Quran.exe"
oShellLink.WorkingDirectory = strCurrentDir & "\publish"
oShellLink.IconLocation = strCurrentDir & "\quran.ico, 0"
oShellLink.Description = "Launch Quran Application"
oShellLink.Save

MsgBox "Shortcut successfully created on your Desktop!", 64, "Quran Setup"
EOF

# ------------------------------------------------------------
# Zip All Contents in Output Directory (with progress)
# ------------------------------------------------------------

echo "==> Packaging all contents of $OUTPUT_DIR into $ZIP_FILE..."

# Create the ZIP file directly outside of OUTPUT_DIR to avoid I/O collision
FINAL_ZIP_NAME="${APP_NAME}-windows-x64.zip"
(cd "$OUTPUT_DIR" && zip -r "../$FINAL_ZIP_NAME" .)

# Move the completed ZIP archive into OUTPUT_DIR location
TEMP_ZIP="dist/$FINAL_ZIP_NAME"

# ------------------------------------------------------------
# Cleanup Everything Except the ZIP
# ------------------------------------------------------------

echo "==> Cleaning up build files and folders..."

rm -rf "${OUTPUT_DIR:?}"/*
mv "$TEMP_ZIP" "$ZIP_FILE"

# ------------------------------------------------------------
# Summary
# ------------------------------------------------------------

echo
echo "============================================================"
echo " Build completed successfully!"
echo "============================================================"
echo
echo "Output Archive:"
echo "  $ZIP_FILE"
echo
ls -lh "$ZIP_FILE"
echo