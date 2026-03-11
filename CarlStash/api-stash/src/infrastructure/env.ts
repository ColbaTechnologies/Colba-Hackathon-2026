import z from "zod";
import { config } from 'dotenv';
config();

export const env = z.object({
  DATABASE_URL: z.url(),
}).parse({
  DATABASE_URL: process.env.DATABASE_URL
});