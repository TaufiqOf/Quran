#!/usr/bin/env bash

set -euo pipefail

# ============================================================
# PNG -> Windows ICO generator
#
# Usage:
#   ./create-icon.sh input.png
#
# Example:
#   ./create-icon.sh Assets/Icons/quran.png
#
# Output:
#   Assets/Icons/quran.ico
#
# ICO contains:
#   16x16
#   32x32
#   48x48
#   64x64
#   128x128
#   256x256
# ============================================================

SIZES=(16 32 48 64 128 256)

if [ "$#" -ne 1 ]; then
    echo "Usage: $0 <input.png>"
    echo
    echo "Example:"
    echo "  $0 Assets/Icons/quran.png"
    exit 1
fi

INPUT="$1"

if [ ! -f "$INPUT" ]; then
    echo "ERROR: Input file not found:"
    echo "  $INPUT"
    exit 1
fi

# ------------------------------------------------------------
# Check ImageMagick
# ------------------------------------------------------------

if command -v magick >/dev/null 2>&1; then
    MAGICK="magick"
elif command -v convert >/dev/null 2>&1; then
    MAGICK="convert"
else
    echo "ERROR: ImageMagick is not installed."
    echo
    echo "Install it with:"
    echo "  sudo apt install imagemagick"
    exit 1
fi

# ------------------------------------------------------------
# Check icotool
# ------------------------------------------------------------

if ! command -v icotool >/dev/null 2>&1; then
    echo "ERROR: icotool is not installed."
    echo
    echo "Install it with:"
    echo "  sudo apt install icoutils"
    exit 1
fi

# ------------------------------------------------------------
# Output locations
# ------------------------------------------------------------

INPUT_DIR="$(dirname "$INPUT")"
INPUT_NAME="$(basename "$INPUT")"
BASE_NAME="${INPUT_NAME%.*}"

OUTPUT_DIR="$INPUT_DIR/${BASE_NAME}-icons"
ICO_FILE="$INPUT_DIR/${BASE_NAME}.ico"

mkdir -p "$OUTPUT_DIR"

echo
echo "============================================================"
echo " Creating application icon"
echo "============================================================"
echo
echo "Input:"
echo "  $INPUT"
echo
echo "Output:"
echo "  $ICO_FILE"
echo

# ------------------------------------------------------------
# Generate PNG sizes
# ------------------------------------------------------------

PNG_FILES=()

for SIZE in "${SIZES[@]}"; do

    OUTPUT="$OUTPUT_DIR/${BASE_NAME}-${SIZE}x${SIZE}.png"

    echo "==> Generating ${SIZE}x${SIZE}"

    "$MAGICK" "$INPUT" \
        -background none \
        -alpha on \
        -resize "${SIZE}x${SIZE}" \
        -gravity center \
        -extent "${SIZE}x${SIZE}" \
        -strip \
        "$OUTPUT"

    PNG_FILES+=("$OUTPUT")
done

# ------------------------------------------------------------
# Remove old ICO
# ------------------------------------------------------------

if [ -f "$ICO_FILE" ]; then
    echo
    echo "==> Removing old ICO..."
    rm -f "$ICO_FILE"
fi

# ------------------------------------------------------------
# Create ICO using icotool
# ------------------------------------------------------------

echo
echo "==> Creating ICO with icotool..."

icotool \
    --create \
    --output "$ICO_FILE" \
    "${PNG_FILES[@]}"

# ------------------------------------------------------------
# Verify ICO
# ------------------------------------------------------------

echo
echo "==> Verifying ICO..."

if ! icotool --list "$ICO_FILE"; then
    echo
    echo "ERROR: ICO verification failed."
    exit 1
fi

# ------------------------------------------------------------
# Show result
# ------------------------------------------------------------

echo
echo "============================================================"
echo " Icon created successfully!"
echo "============================================================"
echo

echo "ICO:"
echo "  $ICO_FILE"

echo
echo "PNG sizes:"

for SIZE in "${SIZES[@]}"; do
    echo "  ${OUTPUT_DIR}/${BASE_NAME}-${SIZE}x${SIZE}.png"
done

echo
echo "File information:"
ls -lh "$ICO_FILE"

echo
echo "ICO information:"
identify "$ICO_FILE"

echo
echo "============================================================"
echo " Done!"
echo "============================================================"
