#!/bin/bash

# Enhanced Pre-Roll Videos Plugin Build Script

VERSION=$1

if [ -z "$VERSION" ]; then
    echo "Usage: ./build.sh <version>"
    echo "Example: ./build.sh 1.0.0"
    exit 1
fi

echo "Building Pre-Roll Videos Enhanced Plugin v${VERSION}..."

# Clean previous builds
rm -rf bin/ obj/ release/

# Build the plugin
dotnet publish Jellyfin.Plugin.PreRollEnhanced.csproj \
    --configuration Release \
    --output bin

# Create release directory
mkdir -p release

# Create zip package
cd bin
zip -r ../release/preroll-enhanced_${VERSION}.zip *
cd ..

echo "Build complete! Package created at: release/preroll-enhanced_${VERSION}.zip"
echo ""
echo "To install:"
echo "1. Upload the zip file to your server"
echo "2. Extract to: /var/lib/jellyfin/plugins/PreRollEnhanced/"
echo "3. Restart Jellyfin"
echo ""
echo "Or add this repository URL to Jellyfin:"
echo "https://raw.githubusercontent.com/YOUR_USERNAME/jellyfin-preroll-enhanced/main/manifest.json"
