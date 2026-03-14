import { randomUUID } from "crypto";
import type { DB } from "../infrastructure.js";
import { locks, pendingMessages, registrations } from "./infrastructure/registeredApp.schema.js";
import { eq } from "drizzle-orm";

const LOCK_ID = "REGISTRATION_LOCK";

export const addAppToRegistration = (db: DB) => async (appId: string, url: string) => {
  await lockRegistration(db, appId);

  const registeredApps = await db.select().from(registrations).where(eq(registrations.app, appId));
  const messages = await db.select().from(pendingMessages);

  const messagesToSend: typeof messages = [];
  const appsToRemove: typeof registeredApps = [];
  for (const message of messages) {
    const app = registeredApps.filter((app) => app.app === appId).at(0);
    if (!app) {
      messagesToSend.push(message);
      break;
    }

    const isAlive = await checkAppIsAlive(app.url);
    if (isAlive) {
      break;
    }

    appsToRemove.push(app);
    messagesToSend.push(message);
  }

  db.transaction(async (tx) => {
    for (const app of appsToRemove) {
      await tx.delete(registrations).where(eq(registrations.id, app.id));
    }

    const registeredApp = await tx
      .insert(registrations)
      .values({
        id: randomUUID(),
        app: appId,
        url
      }).returning();

    for (const message of messagesToSend) {
      await tx.update(pendingMessages).set({
        app: registeredApp[0].id
      }).where(eq(pendingMessages.id, message.id));
    }
  });

  await removeLock(db);

  return [{
    id: messagesToSend.map(x => x.id),
  }]
}

const lockRegistration = async (db: DB, appId: string) => {
  while (true) {
    try {
      await db.insert(locks).values({
        id: LOCK_ID,
        app: appId
      });
      return;
    } catch (error) {
      console.warn("Lock is currently held, retrying...", error);
      await new Promise((resolve) => setTimeout(resolve, 100)); // Wait before retrying
    }
  }
}

const removeLock = async (db: DB) => db.delete(locks).where(eq(locks.id, LOCK_ID));

const checkAppIsAlive = async (url: string) => {
  try {
    const response = await fetch(`${url}/api/alive`);
    return response.ok;
  } catch (error) {
    console.error(`Failed to check if app at ${url} is alive:`, error);
    return false;
  }
}

export type AddAppToRegistration = ReturnType<typeof addAppToRegistration>;