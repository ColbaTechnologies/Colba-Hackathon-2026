import { MessageData } from "../models/messageData"
import { StatusType } from "../models/statusType"
import { saveMessage, fetchMessages } from "../repositories/messages.repository"
import { validateApiKey } from "../utils/validateApiKey"
import { queueService, scheduledQueueService } from "./queue"

export const getMessages = async () => {
    return await fetchMessages()
}

export const createMessage = async (messageData:  Omit<MessageData, "id" | "status" | "retries"> , apiKey: string) => {
    const expectedKey = "123456789"

    validateApiKey(apiKey, expectedKey)

    const message: MessageData = {
        ...messageData,
        status: StatusType.PENDING,
        retries: 0,
        id: crypto.randomUUID()
    }
    
    const savedMessage = await saveMessage(message)

    if (savedMessage.schedule) {
        scheduledQueueService.enqueueScheduled(savedMessage)
    } else {
        queueService.enqueue(savedMessage)
    }
    
    return savedMessage
}
