import type { UUID } from "crypto";
import type { Hono } from "hono";

type Dependencies = {
  appId: UUID;
}

export const mapAliveEndpoint = (app: Hono, { appId }: Dependencies) => app.get(
  '/api/alive', 
  (c) => c.text(`Alive! API ID: ${appId}`)
);