import { useState } from "react";

export interface UserFormValues {
    url: string;
    payload: string;
    apiKey: string;
    isScheduled: "yes" | "no";
    scheduledTime: string;
}

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
        // TODO: wire up to API
        console.log("Submitting:", values);
        setSubmitted(true);
    }

    function handleReset() {
        setValues(initialValues);
        setErrors({});
        setSubmitted(false);
    }

    return { values, errors, submitted, handleChange, handleSubmit, handleReset };
}
