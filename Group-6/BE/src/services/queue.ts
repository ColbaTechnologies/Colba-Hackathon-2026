import EventEmitter from "events";
import { MessageData } from "../models/messageData";

class QueueService extends EventEmitter{
    private readonly store: MessageData[] = [];

    public get length(): number {
        return this.store.length;
    }

    public get empty(): boolean {
        return this.store.length === 0;
    }

    public peek(): MessageData | undefined {
        return this.store[0];
    }

    public enqueue(message: MessageData) {
        this.store.push(message);
        this.emit('messageAdded', message);
    }
    public dequeue(): MessageData | undefined {
        return this.store.shift();
    }
}

export const queueService = new QueueService();