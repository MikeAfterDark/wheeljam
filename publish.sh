#!/bin/bash

echo "Uploading new game update with Butler..."

# Ensure butler is installed
command -v butler >/dev/null 2>&1 || {
    echo >&2 "itch's [butler] program is required but it's not in PATH. See: https://itch.io/docs/butler/installing.html. Aborting."
    exit 1
}

# Ensure jq is installed
command -v jq >/dev/null 2>&1 || {
    echo >&2 "Error: 'jq' is required but not installed. Install it first: https://stedolan.github.io/jq/download/"
    exit 1
}

# Ensure a version argument is provided
if [ -z "$1" ]; then
    echo "Error: No version provided! Usage: ./publish.sh <version>"
    exit 1
fi

version="$1"

CONFIG_FILE="publish.json"
ZIP_FILE_NAME="game.zip"

# Ensure the config file exists
if [ ! -f "$CONFIG_FILE" ]; then
    echo "Error: Configuration file '$CONFIG_FILE' not found!"
    exit 1
fi

# Read values from publish.json
buildDir=$(jq -r '.buildDir' "$CONFIG_FILE")
user=$(jq -r '.user' "$CONFIG_FILE")
projectName=$(jq -r '.projectName' "$CONFIG_FILE")
channel=$(jq -r '.channel' "$CONFIG_FILE")

# Ensure no empty values
if [[ -z "$buildDir" || -z "$user" || -z "$projectName" || -z "$channel" ]]; then
    echo "Error: Missing required fields in '$CONFIG_FILE'!"
    exit 1
fi

# Define paths
versionDir="$buildDir/$version"
zipFile="$versionDir/$ZIP_FILE_NAME"

# Ensure the version directory exists
if [ ! -d "$versionDir" ]; then
    echo "Error: Version directory '$versionDir' not found!"
    exit 1
fi

# Remove existing zip file if it exists
if [ -f "$zipFile" ]; then
    echo "Removing old zip file: $zipFile"
    rm "$zipFile"
fi

# Create a new zip archive
echo "Creating zip archive for '$versionDir'..."
(cd "$versionDir" && zip -r "$ZIP_FILE_NAME" .)

echo "Found Butler, pushing..."
butler push "$zipFile" "$user/$projectName:$channel"

echo "Upload complete!"
