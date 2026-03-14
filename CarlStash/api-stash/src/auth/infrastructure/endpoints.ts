import type { Hono } from "hono";
import type { TenantsRepository } from "..";
import { mapCreateApiKeyEndpoint } from "../create-apikey";

export const mapAuthEndpoints = (app: Hono, tenantsRepo: TenantsRepository) => {
  mapCreateApiKeyEndpoint(app, tenantsRepo);
}