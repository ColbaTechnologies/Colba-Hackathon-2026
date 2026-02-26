import store from "../../config/database"
import { MessageData } from "../../models/messageData"
import { queueService } from "../queue"

export async function checkScheduledMessages() {
    setInterval(async () => {
        const now = new Date()

        const session = store.openSession()
        const messages = await session
            .query<MessageData>({ collection: "Messages" })
            .whereEquals("status", "PENDING")
            .whereLessThanOrEqual("schedule", now)
            .all()

        for (const message of messages) {
            queueService.enqueue(message)
        }
    }, 5000)
}