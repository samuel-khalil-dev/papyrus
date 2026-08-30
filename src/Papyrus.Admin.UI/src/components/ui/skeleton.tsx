import type { HTMLAttributes } from "react";
export function Skeleton({
  className,
  ...props
}: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={`animate-pulse rounded-none bg-muted ${className ?? ""}`}
      {...props}
    />
  );
}
