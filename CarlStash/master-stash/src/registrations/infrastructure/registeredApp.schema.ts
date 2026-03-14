import { createTable } from "../../infrastructure.js";

export const locks = createTable(
  "locks",
  (t) => ({
    id: t.text().primaryKey(),
    app: t.uuid().notNull()
  })
);

export const registrations = createTable(
  "registrations",
  (t) => ({
    id:     t.uuid().primaryKey(),
    app:  t.uuid().primaryKey(),
    url:    t.text().notNull()
  })
);

export const pendingMessages = createTable(
  "pending_messages",
  (t) => ({
    id: t.uuid().primaryKey(),
    destination: t.text().notNull(),
    payload: t.text(),
    app: t.uuid().notNull(),
    tenant: t.uuid().notNull(),
  })
);