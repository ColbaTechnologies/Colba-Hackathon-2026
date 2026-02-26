import EventEmitter from "events";
import { MessageData } from "../models/messageData";
import { WebSocketServer } from "ws";
import config from "../config/config";
import { ws } from "../server";


export const MESSAGEQUEUE: string = "messageQueue";
export const DEADLETTERQUEUE: string = "deadLetterQueue";

class QueueService extends EventEmitter{
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
        var dequeuedMessage = this.queue.shift();
        this.notifyUpdate(this.eventMessage, dequeuedMessage!);
        return dequeuedMessage;
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
