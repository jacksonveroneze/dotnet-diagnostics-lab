# .NET Diagnostics Lab

API de laboratório para simular cenários de diagnóstico de performance e memória em .NET
(alta alocação, promoção para Gen2, pressão na LOH, memory leak estático, event handler leak,
cache sem expiração, closure capturando referências, CancellationTokenSource não disposto,
Timer nunca parado, thread pool starvation, thread leak, lock contention e CPU-bound),
permitindo observar o comportamento via ferramentas de diagnóstico (dotnet-counters,
dotnet-trace, dotnet-gcdump, profilers, etc.).

## Executando

```bash
dotnet run --project src/main/Api
```

Por padrão a API sobe em `http://0.0.0.0:7000` (ver `src/main/Api/Properties/launchSettings.json`).

Em ambiente de Development:

- Documentação OpenAPI: `GET /openapi/v1.json`
- UI interativa (Scalar): `GET /scalar/v1`

## Endpoints auxiliares

| Método | Rota       | Descrição                                             |
|--------|------------|--------------------------------------------------------|
| GET    | `/health`  | Health check da aplicação e dependências.              |
| GET    | `/metrics` | Endpoint de scraping do Prometheus (OpenTelemetry).     |

## Convenções

- Todas as rotas de diagnóstico seguem o padrão `diagnostics/v{version}/{resource}/{action}`.
- Versionamento via segmento de URL (`v1`).
- Parâmetros são passados via query string e validados nos serviços; valores fora do
  intervalo permitido retornam `400 Bad Request` no formato `ProblemDetails`.
- Todas as respostas de sucesso retornam um `SimulationResult`:

```json
{
  "durationMs": 0,
  "allocatedBytes": 0,
  "gcCountBefore": { "gen0": 0, "gen1": 0, "gen2": 0 },
  "gcCountAfter": { "gen0": 0, "gen1": 0, "gen2": 0 }
}
```

### Respostas possíveis

| Status | Quando ocorre                                              |
|--------|--------------------------------------------------------------|
| 200    | Simulação executada com sucesso.                              |
| 400    | Parâmetro fora do intervalo permitido (`ArgumentException`). |
| 500    | Erro inesperado durante a execução da simulação.              |

---

## Memory (`diagnostics/v1/memory`)

Simulações relacionadas a alocação de memória e comportamento do Garbage Collector.

### `GET /diagnostics/v1/memory/string-allocation`

Simula alta alocação de memória no heap através de concatenação de strings (gera muito
lixo em Gen0/Gen1).

| Parâmetro     | Tipo | Obrigatório | Min | Max     | Descrição                                  |
|---------------|------|-------------|-----|---------|---------------------------------------------|
| `iterations`  | int  | Sim         | 1   | 100000  | Quantidade de iterações de concatenação.     |
| `stringLength`| int  | Sim         | 1   | 99000   | Tamanho (em caracteres) de cada bloco.       |

Exemplo:

```
GET /diagnostics/v1/memory/string-allocation?iterations=1000&stringLength=500
```

### `GET /diagnostics/v1/memory/leak-static`

Simula um memory leak real: os objetos alocados são mantidos em uma lista `static`
compartilhada entre requisições (nunca são liberados pelo GC).

| Parâmetro         | Tipo | Obrigatório | Min | Max       | Descrição                              |
|-------------------|------|-------------|-----|-----------|------------------------------------------|
| `objectCount`     | int  | Sim         | 1   | 10000     | Quantidade de objetos (`byte[]`) criados. |
| `objectSizeBytes` | int  | Sim         | 1   | 1048576   | Tamanho de cada objeto em bytes.          |

Exemplo:

```
GET /diagnostics/v1/memory/leak-static?objectCount=100&objectSizeBytes=1024
```

### `GET /diagnostics/v1/memory/gen2-promotion`

Simula objetos que sobrevivem a coletas suficientes para serem promovidos até a Gen2.

| Parâmetro         | Tipo | Obrigatório | Min | Max       | Descrição                              |
|-------------------|------|-------------|-----|-----------|------------------------------------------|
| `objectCount`     | int  | Sim         | 1   | 10000     | Quantidade de objetos (`byte[]`) criados. |
| `objectSizeBytes` | int  | Sim         | 1   | 1048576   | Tamanho de cada objeto em bytes.          |

Exemplo:

```
GET /diagnostics/v1/memory/gen2-promotion?objectCount=500&objectSizeBytes=4096
```

### `GET /diagnostics/v1/memory/loh-pressure`

Simula pressão na Large Object Heap (LOH) alocando objetos grandes (a partir de ~85KB),
retendo parte deles.

| Parâmetro         | Tipo | Obrigatório | Min   | Max       | Descrição                                     |
|-------------------|------|-------------|-------|-----------|--------------------------------------------------|
| `objectCount`     | int  | Sim         | 1     | 2000      | Quantidade de objetos (`byte[]`) criados.        |
| `objectSizeBytes` | int  | Sim         | 85000 | 5242880   | Tamanho base de cada objeto em bytes.             |

