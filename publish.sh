#!/bin/bash

# Build script for AddPdfEnvelope - creates self-contained single-file executables
# for Windows, macOS, and Linux (both x64 and ARM64)

set -e

PUBLISH_DIR="publish"
PROJECT_NAME="AddPdfEnvelope"
PROJECT_FILE="${PROJECT_NAME}.csproj"

# Runtime identifiers for all target platforms
RIDS=(
    "win-x64"
    "win-arm64"
    "osx-x64"
    "osx-arm64"
    "linux-x64"
    "linux-arm64"
)

# Clean up previous publish folder
echo "Cleaning up previous publish folder..."
rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"

# Build and publish for each platform
for RID in "${RIDS[@]}"; do
    echo ""
    echo "=========================================="
    echo "Building for $RID..."
    echo "=========================================="
    
    OUTPUT_DIR="$PUBLISH_DIR/$RID"
    
    dotnet publish "$PROJECT_FILE" -c Release \
        -r "$RID" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -o "$OUTPUT_DIR"
    
    # Remove unnecessary files (keep only executables and required files)
    echo "Cleaning up $RID output..."
    
    # Determine executable name based on platform
    if [[ "$RID" == win-* ]]; then
        EXE_NAME="$PROJECT_NAME.exe"
    else
        EXE_NAME="$PROJECT_NAME"
    fi
    
    # Create ZIP file
    echo "Creating ZIP for $RID..."
    ZIP_NAME="${PROJECT_NAME}-${RID}.zip"
    
    pushd "$OUTPUT_DIR" > /dev/null
    
    # Remove PDB files and other unnecessary files
    rm -f *.pdb
    rm -f *.deps.json
    rm -f *.runtimeconfig.json
    
    # Create ZIP containing all remaining files
    if command -v zip &> /dev/null; then
        zip -r "../$ZIP_NAME" .
    else
        # Fallback to tar if zip is not available
        tar -czvf "../${PROJECT_NAME}-${RID}.tar.gz" .
        echo "Note: 'zip' command not found, created .tar.gz instead"
    fi
    
    popd > /dev/null
    
    echo "Created: $PUBLISH_DIR/$ZIP_NAME"
done

echo ""
echo "=========================================="
echo "Build complete! Output in $PUBLISH_DIR/"
echo "=========================================="
ls -la "$PUBLISH_DIR"/*.zip 2>/dev/null || ls -la "$PUBLISH_DIR"/*.tar.gz 2>/dev/null
