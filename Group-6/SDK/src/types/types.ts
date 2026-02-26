export type StatusType =
  | "PENDING"
  | "SENT"
  | "FAILED"

export interface Message {
  id: string
  url: string
  headers: Record<string, string>
  payload: string
  schedule: Date
  status: StatusType
  retries: number
}

export interface SendMessageInput {
  url: string
  headers: Record<string, string>
  payload: string
  schedule?: Date
}

export interface SDKConfig {
  baseUrl: string
  apiKey: string
}