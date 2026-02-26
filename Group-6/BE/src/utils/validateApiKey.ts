export const validateApiKey = (apiKey: string, expectedKey: string) => {
    if (apiKey !== expectedKey) {
        throw new Error("Invalid API key");
    }
};