import { useState } from "react";
import { toast } from "sonner";
import { createSdk } from "@/lib/sdk";
import type { UserFormValues } from "@/dto/userFormValues";

const initialValues: UserFormValues = {
    url: "",
    payload: "",
    apiKey: "",
    isScheduled: "no",
    scheduledTime: "",
};

export function useUserFormActions() {
    const [values, setValues] = useState<UserFormValues>(initialValues);
    const [submitted, setSubmitted] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [errors, setErrors] = useState<Partial<Record<keyof UserFormValues, string>>>({});

    function handleChange(
        e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
    ) {
        const { name, value } = e.target;
        setValues((prev) => ({
            ...prev,
            [name]: value,
            ...(name === "isScheduled" && value === "no" ? { scheduledTime: "" } : {}),
        }));
        setErrors((prev) => ({ ...prev, [name]: undefined }));
    }

    function validate(): boolean {
        const newErrors: Partial<Record<keyof UserFormValues, string>> = {};

        if (!values.url.trim()) {
            newErrors.url = "URL is required.";
        } else {
            try {
                new URL(values.url);
            } catch {
                newErrors.url = "Enter a valid URL.";
            }
        }

        if (!values.payload.trim()) newErrors.payload = "Payload is required.";
        if (!values.apiKey.trim()) newErrors.apiKey = "API Key is required.";

        if (values.isScheduled === "yes" && !values.scheduledTime) {
            newErrors.scheduledTime = "Please select a scheduled time.";
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    }

    function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        if (!validate()) return;

        setIsLoading(true);

        const client = createSdk({ apiKey: values.apiKey });

        client.sendMessage({
            url: values.url,
            payload: values.payload,
            headers: { "Content-Type": "application/json" },
            retries: 0,
            ...(values.isScheduled === "yes" && values.scheduledTime
                ? { schedule: new Date(values.scheduledTime) }
                : {}),
        })
            .then(() => {
                setSubmitted(true);
                toast.success("Message scheduled successfully!");
            })
            .catch((err: Error) => {
                toast.error(err.message);
            })
            .finally(() => {
                setIsLoading(false);
            });
    }

    function handleReset() {
        setValues(initialValues);
        setErrors({});
        setSubmitted(false);
    }

    return { values, errors, submitted, isLoading, handleChange, handleSubmit, handleReset };
}
