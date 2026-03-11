import type { UUID } from "crypto";
import type { URL } from "url";

export type Message = {
  id: UUID;
  destination: URL;
  payload: string|undefined;
}

export type MessagesRepository = {
  save: (input: { destination: URL; payload: string|undefined }) => Promise<Message>;
  next: () => Message | undefined;
  setAsSent: (message: Message) => Promise<void>;
  setAsFailed: (message: Message) => Promise<void>;
  getFailed: (id: UUID) => Promise<Message | undefined>;
  setForRetrigger: (message: Message) => Promise<void>;
}