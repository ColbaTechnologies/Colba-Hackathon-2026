import axios, { AxiosRequestConfig } from 'axios';
import { MessageData } from '../../models/messageData';
import { deadLetterQueueService, MESSAGEQUEUE, queueService } from '../queue';
import PQueue from 'p-queue';
import { updateMessageStatus } from '../../repositories/messages.repository';
import { StatusType } from '../../models/statusType';

const workerQueue = new PQueue({ concurrency: 100 });

export const initWorker = () => {
    console.log("Worker listening...");

    queueService.on(MESSAGEQUEUE, (message: MessageData) => {
        console.log(`New message detected: ${message.id}`);
        workerQueue.add(async () => {
            try{
                await processMessage(message);
            }catch(error){
                deadLetterQueueService.enqueue(message);
            }
        });
    });
};

async function processMessage(message: MessageData, retryNumber: number = 1) {
    console.log(`Processing message:`, message.payload);
    
    const config: AxiosRequestConfig = {
        headers: message.headers,
        timeout: 5000
    };

    try {
        const response = await axios.post(message.url, message.payload, config);

        console.log(`Set message succesfully. ${response.status} - ${JSON.stringify(response.data)}`);
        queueService.dequeue();

        await updateMessageStatus(message.id, StatusType.SENT);

        return response.data;
    } catch (error: any) {
        if (axios.isAxiosError(error)) {
            console.error(`Error HTTP: ${error.response?.status} - ${error.message}`);
        } else {
            console.error(`Error:`, error);
        }

        console.log(`Retry number ${retryNumber}`)
        if(retryNumber >= 3){
            console.log(`Set message as FAILED`)
            await updateMessageStatus(message.id, StatusType.FAILED);
            throw error;
        }
        
        await processMessage(message, retryNumber++);
    }
}