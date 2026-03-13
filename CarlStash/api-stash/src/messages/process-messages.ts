import { type Message, type MessagesRepository } from ".";

export const processMessages = (
  repo: MessagesRepository
) => () => {
  const message = repo.next();
  if (!message) return Promise.resolve();
  
  console.log("Processing message:", message.id);
  sendMessage(message)
    .then(response => response.ok 
      ? handleSuccess(repo, message) 
      : handleError(repo, message, response.statusText))
    .catch(error => handleError(repo, message, error));

  return Promise.resolve();
}

const sendMessage = (message: Message) => fetch(
  message.destination, 
  {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: message.payload
  }
);

const handleSuccess = async (repo: MessagesRepository, message: Message) => {
  console.log("Successfully processed message:", message.id);
  await repo.setAsSent(message);
}

const handleError = async (
  repo: MessagesRepository, 
  message: Message, 
  error: any
) => { 
  console.error("Error processing message:", message.id);
  console.error(message.id, error);
  console.log(`Retrying message ${message.id}...`);

  for(let i = 0; i < 3; i++) {
    const retryAttempt = i + 1;
    const retryDelay = 1000 * retryAttempt;
    console.log(`Waiting ${retryDelay}ms before next retry attempt for message ${message.id}...`);
    await new Promise(resolve => setTimeout(resolve, retryDelay));

    try {
      const response = await sendMessage(message);
      if (response.ok) {
        console.log(`Successfully processed message ${message.id} on retry attempt ${retryAttempt}`);
        await handleSuccess(repo, message);
        return;
      }
      console.error(`Retry attempt ${retryAttempt} failed for message ${message.id}:`, response.statusText);
    }
    catch (err) {
      console.error(`FAILED FETCH - Retry attempt ${retryAttempt} failed for message ${message.id}:`);
      console.error(message.id, err);
    }
  }
  console.error(`All retry attempts failed for message ${message.id}. Giving up.`);
  await repo.setAsFailed(message);
}