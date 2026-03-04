To run mock receiver:

python3 -m uvicorn tests.mock_receiver:app --host 0.0.0.0 --port 9000

curl -X POST http://localhost:8000/api/v1/messages \
-H 'Content-Type: application/json' \
-d '{"destination_url":"http://localhost:9000/receive","payload":{"hello":"world"}}'


Async Messaging / Webhook Delivery – Test Plan Overview

  This test plan covers validation of a classic asynchronous messaging delivery system.

    The system flow:

      A client submits a message to a queue endpoint.

      The system accepts the request and processes it asynchronously.
      The system forwards the payload to a provided destination_url.
      The receiver endpoint processes the forwarded payload.

  This test collection will be implemented using Postman to validate the complete workflow.

  Key characteristics:

    Asynchronous processing
    Webhook-based delivery
    Delivery status tracking
    Retry/error handling validation

Test Scenarios

  1️⃣ Submit a Message (Queue Ingestion)

    Objective:
    Verify that a message can be successfully submitted for asynchronous processing.

    Method: POST
    Endpoint: /queue

    Validations:

      Response status code is 202 Accepted (or expected async response)

      A job/message ID is returned
      Request is stored for async processing
      Proper validation errors for:
      Missing destination_url
      Invalid URL format
      Missing payload
      Malformed JSON

  2️⃣ Check Delivery Status (Async Job Tracking)

    Objective:
    Verify that the system properly tracks the asynchronous job status.

    Method: GET
    Endpoint: /queue/{job_id} (or equivalent status endpoint)

    Expected States:

    Pending
    Processing
    Delivered
    Failed
    Retrying

    Validations:

    Status transitions correctly
    Delivered state confirms successful webhook call
    Failed state includes:
    Error message
    Retry count
    HTTP response code from receiver

3️⃣ Simulate the Receiver (Mock Webhook Endpoint)

    Objective:
    Simulate the external service that receives forwarded payloads.

    Validations:

    Payload received matches original submission
    Headers are correctly forwarded (e.g., content-type, signature if applicable)
    Retry behavior triggers on:
    500 response
    Timeout
    4xx/5xx errors (based on system design)


Edge Case Scenarios.

🔹 Invalid Destination URL

    DNS failure
    Unreachable host
    HTTPS certificate error

🔹 Timeout Handling

    Receiver does not respond within configured timeout

🔹 Retry Logic

    Confirm exponential backoff (if implemented)
    Confirm max retry limit
    Confirm dead-letter handling (if applicable)

🔹 Idempotency

    Submitting duplicate payloads
    Ensuring no duplicate deliveries (if system supports idempotency keys)


Success Criteria

    The system is considered validated when:
    Messages are accepted and queued successfully
    Status transitions behave correctly
    Webhook delivery occurs as expected
    Retry logic functions correctly
    Failure states are properly reported
    No data loss occurs under normal load
