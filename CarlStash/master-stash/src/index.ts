import { serve } from '@hono/node-server'
import { Hono } from 'hono'
import { mapRegistrationEndpoint } from './registrations/registration-endpoint.js'
import { connectToDb } from './infrastructure.js'
import { addAppToRegistration } from './registrations/register.js'
import { logger } from 'hono/logger'

const dbUrl = process.env.DATABASE_URL;
if (!dbUrl) {
  throw new Error("DATABASE_URL environment variable is not defined");
}

const db = connectToDb(dbUrl);

const app = new Hono();

app.use(logger());

app.get('/', (c) => {
  return c.text('Hello Hono!')
})

mapRegistrationEndpoint(app, addAppToRegistration(db));



serve({
  fetch: app.fetch,
  port: 3000
}, (info) => {
  console.log(`Server is running on http://localhost:${info.port}`)
})
