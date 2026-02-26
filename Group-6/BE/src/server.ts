import http from 'http';
import app from './app';
import config from './config/config';
import { ws } from './websocket';
import { initWorker } from './services/workers/message-worker';
import { loadPendingMessages } from './repositories/messages.repository';

const server = http.createServer(app);

server.on('upgrade', (request, socket, head) => {
  ws.handleUpgrade(request, socket, head, (client) => {
    ws.emit('connection', client, request);
  });
});

server.listen(config.port, () => {
  console.log(`Server running on port ${config.port}`);
  initWorker();
});

loadPendingMessages()
