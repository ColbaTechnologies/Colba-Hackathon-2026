import { runBackgrounProcess } from './infrastructure/background-runner';
import { processMessages } from './messages/process-messages';
import { connectToDb } from './infrastructure/drizzle';
import { env } from './infrastructure/env';
import { buildApi, runApi } from './infrastructure/hono';
import { buildMessageRepository } from './messages/infrastructure/repository';
import { runRegistraionProcess } from './register';

const appId = crypto.randomUUID();
const db = connectToDb(env.DATABASE_URL);
const repo = buildMessageRepository(appId, db);

const api = buildApi(appId, repo);
runBackgrounProcess('message-processor', processMessages(repo));

runRegistraionProcess({
  appId,
  masterUrl: env.MASTER_URL,
  registrationEndpoint: env.REGISTRATION_ENDPOINT,
}, repo);

runApi(api);