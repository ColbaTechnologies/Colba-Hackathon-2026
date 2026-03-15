import type { Hono } from "hono";
import type { TenantsRepository } from ".";
import { validator } from "hono/validator";
import z from "zod";

const createTenantSchema = z.object({
  id: z.string().min(1),
  password: z.string().min(1)
});

export const mapCreateTenantEndpoint = (app: Hono, tenantsRepo: TenantsRepository) => app.post(
  "/api/tenants",
  validator('json', (body, c) => {
    const result = createTenantSchema.safeParse(body);
    if (!result.success) {
      return c.json({ error: "Invalid data" }, 400);
    }
    return result.data;
  }),
  async (c) => {
    const { id, password } = c.req.valid('json');
    await tenantsRepo.store({
      id,
      passwordHash: password // TODO - password should be hashed
    });

    const apiKey = await tenantsRepo.createApiKey(id);

    return c.json({ 
      message: "Tenant created",
      apiKey
    }, 201);
  },
);