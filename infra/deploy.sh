#!/bin/bash
set -euo pipefail

if [ $# -lt 2 ]; then
  echo "Usage: $0 <resource-group> <location>"
  exit 1
fi

GROUP="$1"
LOCATION="$2"

az group create --name "$GROUP" --location "$LOCATION"
az deployment group create \
  --resource-group "$GROUP" \
  --template-file "$(dirname "$0")/main.bicep" \
  --parameters location="$LOCATION"
