import Header from "@/components/ui/header";
import { UserForm } from "./userForm/userForm";
import Hero from "@/components/ui/hero";

export default function Landing() {
    return (
        <div className="min-h-screen bg-background text-foreground">
            <Header />
            <div className="grid min-h-[calc(100vh-57px)] grid-cols-1 lg:grid-cols-2 ">
                <div className="flex items-center justify-center border-r border-border">
                    <Hero />
                </div>
                <div className="flex items-center justify-center px-10 py-16">
                    <UserForm />
                </div>
            </div>
        </div>
    );
}
