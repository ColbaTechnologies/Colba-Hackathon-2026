import axios, { AxiosRequestConfig } from 'axios';
import { MessageData } from '../../models/messageData';
import { queueService } from '../queue';
import PQueue from 'p-queue';

const workerQueue = new PQueue({ concurrency: 100 });

export const initWorker = () => {
    console.log("Worker listening...");

    queueService.on('messageAdded', (message: MessageData) => {
        console.log(`New message detected: ${message.id}`);
        workerQueue.add(async () => {
            await processMessage(message);
        });
    });
};

async function processMessage(message: MessageData, retryNumber: number = 0) {
    console.log(`Processing message:`, message.payload);
    
    const config: AxiosRequestConfig = {
        //todo change this
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer TU_TOKEN_AQUI',
            'X-Custom-Header': 'ValorPersonalizado'
        },
        timeout: 5000
    };

    try {
        const response = await axios.post(message.url, message.payload, config);

        console.log(`Set message succesfully. ${response.status} - ${JSON.stringify(response.data)}`);
        queueService.dequeue();
        //TODO - Update STATUS of the message stored on db

        return response.data;
    } catch (error: any) {
        if (axios.isAxiosError(error)) {
            console.error(`Error HTTP: ${error.response?.status} - ${error.message}`);
        } else {
            console.error(`Error:`, error);
        }

        if(retryNumber > 3)
            throw error;

        await processMessage(message, retryNumber++);
    }
}