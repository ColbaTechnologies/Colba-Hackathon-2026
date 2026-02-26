import store from "../config/database";
import { MessageData, messages } from "../models/messageData";
import { StatusType } from "../models/statusType";
import { queueService, scheduledQueueService } from "../services/queue";

export const saveMessage = async (message: MessageData): Promise<MessageData> => {
    const session = store.openSession();

    await session.store(message);
    await session.saveChanges();
    
    return message;
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