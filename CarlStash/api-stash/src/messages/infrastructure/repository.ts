import type { UUID } from "crypto";
import type { Message, MessagesRepository } from "..";
import type { DB } from "../../infrastructure/drizzle";
import { failedMessages, pendingMessages, sentMessages } from "./messagesTables.schema";
import { eq } from "drizzle-orm";

const messages: Message[] = [];
  
const saveMessage = (appId: UUID, db: DB) => async (input: {
  destination: URL;
  payload: string|undefined;
}) => {
  const message = { 
    id: crypto.randomUUID(), 
    ...input
  };

  await db.insert(pendingMessages).values({
    id: message.id,
    destination: message.destination.toString(),
    payload: message.payload,
    app: appId
  });

  messages.push(message);
  return message;
}

const getNextMessage = () => messages.shift();

const setMessageAsSent = (db: DB) => async (message: Message) => db.transaction(async (tx) => {
  await tx.insert(sentMessages).values({
    id: message.id,
    destination: message.destination.toString(),
    payload: message.payload,
  });
  await tx.delete(pendingMessages).where(eq(pendingMessages.id, message.id));
});

const setMessageAsFailed = (db: DB) => async (message: Message) => db.transaction(async (tx) => {
  await tx.insert(failedMessages).values({
    id: message.id,
    destination: message.destination.toString(),
    payload: message.payload,
  });
  await tx.delete(pendingMessages).where(eq(pendingMessages.id, message.id));
});

const getFailedMessage = (db: DB) => async (id: UUID): Promise<Message | undefined> => {
  const result = await db.select().from(failedMessages).where(eq(failedMessages.id, id));
  if (result.length === 0) return undefined;

  const record = result[0];
  return {
    id: record.id as UUID,
    destination: new URL(record.destination),
    payload: record.payload ?? undefined
  }
}

const setForRetrigger = (appId: UUID, db: DB) => async (message: Message) => db.transaction(async (tx) => {
  await tx.insert(pendingMessages).values({
    id: message.id,
    destination: message.destination.toString(),
    app: appId,
      payload: message.payload,
  });
  await tx.delete(failedMessages).where(eq(failedMessages.id, message.id));
});

export const buildMessageRepository = (appId: UUID, db: DB) => ({
  save: saveMessage(appId, db),
  next: getNextMessage,
  setAsSent: setMessageAsSent(db),
  setAsFailed: setMessageAsFailed(db),
  getFailed: getFailedMessage(db),
  setForRetrigger: setForRetrigger(appId, db)
}) satisfies MessagesRepository;