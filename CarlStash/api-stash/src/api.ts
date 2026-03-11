import { Hono } from "hono";
import { mapAliveEndpoint } from "./alive.endpoint.js";

export const buildApi = () => {
  const app = new Hono();
  
  mapAliveEndpoint(app);

  return app;
}