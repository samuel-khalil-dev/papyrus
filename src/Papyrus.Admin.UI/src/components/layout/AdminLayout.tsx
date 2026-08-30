import { useState, type ReactNode } from "react";
import { Breadcrumbs } from "./Breadcrumbs";
import { Sidebar, type View } from "./Sidebar";
import { Topbar } from "./Topbar";
export function AdminLayout({
  view,
  onNavigate,
  children,
}: {
  view: View;
  onNavigate: (view: View) => void;
  children: ReactNode;
}) {
  const [open, setOpen] = useState(false);
  return (
    <div className="min-h-screen bg-background text-foreground">
      <div className="flex min-h-screen">
        <Sidebar
          view={view}
          onNavigate={onNavigate}
          open={open}
          onClose={() => setOpen(false)}
        />
        <div className="min-w-0 flex-1">
          <Topbar onMenu={() => setOpen(true)} onNavigate={onNavigate} />
          <main className="mx-auto max-w-7xl p-5 sm:p-8">
            <Breadcrumbs view={view} />
            {children}
          </main>
        </div>
      </div>
    </div>
  );
}
