import { ChartBar, FileText, Folder, Image, X } from "@phosphor-icons/react";
import { PapyrusLogo } from "@/components/shared/PapyrusLogo";
export type View = "dashboard" | "posts" | "projects" | "media";
const items = [
  { id: "dashboard" as View, label: "Overview", icon: ChartBar },
  { id: "posts" as View, label: "Blog posts", icon: FileText },
  { id: "projects" as View, label: "Projects", icon: Folder },
  { id: "media" as View, label: "Media library", icon: Image },
];
export function Sidebar({
  view,
  onNavigate,
  open,
  onClose,
}: {
  view: View;
  onNavigate: (view: View) => void;
  open: boolean;
  onClose: () => void;
}) {
  return (
    <aside
      aria-label="Admin navigation"
      className={`fixed inset-y-0 left-0 z-40 w-72 transform border-r border-glass-border bg-[var(--color-glass)] p-5 backdrop-blur-[16px] backdrop-saturate-150 transition-transform duration-250 lg:static lg:translate-x-0 ${open ? "translate-x-0" : "-translate-x-full"}`}
    >
      <header className="flex items-center justify-between px-2 py-3">
        <div className="flex items-center gap-3">
          <PapyrusLogo className="size-10 shrink-0 text-primary" />
          <div>
            <p className="font-display text-xl font-bold tracking-tight">
              papyrus<span className="text-primary">.</span>
            </p>
            <p className="text-xs text-muted-foreground">admin workspace</p>
          </div>
        </div>
        <button
          className="min-h-11 min-w-11 lg:hidden"
          onClick={onClose}
          aria-label="Close navigation"
        >
          <X size={20} weight="regular" />
        </button>
      </header>
      <nav className="mt-8 space-y-1" aria-label="Content management">
        {items.map((item) => {
          const Icon = item.icon;
          return (
            <button
              key={item.id}
              onClick={() => {
                onNavigate(item.id);
                onClose();
              }}
              className={`flex min-h-11 w-full items-center gap-3 rounded-none border-l-2 px-3 py-3 text-left text-sm font-medium transition ${view === item.id ? "border-primary bg-primary/10 text-primary" : "border-transparent text-muted-foreground hover:bg-muted hover:text-foreground"}`}
            >
              <Icon size={18} weight="regular" />
              {item.label}
            </button>
          );
        })}
      </nav>
      <div className="absolute bottom-6 left-5 right-5 border border-border bg-muted p-4 text-xs text-muted-foreground">
        <p className="font-semibold text-foreground">Content command center</p>
        <p className="mt-1">Manage your public presence with confidence.</p>
      </div>
    </aside>
  );
}
