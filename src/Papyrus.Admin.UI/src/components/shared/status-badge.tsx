export function StatusBadge({ published }: { published: boolean }) {
  return (
    <span
      className={`rounded-none border px-2.5 py-1 text-xs font-semibold ${published ? "border-success/60 text-success" : "border-warning/60 text-warning"}`}
    >
      {published ? "Published" : "Draft"}
    </span>
  );
}
