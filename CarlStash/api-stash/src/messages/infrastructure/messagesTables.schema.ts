import { createTable } from "../../infrastructure/drizzle";

export const pendingMessages = createTable(
  "pending_messages",
  (t) => ({
    id: t.uuid().primaryKey(),
    destination: t.text().notNull(),
    payload: t.text(),
    app: t.uuid().notNull()
  })
);

export const failedMessages = createTable(
  "failed_messages",
  (t) => ({
    id: t.uuid().primaryKey(),
    destination: t.text().notNull(),
    payload: t.text(),
  })
);

export const sentMessages = createTable(
  "sent_messages",
  (t) => ({
    id: t.uuid().primaryKey(),
    destination: t.text().notNull(),
    payload: t.text(),
  })
);