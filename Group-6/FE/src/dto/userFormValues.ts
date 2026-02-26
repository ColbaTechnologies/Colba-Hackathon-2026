type UserFormValues = {
    url: string;
    payload: string;
    apiKey: string;
    isScheduled: "yes" | "no";
    scheduledTime: string;
};

export type { UserFormValues };