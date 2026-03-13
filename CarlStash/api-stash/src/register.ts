import z from "zod";
import type { MessagesRepository } from "./messages";

const registrationResponseSchema = z.object({
  messages: z.array(z.object({
    id: z.uuid(),
  })),
});

export const runRegistraionProcess = (
  config: {
    appId: string;
    masterUrl: string;
    registrationEndpoint: string;
  },
  repo: MessagesRepository) => Promise.resolve(async () => {
  try {
    console.log('Starting registration process...');
    const response = await fetch(`${config.masterUrl}/${config.registrationEndpoint}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ appId: config.appId }),
    });

    if (!response.ok) {
      throw new Error(`Registration failed with status: ${response.status}`);
    }

    const data = await response.json();
    const parsedData = await registrationResponseSchema.parseAsync(data);

    for (const message of parsedData.messages) {
      await repo.addToQueue(message.id);
    }
    
    console.log('Registration process completed successfully.');
  } catch (error) {
    console.error('Error during registration process:', error);
  }
});