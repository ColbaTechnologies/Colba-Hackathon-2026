export type Message = {
  id: string;
  destination: string;
  payload: string|undefined;
  tenant: string;
}

export type MessagesRepository = {
  save:         (input: { destination: string; payload: string|undefined; tenant: string }) => Promise<Message>;
  next:         () => Message | undefined;
  setAsSent:    (message: Message) => Promise<void>;
  setAsFailed:  (message: Message) => Promise<void>;
  getFailed:    (id: string) => Promise<Message | undefined>;
  retrigger:    (message: Message) => Promise<void>;
}