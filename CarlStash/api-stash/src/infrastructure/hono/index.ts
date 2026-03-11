import { Hono } from "hono";
import { mapAliveEndpoint } from "./alive-endpoint";
import { logger } from "hono/logger";
import { mapIngestMessagesEndpoint } from "../../messages/ingest-messages-endpoint";
import { type MessagesRepository } from "../../messages";
import { randomUUID, type UUID } from "crypto";
import type { DB } from "../drizzle";

type Dependencies = {
  appId: UUID;
  repo: MessagesRepository;
}

export const buildApi = ({ appId, repo }: Dependencies) => {
  console.log(`Building API with ID: ${appId}`);

  const app = new Hono();

  app.use(logger());
  
  mapAliveEndpoint(app, { 
    appId 
  });
  mapIngestMessagesEndpoint(app, repo);

  return app;
}