import z from "zod";
import { config } from 'dotenv';
config();

export const env = z.object({
  DATABASE_URL: z.url(),
  MASTER_URL: z.url(),
  REGISTRATION_ENDPOINT: z.string(),
}).parse({
  DATABASE_URL: process.env.DATABASE_URL,
  MASTER_URL: process.env.MASTER_URL,
  REGISTRATION_ENDPOINT: "api/register",
});