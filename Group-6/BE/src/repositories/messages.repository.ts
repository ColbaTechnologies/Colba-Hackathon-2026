import { randomUUID } from "crypto";
import store from "../config/database";
import { MessageData, MessageDto, messages } from "../models/messageData";
import { StatusType } from "../models/statusType";
import { queueService, scheduledQueueService } from "../services/queue";

export const saveMessage = async (message: MessageDto): Promise<MessageData> => {
    let messageData: MessageData = {
        id: randomUUID(),
        url: message.url,
        headers: message.headers,
        payload: message.url,
        schedule: message.schedule,
        retries: message.retries,
        createdAt: new Date(),
        status: StatusType.PENDING
    }
    const session = store.openSession();
    await session.store(message);
    await session.saveChanges();
    
    return messageData;
}

export const fetchMessages = async (): Promise<MessageData[]> => {
    const session = store.openSession();
    const messages = await session.query<MessageData>({ collection: "Messages" }).all();
    return messages;
}

export const updateMessageStatus = async (id: string, status: StatusType): Promise<MessageData | null> => {
    const session = store.openSession();
    const message = await session.load<MessageData>(id);

    if (!message) {
        return null;
    }

    message.status = status;
    await session.saveChanges();

    return message;
}

export async function loadPendingMessages() {
    const session = store.openSession();
    const pendingMessages: MessageData[] = await session
        .query<MessageData>({ collection: "Messages" })
        .whereEquals("status", StatusType.PENDING)
        .all();
    for (const msg of pendingMessages) {
        if (msg.schedule) {
            scheduledQueueService.enqueueScheduled(msg)
        } else {
            queueService.enqueue(msg)
        }
    }

    return pendingMessages;
}