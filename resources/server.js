const http = require('http');

const PORT = process.env.PORT || 3001;

const server = http.createServer((req, res) => {
    const url = new URL(req.url, `http://localhost:${PORT}`);
    const delayMs = Math.min(
        parseInt(url.searchParams.get('delayMs') ?? '1', 10),
        60_000 // teto de segurança
    );

    setTimeout(() => {
        res.writeHead(200, { 'Content-Type': 'application/json', 'Connection': 'close' });
        res.end(JSON.stringify({ delayMs, ts: Date.now() }));
    }, delayMs);
});

server.listen(PORT, () => {
    console.log(`delay-server on http://localhost:${PORT} (GET /?delayMs=N)`);
});