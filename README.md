# .NET Diagnostics Lab

API de laboratório para simular cenários de diagnóstico de performance e memória em .NET
(alta alocação, pressão na LOH, memory leak estático, event handler leak, cache sem expiração,
closure capturando referências, CancellationTokenSource não disposto, Timer nunca parado,
coletas de Gen2 bloqueantes, thread pool starvation, thread leak, lock contention, bloqueio de
thread do pool por I/O síncrono, CPU-bound e exaustão de sockets HTTP por `new HttpClient()`
por request), permitindo observar o comportamento via ferramentas de diagnóstico
(dotnet-counters, dotnet-trace, dotnet-gcdump, profilers, etc.).

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

Simula alta alocação de memória no heap gerando um CSV de pessoas aleatórias através de
concatenação de strings (gera muito lixo em Gen0/Gen1).

| Parâmetro    | Tipo | Obrigatório | Min | Max    | Descrição                              |
|--------------|------|-------------|-----|--------|------------------------------------------|
| `iterations` | int  | Sim         | 1   | 100000 | Quantidade de linhas (pessoas) do CSV.  |

Exemplo:

```
GET /diagnostics/v1/memory/string-allocation?iterations=1000
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

### `GET /diagnostics/v1/memory/blocking-gc`

Simula coletas de Gen2 bloqueantes e repetidas via promoção forçada de objetos sobreviventes:
mantém `survivorCount` objetos retidos durante toda a simulação e, a cada iteração, aloca lixo
de curta vida e força `GC.Collect(2, GCCollectionMode.Forced, blocking: true)`, gerando pausas
reais (stop-the-world) observáveis no dotTrace/dotnet-counters — o foco é o custo da coleta em
si, não apenas o crescimento de memória.

| Parâmetro       | Tipo | Obrigatório | Min | Max   | Descrição                                                  |
|-----------------|------|-------------|-----|-------|------------------------------------------------------------|
| `iterations`    | int  | Sim         | 1   | 1000  | Quantidade de coletas de Gen2 forçadas.                     |
| `survivorCount` | int  | Sim         | 1   | 10000 | Quantidade de objetos (~20KB) retidos durante a simulação.  |

Exemplo:

```
GET /diagnostics/v1/memory/blocking-gc?iterations=20&survivorCount=500
```

### `GET /diagnostics/v1/memory/gc-clean`

Endpoint utilitário (não é uma simulação): força coletas completas e bloqueantes (Gen2,
compacting) e aguarda os finalizers, para "limpar" o heap entre execuções de outros cenários e
obter medições mais previsíveis. Não recebe parâmetros e não retorna `SimulationResult`.

Exemplo:

```
GET /diagnostics/v1/memory/gc-clean
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

---

## Io (`diagnostics/v1/io`)

Simulações relacionadas a operações de I/O (rede, sockets, arquivos).

### `GET /diagnostics/v1/io/leak-http-client`

Simula o anti-pattern de instanciar `new HttpClient()` a cada chamada em vez de reutilizar
uma instância via `IHttpClientFactory` ou singleton: dispara `requestCount` chamadas HTTP em
paralelo, cada uma com um `HttpClient` novo, descartado ao final. Cada `HttpClient` descartado
deixa a conexão TCP subjacente em `TIME_WAIT` por ~4 minutos; sob carga sustentada isso esgota
as portas efêmeras do SO, produzindo `SocketException` e latência crescente — sintoma que no
dotTrace aparece como threads presas em abertura de conexão/DNS (I/O), não como CPU ou lock.
O endereço de destino é informado via `targetUrl` (obrigatório) — aponte para qualquer URL
http(s) alcançável, por exemplo o próprio `/health` da aplicação.

| Parâmetro     | Tipo   | Obrigatório | Min | Max  | Descrição                                             |
|---------------|--------|-------------|-----|------|----------------------------------------------------------|
| `requestCount`| int    | Sim         | 1   | 1000 | Quantidade de chamadas HTTP disparadas em paralelo.       |
| `targetUrl`   | string | Sim         | -   | -    | URL absoluta http(s) de destino.                          |

Exemplo:

```
GET /diagnostics/v1/io/leak-http-client?requestCount=50&targetUrl=http://localhost:7000/health
```

### `GET /diagnostics/v1/io/blocking-sync`

Simula o anti-pattern de bloquear uma worker thread do ThreadPool com I/O realmente síncrono
(`.GetAwaiter().GetResult()` sobre uma chamada HTTP de verdade) dentro de um pipeline que
deveria ser assíncrono ponta a ponta: dispara `taskCount` tasks via `Task.Run`, cada uma
bloqueando a thread até a resposta HTTP de `targetUrl` chegar. Diferente do
`thread-pool-starvation` (que bloqueia em `Task.Delay(...).GetAwaiter().GetResult()`, sem I/O
real), aqui a thread fica presa esperando uma resposta de rede genuína — sintoma que aparece em
profilers como threads do pool ocupadas em I/O em vez de CPU.

| Parâmetro   | Tipo   | Obrigatório | Min | Max   | Descrição                                                                  |
|-------------|--------|-------------|-----|-------|-----------------------------------------------------------------------------|
| `taskCount` | int    | Sim         | 1   | 10    | Quantidade de tasks bloqueantes disparadas.                                  |
| `delayMs`   | int    | Sim         | 1   | 10000 | Tempo (ms) que o endpoint de destino demora a responder (repassado como query string para `targetUrl`). |
| `targetUrl` | string | Sim         | -   | -     | URL absoluta http(s) de destino (ex.: `.../io/delay`).                       |

Exemplo:

```
GET /diagnostics/v1/io/blocking-sync?taskCount=5&delayMs=1000&targetUrl=http://localhost:7000/diagnostics/v1/io/delay
```

### `GET /diagnostics/v1/io/delay`

Endpoint utilitário (não é uma simulação): aguarda `delayMs` de forma assíncrona
(`Task.Delay`) e responde `200 OK`. Usado como alvo HTTP real pelo cenário `blocking-sync` (e
também serve como destino para `leak-http-client`) — não representa um anti-pattern por si só.

| Parâmetro | Tipo | Obrigatório | Min | Max | Descrição                                |
|-----------|------|-------------|-----|-----|-----------------------------------------------|
| `delayMs` | int  | Sim         | -   | -   | Tempo de espera (ms) antes de responder.       |

Exemplo:

```
GET /diagnostics/v1/io/delay?delayMs=500
```