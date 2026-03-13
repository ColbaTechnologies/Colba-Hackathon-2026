import type { UUID } from "crypto";
import type { MessagesRepository } from "..";

export const retriggerMessage = async (repo: MessagesRepository, messageId: UUID) => {
  const maybeMessage = await repo.getFailed(messageId);
  if (!maybeMessage) {
    console.warn(`Message with ID ${messageId} not found in failed messages.`);
    return "NOT_FOUND_MESSAGE" as const;
  }

  await repo.retrigger(maybeMessage);

  return "RETRIGGERED" as const;
}