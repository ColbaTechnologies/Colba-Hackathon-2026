import store from "../config/database";
import { MessageData, messages } from "../models/messageData";
import { StatusType } from "../models/statusType";

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