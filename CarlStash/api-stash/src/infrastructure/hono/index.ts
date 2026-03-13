import { Hono } from "hono";
import { serve } from '@hono/node-server'
import { mapAliveEndpoint } from "./alive-endpoint";
import { logger } from "hono/logger";
import { mapIngestMessagesEndpoint } from "../../messages/ingest-messages-endpoint";
import { type MessagesRepository } from "../../messages";
import { type UUID } from "crypto";

export const buildApi = (  
  appId: UUID,
  repo: MessagesRepository
) => {
  console.log(`Building API with ID: ${appId}`);

  const app = new Hono();

  app.use(logger());
  
  mapAliveEndpoint(app, appId);
  mapIngestMessagesEndpoint(app, repo);

  return app;
}

export const runApi = (app: ReturnType<typeof buildApi>) => serve(
  {
    fetch: app.fetch,
    port: 3000
  }, 
  (info) => console.log(`API is running on http://localhost:${info.port}`)
);
