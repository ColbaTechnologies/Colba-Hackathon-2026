# COLBA HACKATHON

## Objective

It will be to create a messaging system that will deliver async requests from a sender to a reciever.

Our system should provide a endpoint that will recieve

- a destination URL
- a payload

The endpoint will return a 2XX response and later, on a **async way**, execute a POST request to the destination URL with the provided payload as the body.

**YOU CANNOT USE ANY QUEUE INFRASTRUCTURE OR NON-NATIVE TECHNOLOGY THAT PROVIDES A PRODUCER/CONSUMER PATTERN. THE CHALLENGE OF THIS HACKATHON IS TO BUILD IT BY OURSELVES.**

This hackathon is inspired by [QTASH](https://upstash.com/docs/qstash/overall/getstarted), take a look on their documentation to get a better idea of what we are building.

[This site](https://webhook.site) will be useful to test the outgoing message. 

## Criteria

- A working POC that at least acomplishes the main objetive.
- Clean code, project structure, good practices, well designed APIs …
- Scalabillity - what happens if we have 5000000x requests? how our system handles multiple replicas or multi-region? what happens if our system has a crash or we re-start it?
- Out of the confort zone - build the solution with a new language, technologies, patterns …
- Thinking out of the box - a crazy idea that works.
- The demo / presentation of your solution - as important as it is to build good solutions is our abillity to comunnicate our ideas.

## Secondary objetives / nice to have / extra points

- Scheduling - provide a way to trigger the request/message on a certain time.
- Topics - our system can deliver the same message to multiple recievers.
- Authentication / security - secure the endpoints by user/tenant, on the sender and the receiver.
- Retries - if the reciever fails to handle the request (returns something different from a 2XX) our system can plan and execute a retry strategy.
- A SDK to make life easier to our clients, for both sender and reciever.
- Status - provide a way so the users can track the status of the messages they have sent (message created, delivered, failed, pending to send, …).
- Not duplicate deliveries - the same message should not be delivered twice.
- Forward headers - the sender can provide special headers with the request that will be sent also to the reciever.
- FIFO - first message to arrive should be the first delivered, ensure the order.
- Callbacks - provide a way so the sender can recieve a notification when the message is delivered (or has failed to deliver).
- Dead letter - when delivering the message fails (or the retry strategy is completed) we can manually trigger it once again or delete it)
- UI - any kind of dashboard to manage our service from both the users and the service operators.
- Use your imagination - do you think a service like this should have a feature that is not mentioned above? Add it.

## How to participate

1. Fork or clone this repro.
2. Create a main branch with you name or your team name, something like: main-carlos, main-winnerteam
3. Create a folder with your name (individual or team).
4. Code (the code should be inside your folder).
5. Once finished the coding create a PR to the main repo branch with name your name. The judges will merge the PR.
6. Make a presentation / demo.
