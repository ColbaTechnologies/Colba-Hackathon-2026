import { useState, useRef, useEffect, type Dispatch, type SetStateAction, type MutableRefObject } from "react";
import type { Message } from "sdk";
import Header from "@/components/ui/header";
import MessageCard from "./components/messageCard";
import type { StatusType } from "./components/statusBadge";
import { sdk } from "@/lib/sdk";

const WS_URL = (import.meta.env.VITE_API_URL ?? "http://localhost:3000")
    .replace(/^http/, "ws");

const SLIDE_STYLE = `
@keyframes slideInLeft {
  from { transform: translateX(-200px); opacity: 0; }
  to   { transform: translateX(0);     opacity: 1; }
}
.msg-slide-in {
  animation: slideInLeft 1s cubic-bezier(0.22, 1, 0.36, 1) both;
}
@keyframes slideOutRight {
  from { transform: translateX(0);      opacity: 1; }
  to   { transform: translateX(200px);  opacity: 0; }
}
.msg-slide-out {
  animation: slideOutRight 0.5s cubic-bezier(0.22, 1, 0.36, 1) both;
}
`;

function isScheduledPending(m: Message): boolean {
    return m.status === "PENDING" && m.schedule > new Date();
}

function flashId(
    id: string,
    setter: Dispatch<SetStateAction<Set<string>>>,
    timerRef: MutableRefObject<ReturnType<typeof setTimeout> | null>
) {
    setter((prev) => new Set(prev).add(id));
    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => {
        setter((prev) => {
            const next = new Set(prev);
            next.delete(id);
            return next;
        });
    }, 600);
}

