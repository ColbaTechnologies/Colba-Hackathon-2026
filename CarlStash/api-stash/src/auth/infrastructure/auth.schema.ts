import { createTable } from "../../infrastructure/drizzle";

export const tenants = createTable("tenants", (t) => ({
  id:           t.text().primaryKey(),
  passwordHash: t.text().notNull(),
}));

export const apiKeys = createTable("api_keys", (t) => ({
  apikey:  t.text().primaryKey(),
  tenantId: t.text().notNull(),
}));