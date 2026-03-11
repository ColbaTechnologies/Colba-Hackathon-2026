import { serve } from '@hono/node-server'
import { runBackgrounProcess } from './infrastructure/background-runner';
import { processMessages } from './messages/process-messages';
import { connectToDb } from './infrastructure/drizzle';
import { env } from './infrastructure/env';
import { buildApi } from './infrastructure/hono';
import { buildMessageRepository } from './messages/infrastructure/repository';

const appId = crypto.randomUUID();
const db = connectToDb(env.DATABASE_URL);
const repo = buildMessageRepository(appId, db);

const api = buildApi({ appId, repo });
runBackgrounProcess('message-processor', processMessages(repo));

serve({
  fetch: api.fetch,
  port: 3000
}, (info) => {
  console.log(`Server is running on http://localhost:${info.port}`)
});