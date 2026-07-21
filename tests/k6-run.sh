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

test_type="${1:-}"
[[ "$test_type" == "-h" || "$test_type" == "--help" ]] && { usage; exit 0; }

if [[ -z "$test_type" ]]; then
  echo "Escolha o teste:"
  PS3="#? "
  select test_type in "${TEST_TYPES[@]}"; do
    [[ -n "$test_type" ]] && break
    echo "Opção inválida."
  done
elif ! printf '%s\n' "${TEST_TYPES[@]}" | grep -qx "$test_type"; then
  echo "Erro: tipo de teste desconhecido '$test_type'" >&2
  echo >&2
  usage >&2
  exit 1
fi

[[ -f ./k6.env ]] && { set -a; source ./k6.env; set +a; }

echo "Start: $(date)"

#k6 run -e TEST_TYPE="$test_type" test-endpoints.js
cd ../infra/docker && docker compose run --rm -e TEST_TYPE="$test_type" k6_stress_test

echo "End: $(date)"