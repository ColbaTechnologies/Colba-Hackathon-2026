import { useState, useRef, useEffect } from "react";
import type { Message } from "sdk";
import Header from "@/components/ui/header";
import MessageCard from "./components/messageCard";
import type { StatusType } from "./components/statusBadge";
import { sdk } from "@/lib/sdk";

const SLIDE_STYLE = `
@keyframes slideInLeft {
  from { transform: translateX(-200px); opacity: 0; }
  to   { transform: translateX(0);     opacity: 1; }
}
.msg-slide-in {
  animation: slideInLeft 1s cubic-bezier(0.22, 1, 0.36, 1) both;
}
`;

export default function Backoffice() {
    const [messages, setMessages] = useState<Message[]>([]);
    const [newIds, setNewIds] = useState<Set<string>>(new Set());
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    useEffect(() => {
        sdk.getMessages()
            .then(setMessages)
            .catch((err: Error) => setError(err.message))
            .finally(() => setIsLoading(false));
    }, []);

    function addMessage(msg: Message) {
        setMessages((prev) => [msg, ...prev]);
        setNewIds((prev) => new Set(prev).add(msg.id));
        if (timerRef.current) clearTimeout(timerRef.current);
        timerRef.current = setTimeout(() => {
            setNewIds((prev) => {
                const next = new Set(prev);
                next.delete(msg.id);
                return next;
            });
        }, 600);
    }

    // Expose for external callers (e.g. WebSocket integration)
    void addMessage;

    const counts = messages.reduce(
        (acc, m) => ({ ...acc, [m.status]: (acc[m.status as StatusType] ?? 0) + 1 }),
        {} as Record<StatusType, number>
    );

    return (
        <div className="min-h-screen bg-background text-foreground">
            <style>{SLIDE_STYLE}</style>
            <Header />

            <main className="max-w-3xl mx-auto px-6 py-10 flex flex-col gap-8">
                <div className="flex items-end justify-between gap-4">
                    <div className="flex flex-col gap-1">
                        <h1 className="text-2xl font-mono tracking-tight">Message Queue</h1>
                        <p className="text-sm text-muted-foreground font-mono">
                            {messages.length} message{messages.length !== 1 ? "s" : ""} in queue
                        </p>
                    </div>
                </div>

                <div className="grid grid-cols-4 gap-3">
                    {(["PENDING", "SENT", "RETRIED", "FAILED"] as StatusType[]).map((s) => (
                        <div key={s} className="border border-border p-3 flex flex-col gap-1">
                            <span className="text-xs font-mono uppercase tracking-widest text-muted-foreground">{s}</span>
                            <span className="text-2xl font-mono">{counts[s] ?? 0}</span>
                        </div>
                    ))}
                </div>

                {isLoading && (
                    <p className="text-sm font-mono text-muted-foreground">Loading messages…</p>
                )}

                {error && (
                    <p className="text-sm font-mono text-destructive">Error: {error}</p>
                )}

                <div className="flex flex-col gap-3">
                    {messages.map((msg) => (
                        <MessageCard key={msg.id} msg={msg} isNew={newIds.has(msg.id)} />
                    ))}
                </div>
            </main>
        </div>
    );
}
