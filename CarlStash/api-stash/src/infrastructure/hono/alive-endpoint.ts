import type { UUID } from "crypto";
import type { Hono } from "hono";

export const mapAliveEndpoint = (app: Hono, appId: UUID) => app.get(
  '/api/alive', 
  ctx => ctx.json({
    message: "I am alive and running!",
    appId
  }, 200)
);