Exemplo:

```
GET /diagnostics/v1/memory/loh-pressure?objectCount=50&objectSizeBytes=100000
```

### `GET /diagnostics/v1/memory/leak-event`

Simula um memory leak por event handler: cria subscribers que se inscrevem em um evento de
um publisher `static` e nunca se desinscrevem, mantendo os subscribers (e seus payloads) vivos
indefinidamente.

| Parâmetro          | Tipo | Obrigatório | Min | Max     | Descrição                                  |
|---------------------|------|-------------|-----|---------|-----------------------------------------------|
| `subscriberCount`   | int  | Sim         | 1   | 10000   | Quantidade de subscribers criados e inscritos. |
| `payloadSizeBytes`  | int  | Sim         | 1   | 1048576 | Tamanho do payload (`byte[]`) de cada subscriber. |

Exemplo:

```
GET /diagnostics/v1/memory/leak-event?subscriberCount=100&payloadSizeBytes=50000
```

### `GET /diagnostics/v1/memory/leak-cache`

Simula um cache sem expiração: cada chamada insere novos objetos com chave sempre única no
`HybridCache` já registrado na aplicação, com expiração configurada para uma duração
efetivamente muito longa (o `HybridCache` não suporta expiração infinita de verdade), fazendo
o cache crescer indefinidamente.

| Parâmetro         | Tipo | Obrigatório | Min | Max       | Descrição                              |
|-------------------|------|-------------|-----|-----------|------------------------------------------|
| `objectCount`     | int  | Sim         | 1   | 10000     | Quantidade de objetos inseridos no cache. |
| `objectSizeBytes` | int  | Sim         | 1   | 1048576   | Tamanho de cada objeto em bytes.          |

Exemplo:

```
GET /diagnostics/v1/memory/leak-cache?objectCount=100&objectSizeBytes=10000
```

### `GET /diagnostics/v1/memory/leak-closure`

Simula uma closure que captura referências: cria processadores com um `Timer` cujo handler de
`Elapsed` captura um campo grande da instância; o timer nunca é parado e a instância é mantida
viva em uma lista `static` compartilhada entre requisições.

| Parâmetro         | Tipo | Obrigatório | Min | Max       | Descrição                              |
|-------------------|------|-------------|-----|-----------|------------------------------------------|
| `objectCount`     | int  | Sim         | 1   | 10000     | Quantidade de processadores criados.      |
| `objectSizeBytes` | int  | Sim         | 1   | 1048576   | Tamanho do campo capturado pela closure (bytes). |

Exemplo:

```
GET /diagnostics/v1/memory/leak-closure?objectCount=50&objectSizeBytes=100000
```

### `GET /diagnostics/v1/memory/leak-cancellation-token-source`

Simula `CancellationTokenSource` não disposto: para cada task, cria um `CancellationTokenSource`
e um linked token, nunca chamando `Dispose()` em nenhum dos dois.

| Parâmetro   | Tipo | Obrigatório | Min | Max   | Descrição                                |
|-------------|------|-------------|-----|-------|---------------------------------------------|
| `delayMs`   | int  | Sim         | 1   | 60000 | Tempo de delay (ms) de cada task.           |
| `taskCount` | int  | Sim         | 1   | 10000 | Quantidade de tasks/CancellationTokenSource criados. |

Exemplo:

```
GET /diagnostics/v1/memory/leak-cancellation-token-source?delayMs=10000&taskCount=2
```

### `GET /diagnostics/v1/memory/leak-timer`

Simula um `Timer` nunca parado: cria instâncias de `System.Timers.Timer` já iniciadas e
mantidas em uma lista `static` compartilhada entre requisições, sem nunca chamar `Stop()`/`Dispose()`.

| Parâmetro     | Tipo | Obrigatório | Min | Max      | Descrição                          |
|---------------|------|-------------|-----|----------|---------------------------------------|
| `timerCount`  | int  | Sim         | 1   | 10000    | Quantidade de timers criados.         |
| `intervalMs`  | int  | Sim         | 1   | 3600000  | Intervalo (ms) de disparo de cada timer. |

Exemplo:

```
GET /diagnostics/v1/memory/leak-timer?timerCount=100&intervalMs=30000
```

---

## Thread (`diagnostics/v1/thread`)

Simulações relacionadas a threads, thread pool e sincronização.

### `GET /diagnostics/v1/thread/thread-pool-starvation`

Simula starvation do thread pool ao bloquear threads do pool com `Task.Delay(...).GetAwaiter().GetResult()`
de forma síncrona dentro de `Task.Run`.

