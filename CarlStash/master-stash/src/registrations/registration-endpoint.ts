import { randomUUID } from "crypto";
import type { Hono } from "hono";
import { validator } from "hono/validator";
import z from "zod";
import type { AddAppToRegistration } from "./register.js";

const registrationRequestSchema = z.object({
  appId: z.uuid(),
  url: z.url()
});

export const mapRegistrationEndpoint = (
  app: Hono, 
  addAppToRegistration: AddAppToRegistration
) => app.post(
  '/api/register',
  validator('json', async (value, c) => {
    const result = await registrationRequestSchema.safeParseAsync(value);
    if (!result.success) {
      console.error("/api/register Validation error:", result.error);
      return c.newResponse(null, 400);
    }
    return result.data;
  }),
  async (c) => {
    const { appId, url } = c.req.valid("json");
    const messages = await addAppToRegistration(appId, url);
    return c.json({ 
      messages
    });
  });

  