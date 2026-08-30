import type { HTMLAttributes } from "react";
import { cn } from "@/lib/utils";
type BadgeVariant = "default" | "success" | "warning" | "danger";
export function Badge({ className, variant = "default", ...props }: HTMLAttributes<HTMLDivElement> & { variant?: BadgeVariant }) {
  return (
    <div
      className={cn("inline-flex items-center rounded-none border px-2.5 py-1 text-xs font-semibold", variant === "default" && "border-primary/60 text-primary", variant === "success" && "border-success/60 text-success", variant === "warning" && "border-warning/60 text-warning", variant === "danger" && "border-danger/60 text-danger", className)}
      {...props}
    />
  );
}
