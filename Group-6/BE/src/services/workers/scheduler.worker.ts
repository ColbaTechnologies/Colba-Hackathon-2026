import store from "../../config/database"
import { MessageData } from "../../models/messageData"
import { queueService, scheduledQueueService } from "../queue"
import { processMessage } from "./message-worker"

export async function checkScheduledMessages() {
    setInterval(async () => {
        const now = new Date()
        const message: MessageData = scheduledQueueService.peek() as MessageData
        if (message && message.schedule <= now) {
            processMessage(message, scheduledQueueService)
        }
    }, 60000)
}