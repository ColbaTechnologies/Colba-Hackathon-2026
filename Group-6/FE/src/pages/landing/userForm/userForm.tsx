import { Button } from "@/components/ui/button";
import { useUserFormActions } from "./hooks/useUserFormActions";

const fieldCls =
    "w-full rounded-none border-0 border-b border-border bg-transparent px-0 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:border-primary focus:outline-none transition-colors";

const labelCls = "text-xs font-mono tracking-widest uppercase text-muted-foreground";

export function UserForm() {
    const { values, errors, submitted, handleChange, handleSubmit, handleReset } =
        useUserFormActions();

    if (submitted) {
        return (
            <div className="flex flex-col items-center gap-4 border border-primary/30 bg-background-subtle p-10 text-center">
                <span className="inline-flex items-center gap-2 text-xs font-mono tracking-widest uppercase text-primary">
                    <span className="size-1.5 rounded-full bg-primary" />
                    Submitted
                </span>
                <p className="text-lg font-bold text-foreground">Request queued successfully</p>
                <Button
                    variant="outline"
                    onClick={handleReset}
                    className="mt-2 rounded-none border-primary text-primary hover:bg-primary hover:text-primary-foreground"
                >
                    Submit another
                </Button>
            </div>
        );
    }

    return (
        <form
            onSubmit={handleSubmit}
            className="flex flex-col gap-7 w-full max-w-md"
        >
            {/* Title */}
            <div className="border-b border-border pb-4">
                <span className="text-xs font-mono tracking-widest uppercase text-primary">New request</span>
            </div>

            {/* URL */}
            <div className="flex flex-col gap-2">
                <label htmlFor="url" className={labelCls}>URL</label>
                <input
                    id="url"
                    name="url"
                    type="text"
                    placeholder="https://example.com/api/endpoint"
                    value={values.url}
                    onChange={handleChange}
                    className={fieldCls}
                />
                {errors.url && <p className="text-xs text-destructive">{errors.url}</p>}
            </div>

            {/* Payload */}
            <div className="flex flex-col gap-2">
                <label htmlFor="payload" className={labelCls}>Payload</label>
                <textarea
                    id="payload"
                    name="payload"
                    rows={4}
                    placeholder='{"key": "value"}'
                    value={values.payload}
                    onChange={handleChange}
                    className={`${fieldCls} resize-none font-mono`}
                />
                {errors.payload && <p className="text-xs text-destructive">{errors.payload}</p>}
            </div>

            {/* API Key */}
            <div className="flex flex-col gap-2">
                <label htmlFor="apiKey" className={labelCls}>API Key</label>
                <input
                    id="apiKey"
                    name="apiKey"
                    type="password"
                    placeholder="••••••••••••••••"
                    value={values.apiKey}
                    onChange={handleChange}
                    className={fieldCls}
                />
                {errors.apiKey && <p className="text-xs text-destructive">{errors.apiKey}</p>}
            </div>

            {/* Scheduled */}
            <div className="flex flex-col gap-3">
                <label className={labelCls}>Scheduled message?</label>
                <div className="flex gap-6">
                    {(["yes", "no"] as const).map((opt) => (
                        <label
                            key={opt}
                            className={`flex items-center gap-2 cursor-pointer select-none text-sm font-mono tracking-wider uppercase transition-colors ${values.isScheduled === opt
                                ? "text-primary"
                                : "text-muted-foreground hover:text-foreground"
                                }`}
                        >
                            <span
                                className={`size-3 rounded-full border ${values.isScheduled === opt
                                    ? "border-primary bg-primary"
                                    : "border-border"
                                    }`}
                            />
                            <input
                                type="radio"
                                name="isScheduled"
                                value={opt}
                                checked={values.isScheduled === opt}
                                onChange={handleChange}
                                className="sr-only"
                            />
                            {opt === "yes" ? "Yes" : "No"}
                        </label>
                    ))}
                </div>
            </div>

            {/* Time picker – shown only when scheduled */}
            {values.isScheduled === "yes" && (
                <div className="flex flex-col gap-2">
                    <label htmlFor="scheduledTime" className={labelCls}>Scheduled time</label>
                    <input
                        id="scheduledTime"
                        name="scheduledTime"
                        type="datetime-local"
                        value={values.scheduledTime}
                        onChange={handleChange}
                        className={`${fieldCls} [color-scheme:dark]`}
                    />
                    {errors.scheduledTime && (
                        <p className="text-xs text-destructive">{errors.scheduledTime}</p>
                    )}
                </div>
            )}

            <Button
                type="submit"
                className="mt-2 w-full rounded-none bg-primary text-primary-foreground font-mono tracking-widest uppercase hover:bg-primary-hover"
            >
                Submit
            </Button>
        </form>
    );
}
