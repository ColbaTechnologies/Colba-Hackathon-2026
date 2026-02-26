import EventEmitter from "events";
import { MessageData } from "../models/messageData";

export const MESSAGEADDED: string = "messageAdded";
export const MESSAGEFAILED: string = "messageFailed";

class QueueService extends EventEmitter{
    private readonly store: MessageData[] = [];

    constructor(private eventMessage: string) {
        super();
    }

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
        this.emit(this.eventMessage, message);
    }
    public dequeue(): MessageData | undefined {
        return this.store.shift();
    }
}

export const queueService = new QueueService(MESSAGEADDED);
export const deadLetterQueueService = new QueueService(MESSAGEFAILED);