import { UserForm } from "./userForm/userForm";

export default function Landing() {
    return (
        <div className="min-h-screen bg-background text-foreground">

            <div className="border-b border-border px-8 py-4">
                <img src="/logo.png" alt="CStash Logo" className="h-6 invert inline-block pr-5" />
                <span className="inline-flex items-center gap-2 border border-black px-3 py-1 text-xs font-mono tracking-widest uppercase text-black">
                    CStash
                </span>
            </div>


            <div className="grid min-h-[calc(100vh-57px)] grid-cols-1 lg:grid-cols-2">

                <div className="flex flex-col justify-center gap-6 border-r border-border px-10 py-16">
                    <h1 className="text-5xl font-extrabold leading-tight tracking-tight">
                        <span className="text-foreground">Configure your</span> <br />
                        <span className="text-primary">API request</span>
                    </h1>
                    <p className="max-w-sm text-sm leading-relaxed text-muted-foreground">
                        Fill in the endpoint, payload and credentials. Schedule the message or send it right away.
                    </p>
                </div>

                <div className="flex items-center justify-center px-10 py-16">
                    <UserForm />
                </div>
            </div>
        </div>
    );
}
