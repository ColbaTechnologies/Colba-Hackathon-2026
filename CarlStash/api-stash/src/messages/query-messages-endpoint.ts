import type { Hono } from "hono";
import type { DB } from "../infrastructure/drizzle";
import { failedMessages, pendingMessages, sentMessages } from "./infrastructure/messagesTables.schema";
import { and, eq } from "drizzle-orm";
import { validator } from "hono/validator";
import { getFakeTenant } from "../infrastructure/hono/getFakeTenant";

const mapGetMessageById = (app: Hono, db: DB) => app.get(
  '/api/messages/:id',
  validator('header', (header, c) => {
    // TODO - should validate auth and get the user/tenantId
    return {
      tenant: getFakeTenant(),
    };
  }),
  async (c) => {
    const { id } = c.req.param();
    const { tenant } = c.req.valid('header');

    const pending = await db
      .select()
      .from(pendingMessages)
      .where(
        and(
          eq(pendingMessages.id, id),
          eq(pendingMessages.tenant, tenant)
        ));
    if (pending.length > 0) { 
      return c.json(pending[0]);
    }

    const failed = await db
      .select()
      .from(failedMessages)
      .where(
        and(
          eq(failedMessages.id, id),
          eq(failedMessages.tenant, tenant)
        ));
    if (failed.length > 0) {
      return c.json(failed[0]);
    }

    const sent = await db
      .select()
      .from(sentMessages)
      .where(
        and(
          eq(sentMessages.id, id),
          eq(sentMessages.tenant, tenant)
        ));
    if (sent.length > 0) {
      return c.json(sent[0]);
    }

    return c.newResponse(null, 404);
  }
);

const mapGetPendingMessages = (app: Hono, db: DB) => app.get(
  '/api/messages/pending',
  validator('header', (header, c) => {
    // TODO - should validate auth and get the user/tenantId
    return {
      tenant: getFakeTenant(),
    };
  }),
  async (c) => {
    const { tenant } = c.req.valid('header');
    const pending = await db
      .select()
      .from(pendingMessages)
      .where(eq(pendingMessages.tenant, tenant));
    return c.json(pending);
  }
);

const mapGetFailedMessages = (app: Hono, db: DB) => app.get(
  '/api/messages/failed',
  validator('header', (header, c) => {
    // TODO - should validate auth and get the user/tenantId
    return {
      tenant: getFakeTenant(),
    };
  }),
  async (c) => {
    const { tenant } = c.req.valid('header');
    const failed = await db
      .select()
      .from(failedMessages)
      .where(eq(failedMessages.tenant, tenant));
    return c.json(failed);
  }
);

const mapGetSentMessages = (app: Hono, db: DB) => app.get(
  '/api/messages/sent',
  validator('header', (header, c) => {
    // TODO - should validate auth and get the user/tenantId
    return {
      tenant: getFakeTenant(),
    };
  }),
  async (c) => {
    const { tenant } = c.req.valid('header');
    const sent = await db
      .select()
      .from(sentMessages)
      .where(eq(sentMessages.tenant, tenant));
    return c.json(sent);
  }
);

export const mapQueryEndpoints = (app: Hono, db: DB) => {
  mapGetMessageById(app, db);
  mapGetPendingMessages(app, db);
  mapGetFailedMessages(app, db);
  mapGetSentMessages(app, db);
};