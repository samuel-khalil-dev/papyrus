import { Command, Key, List, Moon, Sun, Terminal } from "@phosphor-icons/react";
import { useEffect, useState } from "react";
import { useAuth } from "@/hooks/useAuth";
import { useTenant } from "@/hooks/useTenant";
import { useTheme } from "@/components/shared/use-theme";

type View = "dashboard" | "posts" | "projects" | "media";

export function Topbar({
  onMenu,
  onNavigate,
}: {
  onMenu: () => void;
  onNavigate: (view: View) => void;
}) {
  const { theme, setTheme } = useTheme();
  const { tenantId, setTenantId } = useTenant();
  const { isAuthenticated } = useAuth();
  const [commandOpen, setCommandOpen] = useState(false);
  const [tenant, setTenant] = useState(tenantId);
  useEffect(() => {
    const handler = (event: KeyboardEvent) => {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        setCommandOpen(true);
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, []);
  const choose = (view: View) => {
    setCommandOpen(false);
    onNavigate(view);
  };
  return (
    <>
      <header className="sticky top-0 z-30 flex h-20 items-center justify-between border-b border-glass-border bg-[var(--color-glass)] px-4 backdrop-blur-[16px] backdrop-saturate-150 transition-colors sm:px-8">
        <button
          className="lg:hidden"
          onClick={onMenu}
          aria-label="Open navigation"
        >
          <List />
        </button>
        <div className="hidden items-center gap-2 text-sm text-muted-foreground sm:flex">
          <span className="h-2 w-2 bg-primary" />
          All systems operational
        </div>
        <div className="ml-auto flex items-center gap-2 sm:gap-4">
          <label className="hidden items-center gap-2 border border-border px-3 py-2 text-xs sm:flex">
            <span className="text-muted-foreground">Tenant</span>
            <input
              value={tenant}
              onChange={(event) => setTenant(event.target.value)}
              onBlur={() =>
                /^[a-z0-9][a-z0-9-]{0,62}$/.test(tenant) && setTenantId(tenant)
              }
              className="w-16 bg-transparent font-medium outline-none"
              aria-label="Tenant ID"
            />
          </label>
          <button
            onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
            className="p-2 hover:bg-muted"
            aria-label="Toggle theme"
          >
            {theme === "dark" ? (
              <Sun size={18} weight="regular" />
            ) : (
              <Moon size={18} weight="regular" />
            )}
          </button>
          <span
            title={isAuthenticated ? "API key configured" : "API key missing"}
            className="bg-muted p-2 text-primary"
          >
            <Key size={18} weight="regular" />
          </span>
          <button
            onClick={() => setCommandOpen(true)}
            className="p-2 hover:bg-muted"
            aria-label="Open command menu"
          >
            <Command size={18} weight="regular" />
          </button>
        </div>
      </header>
      {commandOpen && (
        <div
          className="fixed inset-0 z-50 grid place-items-start bg-background/80 p-5 pt-28 backdrop-blur-sm"
          onClick={() => setCommandOpen(false)}
        >
          <div
            className="w-full max-w-lg border border-primary/60 bg-card p-2 shadow-2xl"
            onClick={(event) => event.stopPropagation()}
          >
            <p className="border-b border-border px-3 py-3 text-xs uppercase tracking-[.2em] text-muted-foreground">
              <Terminal className="mr-2 inline" />
              Navigate
            </p>
            {(["dashboard", "posts", "projects", "media"] as const).map(
              (view) => (
                <button
                  key={view}
                  className="flex w-full items-center gap-3 px-3 py-3 text-left text-sm hover:bg-muted hover:text-primary"
                  onClick={() => choose(view)}
                >
                  <List size={16} weight="regular" />
                  {view}
                </button>
              ),
            )}
          </div>
        </div>
      )}
    </>
  );
}
