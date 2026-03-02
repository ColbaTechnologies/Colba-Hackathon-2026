export default function Detail({ label, value }: { label: string; value: string }) {
    return (
        <div className="flex flex-col gap-1">
            <span className="text-xs font-mono uppercase tracking-widest text-muted-foreground">{label}</span>
            <pre className="text-xs font-mono bg-muted p-2 overflow-x-auto whitespace-pre-wrap break-all">
                {value}
            </pre>
        </div>
    );
}
