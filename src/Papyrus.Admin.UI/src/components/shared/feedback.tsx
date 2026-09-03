import { WarningCircle, CircleNotch } from "@phosphor-icons/react";
import papyrusBackground from "@/assets/Papyrus_Background.svg";

export function LoadingState({
  label = "Loading workspace…",
}: {
  label?: string;
}) {
  return (
    <div className="flex items-center gap-2 py-12 text-sm text-muted-foreground">
      <CircleNotch
        className="animate-spin text-primary"
        size={18}
        weight="regular"
      />
      {label}
    </div>
  );
}
export function EmptyState({
  title,
  description,
}: {
  title: string;
  description: string;
}) {
  return (
    <div className="relative overflow-hidden rounded-none border border-dashed border-primary/50 p-12 text-center">
      <img
        src={papyrusBackground}
        alt=""
        className="pointer-events-none absolute inset-0 m-auto h-full w-full object-cover opacity-[.05]"
      />
      <div className="relative">
        <p className="font-semibold">{title}</p>
        <p className="mt-1 text-sm text-muted-foreground">{description}</p>
      </div>
    </div>
  );
}
export function ErrorState({
  message = "Unable to load this data.",
}: {
  message?: string;
}) {
  return (
    <div className="flex items-center gap-2 rounded-none border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
      <WarningCircle size={18} weight="regular" />
      {message}
    </div>
  );
}
