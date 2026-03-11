import { type Config } from "drizzle-kit";
import { env } from "./src/infrastructure/env";

export default {
  schema: "./src/**/*.schema.ts",
  dialect: "postgresql",
  dbCredentials: {
    url: env.DATABASE_URL,
  },
  tablesFilter: ["CARLSTASH_*"],
} satisfies Config;
