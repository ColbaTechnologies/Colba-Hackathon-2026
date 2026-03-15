import type { DB } from "../../infrastructure/drizzle";
import type { TenantsRepository } from "..";
import { apiKeys, tenants } from "./auth.schema";
import { eq } from "drizzle-orm";
import { createApiKey } from "../apiKeys/create-apikey";
import { validateApiKey } from "../apiKeys/validate-apikey";

export const tenantsRepository = (db: DB): TenantsRepository => ({
  store: async (tenant) => {
    await db.insert(tenants).values(tenant);
  },
  validate: async (id: string, password) => {
    const result = await db.select().from(tenants).where(eq(tenants.id, id));
    if (result.length === 0) return false;

    const record = result[0];
    return record.passwordHash === password; // TODO - password should be hashed
  },
  getById: async (tenantId) => {
    const result = await db.select().from(tenants).where(eq(tenants.id, tenantId));
    return result.at(0);
  },
  createApiKey: createApiKey(db),
  validateApiKey: validateApiKey(db)
});