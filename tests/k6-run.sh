#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

readonly TEST_TYPES=(
  string-allocation
  leak-static
  gen2-promotion
  loh-pressure
  thread-pool-starvation
  thread-leak
  lock-contention
  fibonacci
)

usage() {
  echo "Usage: $0 [test-type]"
  echo
  echo "Available test types:"
  printf '  - %s\n' "${TEST_TYPES[@]}"
  echo
  echo "Run without arguments for an interactive menu."
}

is_valid_type() {
  local candidate="$1"
  local test_type
  for test_type in "${TEST_TYPES[@]}"; do
    if [[ "$test_type" == "$candidate" ]]; then
      return 0
    fi
  done
  return 1
}

if [[ "${1:-}" == "-h" || "${1:-}" == "--help" ]]; then
  usage
  exit 0
fi

test_type="${1:-}"

if [[ -z "$test_type" ]]; then
  echo "Escolha o teste:"
  PS3="#? "
  select selected_type in "${TEST_TYPES[@]}"; do
    if [[ -n "$selected_type" ]]; then
      test_type="$selected_type"
      break
    fi
    echo "Opção inválida."
  done
elif ! is_valid_type "$test_type"; then
  echo "Erro: tipo de teste desconhecido '$test_type'" >&2
  echo >&2
  usage >&2
  exit 1
fi

if [[ -f ./k6.env ]]; then
  set -a
  source ./k6.env
  set +a
fi

echo "Start: $(date)"

k6 run \
  -e TEST_TYPE="$test_type" \
  test-endpoints.js

echo "End: $(date)"