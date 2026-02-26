export default function Hero() {
    return (
        <div className="flex flex-col justify-center gap-6 border-r border-border px-10 py-16">
            <h1 className="text-5xl font-extrabold leading-tight tracking-tight">
                <span className="text-foreground">Configure your</span> <br />
                <span className="text-primary">API request</span>
            </h1>
            <p className="max-w-sm text-sm leading-relaxed text-muted-foreground">
                Fill in the endpoint, payload and credentials. Schedule the message or send it right away.
            </p>
        </div>)
}


