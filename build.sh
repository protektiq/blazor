#!/bin/bash

# Build the Blazor WebAssembly app
echo "Building Blazor WebAssembly app..."
dotnet publish CustomerSupportSystem.Wasm/CustomerSupportSystem.Wasm.csproj -c Release -o ./dist

# Copy the published files to the output directory
echo "Copying files to output directory..."
cp -r ./dist/wwwroot/* ./out/

echo "Build completed successfully!"
