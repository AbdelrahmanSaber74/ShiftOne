const http = require('http');
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, 'FrontEnd', 'build');
const types = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'application/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.svg': 'image/svg+xml',
  '.ico': 'image/x-icon',
  '.txt': 'text/plain; charset=utf-8',
  '.map': 'application/json; charset=utf-8'
};
function send(res, file) {
  fs.readFile(file, (err, data) => {
    if (err) { res.writeHead(500); res.end('Server error'); return; }
    res.writeHead(200, { 'Content-Type': types[path.extname(file)] || 'application/octet-stream', 'Cache-Control': 'no-cache' });
    res.end(data);
  });
}
http.createServer((req, res) => {
  const pathname = decodeURIComponent((req.url || '/').split('?')[0]);
  const relative = pathname === '/' ? 'index.html' : pathname.replace(/^\/+/, '');
  const requested = path.resolve(root, relative);
  if (!requested.startsWith(root)) return send(res, path.join(root, 'index.html'));
  fs.stat(requested, (err, stat) => {
    if (!err && stat.isFile()) return send(res, requested);
    return send(res, path.join(root, 'index.html'));
  });
}).listen(3000, '127.0.0.1', () => console.log('ShiftOne frontend on http://localhost:3000'));
