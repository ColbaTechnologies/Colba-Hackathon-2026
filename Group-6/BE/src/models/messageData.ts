import { StatusType } from "./statusType";

export interface MessageData {
    id: number;
    url: string;
    headers: string
    payload: string;
    schedule: Date;
    status: StatusType
    retries: number;
}
export let messages: MessageData[] = [];