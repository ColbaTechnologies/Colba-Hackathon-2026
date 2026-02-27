import { UUID } from "node:crypto";
import { StatusType } from "./statusType";

export interface MessageData {
    id: UUID;
    url: string;
    headers: HeadersDictionary
    payload: string;
    schedule: Date;
    status: StatusType
    retries: number;
    createdAt: Date;
    
}
export let messages: MessageData[] = [];

interface HeadersDictionary {
    [key: string]: string;
}

export interface MessageDto {
    url: string;
    headers: HeadersDictionary
    payload: string;
    schedule: Date;
    retries: number;
}