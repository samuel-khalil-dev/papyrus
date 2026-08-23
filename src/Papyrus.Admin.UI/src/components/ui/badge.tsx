import type { HTMLAttributes } from 'react';
export function Badge({ className, ...props }: HTMLAttributes<HTMLDivElement>) { return <div className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold ${className ?? ''}`} {...props} />; }
