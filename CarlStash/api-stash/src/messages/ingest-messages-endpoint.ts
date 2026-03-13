import type { Hono } from "hono";
import { validator } from "hono/validator";
import z from "zod";
import type { MessagesRepository } from ".";
import type { UUID } from "crypto";

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
  validator('header', async (headers, c) => {
    // TODO - should validate auth and get the user/tenantId
    return {
      tenant: "d699c742-70ab-4abd-a84c-6b8185c7b938" //returning a fake one for the moment
    };
  }),
  async (c) => {
    const { destination, payload } =  c.req.valid('json');
    const { tenant } = c.req.valid('header');
    const message = await repository.save({ destination, payload, tenant });
    return c.json({ messageId: message.id }, 201);
  }
);