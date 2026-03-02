import { Request, Response } from "express"

export const messageReceived = async (req: Request, res: Response) => {
    try {
        console.log(`Message received at ${Date.now.toString()}`)
        res.status(200).json()
    } catch (error) {
        res.status(500).json({ error: (error as Error).message })
    }
}

