type UserFormValues = {
    url: string;
    payload: string;
    apiKey: string;
    isScheduled: "yes" | "no";
    scheduledTime: string | null;
};

export type { UserFormValues };