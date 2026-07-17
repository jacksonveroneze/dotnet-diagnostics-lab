#!/usr/bin/env bash
set -euo pipefail

echo "Start: $(date)"

type="${1:-grpc}"

set -a
source ./k6.env
set +a

case "$type" in
  rest)
    k6 run --summary-export results-rest.json test-rest.js
    ;;
  grpc)
    k6 run --summary-export results-grpc.json test-grpc.js
    ;;
  *)
    echo "Usage: ./k6-run.sh [rest|grpc]"
    exit 1
    ;;
esac

echo "End: $(date)"