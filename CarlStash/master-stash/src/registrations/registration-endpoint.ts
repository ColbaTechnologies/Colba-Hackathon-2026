import { randomUUID } from "crypto";
import type { Hono } from "hono";
import { validator } from "hono/validator";
import z from "zod";

const registrationRequestSchema = z.object({
  appId: z.uuid(),
});

export const mapRegistrationEndpoint = (app: Hono) => app.post(
  '/api/register',
  validator('json', async (value, c) => {
    if (typeof value.appId !== 'string' || value.appId.trim() === '') {
      console.error("/register Validation error: appId is required and must be a non-empty string");
      return c.newResponse(null, 400);
    }
  }),
  async (c) => {  
    return c.json({ 
      messages: [
        {
          id: randomUUID(),
        }
      ]
    });
  });