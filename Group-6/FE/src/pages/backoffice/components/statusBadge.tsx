export type StatusType = "PENDING" | "SENT" | "RETRIED" | "FAILED";

export const STATUS_STYLES: Record<StatusType, string> = {
    PENDING: "bg-yellow-100 text-yellow-800 border-yellow-300",
    SENT: "bg-green-100  text-green-800  border-green-300",
    RETRIED: "bg-blue-100   text-blue-800   border-blue-300",
    FAILED: "bg-red-100    text-red-800    border-red-300",
};

export default function StatusBadge({ status }: { status: StatusType }) {
    return (
        <span
            className={`inline-block border px-2 py-0.5 text-xs font-mono tracking-widest uppercase ${STATUS_STYLES[status]}`}
        >
            {status}
        </span>
    );
}