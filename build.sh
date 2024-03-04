#!/bin/bash

dotnet restore

# empty build folder

if [ -d "build" ]; then
  rm -rf build
fi

echo ""
echo "Now building for all supported platforms"
echo ""

# build the project

dotnet publish OTD.Variant.Manager.UX.Desktop -r win-x64 -c Release -o build/win-x64
dotnet publish OTD.Variant.Manager.UX.Desktop -r win-x86 -c Release -o build/win-x86
dotnet publish OTD.Variant.Manager.UX.Desktop -r win-arm64 -c Release -o build/win-arm64
dotnet publish OTD.Variant.Manager.UX.Desktop -r linux-x64 -c Release -o build/linux-x64
dotnet publish OTD.Variant.Manager.UX.Desktop -r linux-arm64 -c Release -o build/linux-arm64
dotnet publish OTD.Variant.Manager.UX.Desktop -r osx-x64 -c Release -o build/osx-x64
dotnet publish OTD.Variant.Manager.UX.Desktop -r osx-arm64 -c Release -o build/osx-arm64

# Remove all OpenTabletDriver*.pdb files inside each build folder
# Also rename the executable from "OTD.Variant.Manager.UX.Desktop" to just "OTD.Variant.Manager"

echo ""
echo "Done building, now cleaning up"
echo ""

output="OTD.Variant.Manager"
toCheck="OTD.Variant.Manager.UX.Desktop"

(
    cd build
    find . -name "OpenTabletDriver*.pdb" -type f -delete

    for f in *; do
        # Check to make sure it's a directory
        if [ -d "$f" ]; then
            # Windows
            if [ -f "$f/$toCheck.exe" ]; then
                mv "$f/$toCheck.exe" "$f/$output.exe"
            fi

            # Unix based
            if [ -f "$f/$toCheck" ]; then
                mv "$f/$toCheck" "$f/$output"
            fi
        fi

    done
)

# Create a zip file for each build folder

echo ""
echo "Done cleaning up, now creating zip files"
echo ""

(
    cd build
    for f in *; do
        if [ -d "$f" ]; then
            zip -r "$output-$f.zip" "$f"
        fi
    done
)