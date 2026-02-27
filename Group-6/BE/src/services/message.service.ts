import { MessageData, MessageDto } from "../models/messageData"
import { StatusType } from "../models/statusType"
import { saveMessage, fetchMessages } from "../repositories/messages.repository"
import { validateApiKey } from "../utils/validateApiKey"
import { queueService, scheduledQueueService } from "./queue"

export const getMessages = async () => {
    return await fetchMessages()
}

export const createMessage = async (messageData: MessageDto, apiKey: string) => {
    const expectedKey = "111"

    validateApiKey(apiKey, expectedKey)

    const savedMessage = await saveMessage(messageData)

    if (savedMessage.schedule) {
        scheduledQueueService.enqueueScheduled(savedMessage)
    } else {
        queueService.enqueue(savedMessage)
    }

    return savedMessage
}

