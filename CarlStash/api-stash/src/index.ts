import { runBackgrounProcess } from './infrastructure/background-runner';
import { processMessages } from './messages/process-messages';
import { connectToDb } from './infrastructure/drizzle';
import { env } from './infrastructure/env';
import { buildApi, runApi } from './infrastructure/hono';
import { messagesRepository } from './messages/infrastructure/repository';
import { runRegistraionProcess } from './registration';
import { tenantsRepository } from './auth/infrastructure/repository';

const appId = crypto.randomUUID();
const db = connectToDb(env.DATABASE_URL);
const messagesRepo = messagesRepository(appId, db);
const tenantsRepo = tenantsRepository(db);

const api = buildApi(appId, messagesRepo, tenantsRepo, db);
runBackgrounProcess('message-processor', processMessages(messagesRepo));

runRegistraionProcess({
  appId,
  masterUrl: env.MASTER_URL,
  registrationEndpoint: env.REGISTRATION_ENDPOINT,
}, messagesRepo);

runApi(api);