import type { Hono } from "hono";
import { validator } from "hono/validator";
import z from "zod";
import type { MessagesRepository } from ".";

const ingestMessagesSchema = z.object({
  destination: z.url(),
  payload: z.string().optional(),
});

export const mapIngestMessagesEndpoint = (
  app: Hono, 
  repository: MessagesRepository
) => app.post(
  '/api/messages',
  validator('json', async (value, c) => {
    const result = await ingestMessagesSchema.safeParseAsync(value);
    if (!result.success) {
      console.error("/api/messages Validation error:", result.error);
      return c.newResponse(null, 400);
    }
    return result.data;
  }),
  async (c) => {
    const { destination, payload } =  c.req.valid('json');
    const message = await repository.save({ 
      destination: new URL(destination), 
      payload 
    });
    return c.json({ messageId: message.id }, 201);
  }
);