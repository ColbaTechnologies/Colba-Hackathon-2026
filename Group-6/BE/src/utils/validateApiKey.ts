export const validateApiKey = (headersString: string, expectedKey: string) => {

    let headers: Record<string, any>;
    try {
        headers = JSON.parse(headersString);
    } catch (e) {
        throw new Error("Invalid headers format with error: " + (e as Error).message);
    }

    const apiKeyFromHeaders = headers["x-api-key"];
    if (apiKeyFromHeaders !== expectedKey) {
        throw new Error("Invalid API key");
    }
};