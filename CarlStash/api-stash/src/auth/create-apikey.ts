import { randomInt } from "node:crypto";
import type { DB } from "../infrastructure/drizzle";
import { apiKeys } from "./infrastructure/auth.schema";

const API_KEY_LENGTH = 25;
const API_KEY_CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+[]{}<>?/";

const generateApiKey = () => Array.from(
  { length: API_KEY_LENGTH }, 
  () => API_KEY_CHARSET[randomInt(API_KEY_CHARSET.length)]
).join("");

export const createApiKey = async (db: DB, tenantId: string) => {
  const apikey = generateApiKey();
  await db.insert(apiKeys).values({
    apikey,
    tenantId
  });
  return apikey;
} 