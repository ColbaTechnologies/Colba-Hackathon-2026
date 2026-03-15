import type { Hono } from "hono";
import type { TenantsRepository } from "..";
import { mapCreateApiKeyEndpoint } from "../apiKeys/create-apikey";
import { mapCreateTenantEndpoint } from "../create-tenant-endpoint";

export const mapAuthEndpoints = (app: Hono, tenantsRepo: TenantsRepository) => {
  mapCreateApiKeyEndpoint(app, tenantsRepo);
  mapCreateTenantEndpoint(app, tenantsRepo);
}