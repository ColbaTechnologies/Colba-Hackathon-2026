import { randomInt } from "node:crypto";
import type { DB } from "../../infrastructure/drizzle";
import { apiKeys } from "../infrastructure/auth.schema";
import type { Hono } from "hono";
import { validator } from "hono/validator";
import type { TenantsRepository } from "..";

const API_KEY_LENGTH = 25;
const API_KEY_CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+[]{}<>?/";

const generateApiKey = () => Array.from(
  { length: API_KEY_LENGTH }, 
  () => API_KEY_CHARSET[randomInt(API_KEY_CHARSET.length)]
).join("");

export const createApiKey = (db: DB) => async (tenantId: string) => {
  const apikey = generateApiKey();
  await db.insert(apiKeys).values({
    apikey,
    tenantId
  });
  return apikey;
}

export const mapCreateApiKeyEndpoint = (
  app: Hono,
  repo: TenantsRepository
) => app.post(
  "/api/tenants/create-api-key", 
  validator('header', (header, c) => {
    const tenantId = header['x-api-tenant-id'];
    const password = header['x-api-password'];
    return (
      typeof tenantId !== 'string' || 
      tenantId.trim() === '' || 
      typeof password !== 'string' || 
      password.trim() === ''
    ) ? c.json({ error: "Missing or invalid data" }, 400) : {
      tenantId,
      password
    };
  }),
  async (c) => {
    const { tenantId, password } = c.req.valid('header');
    const isValid = await repo.validate(tenantId, password);
    if (!isValid) {
      return c.json({ error: "invalid" }, 401);
    }

    const apiKey = await repo.createApiKey(tenantId);
    return c.json({ apiKey }, 201);
  }
);