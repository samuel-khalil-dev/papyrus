import type { ButtonHTMLAttributes } from "react";
import { cn } from "@/lib/utils";
type ButtonVariant = "default" | "outline" | "ghost" | "destructive";
export function Button({
  className,
  variant = "default",
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variant?: ButtonVariant }) {
  return (
    <button
      className={cn(
        "inline-flex items-center justify-center rounded-none border px-4 py-2 text-sm font-medium shadow-sm transition-all duration-150 hover:shadow-md disabled:pointer-events-none disabled:opacity-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        variant === "default" && "border-primary bg-primary text-primary-foreground hover:bg-primary/90",
        variant === "outline" && "border-primary bg-transparent text-primary hover:bg-primary/10",
        variant === "ghost" && "border-transparent bg-transparent text-primary hover:bg-primary/10",
        variant === "destructive" && "border-danger bg-danger text-primary-foreground hover:bg-danger/90",
        className,
      )}
      {...props}
    />
  );
}
