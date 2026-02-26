import { WebSocketServer } from 'ws';
import app from './app';
import config from './config/config';

export const ws = new WebSocketServer({port: config.port});

app.listen(config.port, () => {
  console.log(`Server running on port ${config.port}`);
});