import { StatusType } from "../../models/statusType";
import { fetchMessages } from "../../repositories/messages.repository";
import { queueService, scheduledQueueService } from "../queue";

export async function enqueuePendingMessages() {
    let messages = await fetchMessages();
    messages.forEach(m => {
        if(m.status === StatusType.PENDING)
            m .schedule === null || m.schedule === undefined ? 
                queueService.enqueue(m)
                : scheduledQueueService.enqueue(m);
    })
}