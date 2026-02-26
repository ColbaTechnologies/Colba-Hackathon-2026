import { useState } from "react";
import StatusBadge from "./statusBadge";
import type { StatusType } from "./statusBadge";
import Detail from "./detail";

export interface MessageData {
    id: number;
    url: string;
    headers: string;
    payload: string;
    schedule: string;
    status: StatusType;
    retries: number;
}

export default function MessageCard({ msg, isNew }: { msg: MessageData; isNew: boolean }) {
    const [expanded, setExpanded] = useState(false);

    return (
        <div className={`border border-border bg-card text-card-foreground p-5 flex flex-col gap-3${isNew ? " msg-slide-in" : ""}`}>
            <div className="flex items-start justify-between gap-4">
                <div className="flex items-center gap-3 min-w-0">
                    <span className="font-mono text-xs text-muted-foreground shrink-0">#{msg.id}</span>
                    <span className="font-mono text-sm truncate">{msg.url}</span>
                </div>
                <StatusBadge status={msg.status} />
            </div>

            <div className="flex items-center gap-6 text-xs font-mono text-muted-foreground">
                <span>
                    <span className="uppercase tracking-widest mr-1">Scheduled</span>
                    {new Date(msg.schedule).toLocaleString()}
                </span>
                {msg.retries > 0 && (
                    <span className="text-destructive">
                        {msg.retries} {msg.retries === 1 ? "retry" : "retries"}
                    </span>
                )}
            </div>

            <button
                onClick={() => setExpanded((p) => !p)}
                className="self-start text-xs font-mono underline underline-offset-2 text-muted-foreground hover:text-foreground transition-colors"
            >
                {expanded ? "hide details" : "show details"}
            </button>

            {expanded && (
                <div className="grid grid-cols-1 gap-2 border-t border-border pt-3">
                    <Detail label="Headers" value={msg.headers} />
                    <Detail label="Payload" value={msg.payload} />
                </div>
            )}
        </div>
    );
}
