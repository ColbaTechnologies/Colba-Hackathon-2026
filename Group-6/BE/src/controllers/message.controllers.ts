import { Request, Response } from "express"
import { createMessage, getMessages } from "../services/message.service"

export const getMessagesController = (req: Request, res: Response) => {
    const messages = getMessages()
    res.status(200).json(messages)
}

export const createMessageController = (req: Request, res: Response) => {
    try {
        const apiKey = req.header('x-api-key')

        const { url, headers } = req.body

        if (!url || !headers || !apiKey) {
            return res.status(400).json({ error: "Missing required fields" })
        }

        const message = createMessage(req.body, apiKey)
        res.status(201).json(message)
    } catch (error) {
        res.status(500).json({error: (error as Error).message})
    }
}