| Parâmetro   | Tipo | Obrigatório | Min | Max   | Descrição                                |
|-------------|------|-------------|-----|-------|---------------------------------------------|
| `delayMs`   | int  | Sim         | 100 | 10000 | Tempo de bloqueio (ms) de cada task.        |
| `taskCount` | int  | Sim         | 1   | 10    | Quantidade de tasks disparadas em paralelo. |

Exemplo:

```
GET /diagnostics/v1/thread/thread-pool-starvation?delayMs=1000&taskCount=5
```

### `GET /diagnostics/v1/thread/thread-leak`

Simula vazamento de threads: cria threads dedicadas (fora do thread pool) que permanecem
bloqueadas (`Thread.Sleep`) por um período, com uma pilha de recursão profunda para
aumentar o uso de stack.

| Parâmetro   | Tipo | Obrigatório | Min | Max   | Descrição                                  |
|-------------|------|-------------|-----|-------|-----------------------------------------------|
| `delayMs`   | int  | Sim         | 100 | 50000 | Tempo (ms) que cada thread fica bloqueada.     |
| `taskCount` | int  | Sim         | 1   | 99    | Quantidade de threads criadas.                 |

Exemplo:

```
GET /diagnostics/v1/thread/thread-leak?delayMs=5000&taskCount=10
```

### `GET /diagnostics/v1/thread/lock-contention`

Simula contenção de lock: várias tasks disputam um `lock` compartilhado, cada uma
segurando o lock durante `delayMs`.

| Parâmetro   | Tipo | Obrigatório | Min | Max   | Descrição                                  |
|-------------|------|-------------|-----|-------|-----------------------------------------------|
| `delayMs`   | int  | Sim         | 100 | 10000 | Tempo (ms) que cada task segura o lock.        |
| `taskCount` | int  | Sim         | 1   | 10    | Quantidade de tasks disputando o lock.         |

Exemplo:

```
GET /diagnostics/v1/thread/lock-contention?delayMs=500&taskCount=8
```

---

## CPU (`diagnostics/v1/cpu`)

Simulações relacionadas a alto consumo de CPU.

### `GET /diagnostics/v1/cpu/fibonacci`

Simula uso intenso de CPU calculando Fibonacci de forma recursiva (não otimizada, sem
memoização).

| Parâmetro          | Tipo | Obrigatório | Min | Max | Descrição                                |
|--------------------|------|-------------|-----|-----|---------------------------------------------|
| `sequencePosition` | int  | Sim         | 1   | 40  | Posição da sequência de Fibonacci a calcular. |

Exemplo:

```
GET /diagnostics/v1/cpu/fibonacci?sequencePosition=35
```

### `GET /diagnostics/v1/cpu/regex-backtracking`

Simula um ReDoS (Regular Expression Denial of Service): usa uma regex vulnerável a
backtracking catastrófico (`^(a+)+$`, grupo aninhado com quantificador) contra um
input que quase casa, forçando o motor de regex a explorar um número exponencial de
combinações antes de falhar.

| Parâmetro     | Tipo | Obrigatório | Min | Max | Descrição                                             |
|---------------|------|-------------|-----|-----|--------------------------------------------------------|
| `inputLength` | int  | Sim         | 1   | 30  | Quantidade de caracteres `'a'` no input (custo ~O(2^n)). |

Valores próximos do máximo demoram desproporcionalmente mais (crescimento
exponencial) — isso é o comportamento esperado do cenário, não um bug do
endpoint. Comece com valores baixos (ex.: 15-20) antes de subir.

Exemplo:

```
GET /diagnostics/v1/cpu/regex-backtracking?inputLength=25
```

---

## Exception (`diagnostics/v1/exception`)

Simulações relacionadas ao tratamento global de exceções.

### `GET /diagnostics/v1/exception/throw`

Lança deliberadamente uma exceção, permitindo observar o pipeline de tratamento
de erro (`GlobalExceptionHandler`, resposta em `ProblemDetails`, logs/traces de
exceção).

| Parâmetro | Tipo | Obrigatório | Valores               | Descrição                                             |
|-----------|------|-------------|------------------------|--------------------------------------------------------|
| `type`    | enum | Sim         | `Argument`, `Unhandled` | Tipo de exceção simulada (ver tabela de respostas abaixo). |

| `type`      | Exceção lançada             | Status HTTP |
|-------------|------------------------------|-------------|
| `Argument`  | `ArgumentException`          | 400         |
| `Unhandled` | `InvalidOperationException`  | 500         |

Um valor inválido para `type` retorna `500 Internal Server Error`: a falha de
binding do parâmetro lança `BadHttpRequestException`, que cai no branch padrão
do `GlobalExceptionHandler` (mesmo comportamento de qualquer parâmetro
inválido nos demais endpoints, ex.: `sequencePosition=abc`).

Exemplo:

```
GET /diagnostics/v1/exception/throw?type=Argument
GET /diagnostics/v1/exception/throw?type=Unhandled
```