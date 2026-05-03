#!/usr/bin/env bash
set -euo pipefail

MOD_NAME="VieBeautifulFont"
REPO="sant1ago-da-hanoi/oni-vie-beautiful-font"
ASSET_NAME="VieBeautifulFont.zip"

# --- Detect OS and set mod path ---
detect_mod_dir() {
  case "$(uname -s)" in
    Darwin)
      echo "$HOME/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local"
      ;;
    Linux)
      # Standard Steam on Linux
      local steam_dir="$HOME/.config/unity3d/Klei/Oxygen Not Included/mods/Local"
      if [ -d "$steam_dir" ] || [ -d "$(dirname "$(dirname "$steam_dir")")" ]; then
        echo "$steam_dir"
      else
        # Flatpak Steam
        echo "$HOME/.var/app/com.valvesoftware.Steam/.config/unity3d/Klei/Oxygen Not Included/mods/Local"
      fi
      ;;
    MINGW*|MSYS*|CYGWIN*)
      echo "$USERPROFILE/Documents/Klei/OxygenNotIncluded/mods/Local"
      ;;
    *)
      echo ""
      ;;
  esac
}

# --- Get latest release download URL ---
get_download_url() {
  local url
  if command -v curl &>/dev/null; then
    url=$(curl -sL "https://api.github.com/repos/$REPO/releases/latest" \
      | grep -o "\"browser_download_url\": *\"[^\"]*$ASSET_NAME\"" \
      | head -1 \
      | cut -d'"' -f4)
  elif command -v wget &>/dev/null; then
    url=$(wget -qO- "https://api.github.com/repos/$REPO/releases/latest" \
      | grep -o "\"browser_download_url\": *\"[^\"]*$ASSET_NAME\"" \
      | head -1 \
      | cut -d'"' -f4)
  else
    echo "Error: curl or wget required" >&2
    exit 1
  fi
  echo "$url"
}

# --- Download file ---
download() {
  local url="$1" dest="$2"
  if command -v curl &>/dev/null; then
    curl -sL -o "$dest" "$url"
  else
    wget -qO "$dest" "$url"
  fi
}

# --- Main ---
echo "=== ONI Vietnamese Beautiful Font Installer ==="
echo ""

# Detect mod directory
MOD_DIR="$(detect_mod_dir)"
if [ -z "$MOD_DIR" ]; then
  echo "Error: unsupported OS ($(uname -s))"
  exit 1
fi

TARGET="$MOD_DIR/$MOD_NAME"
echo "Mod directory: $TARGET"

# Check for existing install
if [ -d "$TARGET" ]; then
  echo "Existing install found. Updating..."
  rm -rf "$TARGET"
fi

# Get download URL
echo "Fetching latest release..."
DOWNLOAD_URL="$(get_download_url)"
if [ -z "$DOWNLOAD_URL" ]; then
  echo "Error: could not find $ASSET_NAME in latest release"
  exit 1
fi

# Download
TMPDIR_DL="$(mktemp -d)"
trap 'rm -rf "$TMPDIR_DL"' EXIT
ZIP_PATH="$TMPDIR_DL/$ASSET_NAME"

echo "Downloading $ASSET_NAME..."
download "$DOWNLOAD_URL" "$ZIP_PATH"

# Extract
echo "Installing to $TARGET..."
mkdir -p "$TARGET"
unzip -qo "$ZIP_PATH" -d "$TARGET"

echo ""
echo "Done! Installed to:"
echo "  $TARGET"
echo ""
echo "Next steps:"
echo "  1. Open ONI"
echo "  2. Go to Mods -> enable $MOD_NAME"
echo "  3. Restart game"
echo "  4. Settings -> Language -> Tieng Viet"
