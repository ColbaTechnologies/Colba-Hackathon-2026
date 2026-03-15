import type { DB } from "../../infrastructure/drizzle";
import { apiKeys, tenants } from "../infrastructure/auth.schema";
import { eq } from "drizzle-orm";

export const validateApiKey = (db: DB) => async (apiKey: string) => { 
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
