import { serve } from '@hono/node-server'
import { buildApi } from './api.js';

const api = buildApi();

new Promise(async () => {
  while (true) { 
    console.log('Running backgroun server...')
    await new Promise(resolve => setTimeout(resolve, 1000));
    throw new Error('Background server error')
  }
});

serve({
  fetch: api.fetch,
  port: 3000
}, (info) => {
  console.log(`Server is running on http://localhost:${info.port}`)
});
