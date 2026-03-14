import type { Hono } from "hono";
import type { MessagesRepository } from "..";
import type { DB } from "../../infrastructure/drizzle";
import { mapQueryEndpoints } from "../query-messages-endpoint";
import { mapIngestMessagesEndpoint } from "../ingest-messages-endpoint";

export const mapMessagesEdpoints = (
  app: Hono,
  repo: MessagesRepository,
  db: DB
) => {
  mapIngestMessagesEndpoint(app, repo);
  mapQueryEndpoints(app, db);
}