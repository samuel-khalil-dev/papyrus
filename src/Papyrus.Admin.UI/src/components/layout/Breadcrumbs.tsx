import type { View } from "./Sidebar";
export function Breadcrumbs({ view }: { view: View }) {
  return (
    <div className="mb-6 border-b border-border pb-3 text-sm text-muted-foreground">
      <span>Admin</span>
      <span className="mx-2">/</span>
      <span className="font-medium capitalize text-foreground">
        {view}
      </span>
    </div>
  );
}
