import { MessagingSDK } from "sdk";
import type { SDKConfig } from "sdk";

const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:3000";
const API_KEY = import.meta.env.VITE_API_KEY ?? "";

/** Default SDK instance (uses VITE_API_KEY env var, e.g. for the backoffice). */
export const sdk = new MessagingSDK({ baseUrl: BASE_URL, apiKey: API_KEY });

/** Create a one-off SDK instance with a specific API key (e.g. per form submission). */
export function createSdk(config: Partial<SDKConfig> = {}): MessagingSDK {
    return new MessagingSDK({
        baseUrl: config.baseUrl ?? BASE_URL,
        apiKey: config.apiKey ?? API_KEY,
    });
}
