import type { Hono } from "hono";

export const mapAliveEndpoint = (app: Hono) => app.get(
  '/api/alive', 
  (c) => {
    return c.text('Alive!')
  }
);