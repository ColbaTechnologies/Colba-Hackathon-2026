import EventEmitter from "events";
import { MessageData } from "../models/messageData";
import { WebSocketServer } from "ws";
import { ws } from "../websocket";


export const MESSAGEQUEUE: string = "messageQueue";
export const DEADLETTERQUEUE: string = "deadLetterQueue";
export const SCHEDULEDQUEUE: string = "scheduledQueue";

export class QueueService extends EventEmitter {
    private readonly queue: MessageData[] = [];

    constructor(private eventMessage: string,
        private wss: WebSocketServer
    ) {
        super();
    }

    public get length(): number {
        return this.queue.length;
    }

    public get empty(): boolean {
        return this.queue.length === 0;
    }

    public peek(): MessageData | undefined {
        return this.queue[0];
    }

    public enqueue(message: MessageData) {
        this.queue.push(message);
        this.notifyUpdate(this.eventMessage, message)
        this.emit(this.eventMessage, message);
    }
    public dequeue(): MessageData | undefined {
        return this.queue.shift();
    }
    public enqueueScheduled(message: MessageData) {
        let low = 0
        let high = this.queue.length

        while (low < high) {
            const mid = Math.floor((low + high) / 2)
            if (this.queue[mid].schedule! < message.schedule!) {
                low = mid + 1
            } else {
                high = mid
            }
        }

        this.queue.splice(low, 0, message);
        this.notifyUpdate(this.eventMessage, message)
        this.emit(this.eventMessage, message);
    }

    public notify(message: MessageData) {
        this.notifyUpdate(this.eventMessage, message);
    }

    private notifyUpdate(messageType: string, message: MessageData) {
        const payload = JSON.stringify({
            queueName: messageType,
            message: message
        });

        this.wss.clients.forEach(client => {
            if (client.readyState === WebSocket.OPEN) {
                client.send(payload);
            }
        });
    }
}

export const queueService = new QueueService(MESSAGEQUEUE, ws);
export const deadLetterQueueService = new QueueService(DEADLETTERQUEUE, ws);
export const scheduledQueueService = new QueueService(SCHEDULEDQUEUE, ws);