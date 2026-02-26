import { Message, SendMessageInput, SDKConfig } from "./types/types"

export class MessagingSDK {
    private baseUrl: string
    private apiKey: string

    constructor(config: SDKConfig) {
        this.baseUrl = config.baseUrl
        this.apiKey = config.apiKey
    }

    private async request<T>(path: string, options: RequestInit): Promise<T> {
        const response = await fetch(`${this.baseUrl}${path}`, {
            ...options,
            headers: {
                "Content-Type": "application/json",
                "x-api-key": this.apiKey,
                ...options.headers
            }
        })

        if (!response.ok) {
            const error = await response.json().catch(() => null)
            throw new Error(`Request failed with status ${response.status}: ${error?.error || "Unknown error"}`)
        }
        return await response.json()
    }

    async getMessages(): Promise<Message[]> {
    const data = await this.request<any[]>("/messages", {
      method: "GET"
    })

    return data.map(this.mapMessage)
  }

    async sendMessage(input: SendMessageInput): Promise<Message> {
        const body = {
            ...input,
            schedule: input.schedule?.toISOString()
        }

        const data = await this.request<any>(`/messages`, {
            method: "POST",
            body: JSON.stringify(body)
        })
        return this.mapMessage(data)
    }

    private mapMessage(data: any): Message {
        return {
            id: data.id,
            url: data.url,
            headers: data.headers,
            payload: data.payload,
            schedule: new Date(data.schedule),
            status: data.status,
            retries: data.retries
        }
    }
}