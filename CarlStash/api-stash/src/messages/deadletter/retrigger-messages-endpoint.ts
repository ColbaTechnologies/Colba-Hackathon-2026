import type { Hono } from "hono";
import type { MessagesRepository } from "..";
import { validator } from "hono/validator";
import z from "zod";
import type { UUID } from "crypto";
import { retriggerMessage } from ".";
import { match } from "ts-pattern";

export const mapRetriggerMessagesEndpoint = (
  app: Hono, 
  repository: MessagesRepository
) => app.post(
  '/api/messages/retrigger/:messageId',
  validator('param', async (value, ctx) => {
    const result = await z.uuid().safeParseAsync(value.messageId);
    if (!result.success) { 
      console.error("/api/messages/retrigger Validation error:", result.error);
      return ctx.newResponse(null, 400);
    }
    return result.data as UUID;
  }),
  async (ctx) => {
    const messageId = ctx.req.valid('param');
    const retriggerResult = await retriggerMessage(repository, messageId);
    return match(retriggerResult)
      .with("NOT_FOUND_MESSAGE", () => ctx.newResponse(null, 404))
      .with("RETRIGGERED",       () => ctx.newResponse(null, 204))
      .exhaustive();
  }
);