import { MessageData } from '../../models/messageData';
import { queueService } from '../queue';

export const initWorker = () => {
    console.log("🚀 Worker listening...");

    queueService.on('messageAdded', (message: MessageData) => {
        console.log(`New message detected: ${message.id}`);
        processMessage(message);
    });
};

async function processMessage(message: MessageData) {
    console.log(`Processing message:`, message.payload);
    
    queueService.dequeue();
    console.log(`Process finished: ${queueService.length}`);
}