export default function Backoffice() {
    const [messages, setMessages] = useState<Message[]>([]);
    const [scheduledMessages, setScheduledMessages] = useState<Message[]>([]);
    const [deadLetters, setDeadLetters] = useState<Message[]>([]);
    const [newIds, setNewIds] = useState<Set<string>>(new Set());
    const [newSlIds, setNewSlIds] = useState<Set<string>>(new Set());
    const [newDlIds, setNewDlIds] = useState<Set<string>>(new Set());
    const [exitingIds, setExitingIds] = useState<Set<string>>(new Set());
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const slTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const dlTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    useEffect(() => {
        sdk.getMessages()
            .then((all) => {
                setScheduledMessages(all.filter(isScheduledPending));
                setMessages(all.filter((m) => m.status !== "FAILED" && !isScheduledPending(m)));
                setDeadLetters(all.filter((m) => m.status === "FAILED"));
            })
            .catch((err: Error) => setError(err.message))
            .finally(() => setIsLoading(false));
    }, []);

    useEffect(() => {
        const socket = new WebSocket(WS_URL);

        socket.onmessage = (event) => {
            try {
                const { queueName, message: raw } = JSON.parse(event.data) as {
                    queueName: string;
                    message: Message;
                };
                const msg: Message = { ...raw, schedule: new Date(raw.schedule) };

                if (queueName === "deadLetterQueue") {
                    // Move message out of all other queues and into dead letters
                    setMessages((prev) => prev.filter((m) => m.id !== msg.id));
                    setScheduledMessages((prev) => prev.filter((m) => m.id !== msg.id));
                    setDeadLetters((prev) => {
                        const idx = prev.findIndex((m) => m.id === msg.id);
                        if (idx === -1) return [msg, ...prev];
                        const next = [...prev];
                        next[idx] = msg;
                        return next;
                    });
                    flashId(msg.id, setNewDlIds, dlTimerRef);
                } else if (queueName === "scheduledQueue") {
                    if (msg.status === "PENDING") {
                        // Add/update in scheduled list
                        setScheduledMessages((prev) => {
                            const idx = prev.findIndex((m) => m.id === msg.id);
                            if (idx === -1) return [msg, ...prev];
                            const next = [...prev];
                            next[idx] = msg;
                            return next;
                        });
                        flashId(msg.id, setNewSlIds, slTimerRef);
                    } else {
                        // Message has been processed (SENT) — slide it out of the scheduled queue
                        setScheduledMessages((prev) => {
                            const idx = prev.findIndex((m) => m.id === msg.id);
                            if (idx === -1) return prev;
                            const next = [...prev];
                            next[idx] = msg;
                            return next;
                        });
                        setTimeout(() => {
                            setExitingIds((prev) => new Set(prev).add(msg.id));
                            setTimeout(() => {
                                setScheduledMessages((prev) => prev.filter((m) => m.id !== msg.id));
                                setExitingIds((prev) => { const next = new Set(prev); next.delete(msg.id); return next; });
                            }, 500);
                        }, 3000);
                    }
                } else {
                    // messageQueue — add/update in main messages list
                    if (msg.status === "SENT") {
                        // Update status badge, then animate the card out
                        setMessages((prev) => {
                            const idx = prev.findIndex((m) => m.id === msg.id);
                            if (idx === -1) return prev;
                            const next = [...prev];
                            next[idx] = msg;
                            return next;
                        });
                        setTimeout(() => {
                            setExitingIds((prev) => new Set(prev).add(msg.id));
                            setTimeout(() => {
                                setMessages((prev) => prev.filter((m) => m.id !== msg.id));
                                setExitingIds((prev) => { const next = new Set(prev); next.delete(msg.id); return next; });
                            }, 500);
                        }, 3000);
                    } else {
                        setMessages((prev) => {
                            const idx = prev.findIndex((m) => m.id === msg.id);
                            if (idx === -1) return [msg, ...prev];
                            const next = [...prev];
                            next[idx] = msg;
                            return next;
                        });
                        flashId(msg.id, setNewIds, timerRef);
                    }
                }
            } catch {
                // ignore malformed frames
            }
        };

        return () => {
            if (socket.readyState === WebSocket.OPEN) {
                socket.close();
            } else {
                socket.onopen = () => socket.close();
            }
        };
    }, []);

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
                    <div className="flex flex-col gap-1">
                        <h2 className="text-xl font-mono tracking-tight text-green-500">Message Queue</h2>
                        <p className="text-sm text-muted-foreground font-mono">
                            {messages.length} message{messages.length !== 1 ? "s" : ""} in queue
                        </p>
                    </div>
                    {messages.map((msg) => (
                        <MessageCard key={msg.id} msg={msg} isNew={newIds.has(msg.id)} isExiting={exitingIds.has(msg.id)} />
                    ))}
                </div>

                {/* Dead Letter Queue */}
                <div className="flex flex-col gap-3">
                    <div className="flex flex-col gap-1">
                        <h2 className="text-xl font-mono tracking-tight text-destructive">Dead Letter Queue</h2>
                        <p className="text-sm text-muted-foreground font-mono">
                            {deadLetters.length} failed message{deadLetters.length !== 1 ? "s" : ""}
                        </p>
                    </div>
                    {deadLetters.length === 0 ? (
                        <p className="text-sm font-mono text-muted-foreground">No failed messages.</p>
                    ) : (
                        deadLetters.map((msg) => (
                            <MessageCard key={msg.id} msg={msg} isNew={newDlIds.has(msg.id)} />
                        ))
                    )}
                </div>

                {/* Scheduled Queue */}
                <div className="flex flex-col gap-3">
                    <div className="flex flex-col gap-1">
                        <h2 className="text-xl font-mono tracking-tight text-yellow-500">Scheduled Queue</h2>
                        <p className="text-sm text-muted-foreground font-mono">
                            {scheduledMessages.length} scheduled message{scheduledMessages.length !== 1 ? "s" : ""}
                        </p>
                    </div>
                    {scheduledMessages.length === 0 ? (
                        <p className="text-sm font-mono text-muted-foreground">No scheduled messages.</p>
                    ) : (
                        scheduledMessages.map((msg) => (
                            <MessageCard key={msg.id} msg={msg} isNew={newSlIds.has(msg.id)} isExiting={exitingIds.has(msg.id)} />
                        ))
                    )}
                </div>
            </main>
        </div>
    );
}
