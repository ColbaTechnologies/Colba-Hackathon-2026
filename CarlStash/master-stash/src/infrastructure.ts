import { drizzle } from 'drizzle-orm/node-postgres';
import { pgTableCreator } from 'drizzle-orm/pg-core/table';

export const connectToDb = (url: string) => drizzle(url);

export type DB = ReturnType<typeof connectToDb>;

export const createTable = pgTableCreator((name) => `CARLSTASH_${name}`);