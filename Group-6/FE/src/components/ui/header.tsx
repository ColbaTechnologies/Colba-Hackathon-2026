export default function Header() {
    return (
        <div className="border-b border-border px-8 py-4">
            <img src="/logo.png" alt="CStash Logo" className="h-6 invert inline-block pr-5" />
            <span className="inline-flex items-center gap-2 border border-black px-3 py-1 text-xs font-mono tracking-widest uppercase text-black">
                CStash
            </span>
        </div>
    );
}