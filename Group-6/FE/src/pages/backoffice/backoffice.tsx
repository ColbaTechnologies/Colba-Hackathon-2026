import { useState, useRef } from "react";
import Header from "@/components/ui/header";
import MessageCard from "./components/messageCard";
import type { MessageData } from "./components/messageCard";
import type { StatusType } from "./components/statusBadge";

const SLIDE_STYLE = `
@keyframes slideInLeft {
  from { transform: translateX(-200px); opacity: 0; }
  to   { transform: translateX(0);     opacity: 1; }
}
.msg-slide-in {
  animation: slideInLeft 1s cubic-bezier(0.22, 1, 0.36, 1) both;
}
`;


const MOCK_MESSAGES: MessageData[] = [
    {
        id: 1,
        url: "https://api.example.com/webhook",
        headers: '{"Content-Type":"application/json"}',
        payload: '{"event":"user.signup","userId":42}',
        schedule: "2026-02-26T10:00:00Z",
        status: "PENDING",
        retries: 0,
    },
    {
        id: 2,
        url: "https://hooks.slack.com/services/T00/B00/XYZ",
        headers: '{"Authorization":"Bearer sk-xxx"}',
        payload: '{"text":"Deployment succeeded on prod"}',
        schedule: "2026-02-26T10:05:00Z",
        status: "SENT",
        retries: 0,
    },
    {
        id: 3,
        url: "https://api.example.com/notify",
        headers: '{"Content-Type":"application/json"}',
        payload: '{"event":"order.shipped","orderId":99}',
        schedule: "2026-02-26T10:10:00Z",
        status: "RETRIED",
        retries: 2,
    },
    {
        id: 4,
        url: "https://api.dead-endpoint.com/push",
        headers: '{"X-API-Key":"abc123"}',
        payload: '{"ping":true}',
        schedule: "2026-02-26T09:55:00Z",
        status: "FAILED",
        retries: 3,
    },
    {
        id: 5,
        url: "https://api.example.com/webhook",
        headers: '{"Content-Type":"application/json"}',
        payload: '{"event":"payment.received","amount":120}',
        schedule: "2026-02-26T10:15:00Z",
        status: "PENDING",
        retries: 0,
    },
];


const DEMO_POOL: Omit<MessageData, "id" | "schedule">[] = [
    { url: "https://api.example.com/events", headers: '{"Content-Type":"application/json"}', payload: '{"event":"cart.checkout"}', status: "PENDING", retries: 0 },
    { url: "https://hooks.zapier.com/hooks/catch", headers: '{"Authorization":"Bearer zap-xxx"}', payload: '{"trigger":"new_lead"}', status: "PENDING", retries: 0 },
    { url: "https://api.stripe.com/webhooks", headers: '{"Stripe-Signature":"sig_xx"}', payload: '{"event":"invoice.paid"}', status: "SENT", retries: 0 },
    { url: "https://api.dead-svc.io/push", headers: '{"X-Key":"deadbeef"}', payload: '{"ping":true}', status: "FAILED", retries: 3 },
    { url: "https://api.example.com/retry", headers: '{"Content-Type":"application/json"}', payload: '{"event":"order.failed","id":77}', status: "RETRIED", retries: 1 },
];

let demoCounter = MOCK_MESSAGES.length + 1;

export default function Backoffice() {
    const [messages, setMessages] = useState<MessageData[]>(MOCK_MESSAGES);
    const [newIds, setNewIds] = useState<Set<number>>(new Set());
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    // TODO: replace mock with WebSocket
    // useEffect(() => {
    //   const ws = new WebSocket("ws://localhost:3000/queue");
    //   ws.onmessage = (e) => {
    //     const msg: MessageData = JSON.parse(e.data);
    //     addMessage(msg);
    //   };
    //   return () => ws.close();
    // }, []);

    function addMessage(msg: MessageData) {
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

    function handleAddDemo() {
        const template = DEMO_POOL[(demoCounter - MOCK_MESSAGES.length - 1) % DEMO_POOL.length];
        const msg: MessageData = {
            ...template,
            id: demoCounter++,
            schedule: new Date().toISOString(),
        };
        addMessage(msg);
    }

    const counts = messages.reduce(
        (acc, m) => ({ ...acc, [m.status]: (acc[m.status] ?? 0) + 1 }),
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
                    <button
                        onClick={handleAddDemo}
                        className="border border-black px-4 py-1.5 text-xs font-mono uppercase tracking-widest hover:bg-black hover:text-white transition-colors"
                    >
                        + Add message
                    </button>
                </div>

                <div className="grid grid-cols-4 gap-3">
                    {(["PENDING", "SENT", "RETRIED", "FAILED"] as StatusType[]).map((s) => (
                        <div key={s} className="border border-border p-3 flex flex-col gap-1">
                            <span className="text-xs font-mono uppercase tracking-widest text-muted-foreground">{s}</span>
                            <span className="text-2xl font-mono">{counts[s] ?? 0}</span>
                        </div>
                    ))}
                </div>

                <div className="flex flex-col gap-3">
                    {messages.map((msg) => (
                        <MessageCard key={msg.id} msg={msg} isNew={newIds.has(msg.id)} />
                    ))}
                </div>
            </main>
        </div>
    );
}
