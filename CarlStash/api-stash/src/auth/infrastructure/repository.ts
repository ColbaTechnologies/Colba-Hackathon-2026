import type { DB } from "../../infrastructure/drizzle";
import type { TenantsRepository } from "..";
import { apiKeys, tenants } from "./auth.schema";
import { eq } from "drizzle-orm";
import { createApiKey } from "../create-apikey";

export const tenantsRepository = (db: DB): TenantsRepository => ({
  store: async (tenant) => {
    await db.insert(tenants).values(tenant);
  },
  getById: async (tenantId) => {
    const result = await db.select().from(tenants).where(eq(tenants.id, tenantId));
    return result.at(0);
  },
  createApiKey: (tenantId) => createApiKey(db, tenantId),
  validateApiKey: async (apiKey) => {
    const result = await db
      .select()
      .from(apiKeys)
      .where(eq(apiKeys.apikey, apiKey));
    if (result.length === 0) return undefined;

    const record = result[0];
    const tenantResult = await db
      .select()
      .from(tenants)
      .where(eq(tenants.id, record.tenantId));
    if (tenantResult.length === 0) return undefined;

    const tenantRecord = tenantResult[0];
    return {
      id: tenantRecord.id,
      passwordHash: tenantRecord.passwordHash
    };
  }
});