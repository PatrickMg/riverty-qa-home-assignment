#!/usr/bin/env bash
set -euo pipefail

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker is required to run this script." >&2
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RESULTS_DIR="$ROOT_DIR/TestResults"

mkdir -p "$RESULTS_DIR"

IMAGE="mcr.microsoft.com/dotnet/sdk:8.0"
echo "Pulling $IMAGE ..."
docker pull "$IMAGE" >/dev/null

docker run --rm \
  -v "$ROOT_DIR":/workspace \
  -w /workspace \
  "$IMAGE" \
  bash -c "dotnet test CardValidation.sln -c Release --collect:\"XPlat Code Coverage\" --results-directory /workspace/TestResults --logger:\"trx;LogFileName=test-results.trx\""

echo "Test artifacts are available under $RESULTS_DIR"
