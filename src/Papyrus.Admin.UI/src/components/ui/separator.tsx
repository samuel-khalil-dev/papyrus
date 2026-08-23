import type { HTMLAttributes } from 'react';
export function Separator({ className, ...props }: HTMLAttributes<HTMLDivElement>) { return <div role="separator" className={`h-px w-full bg-slate-200 dark:bg-slate-800 ${className ?? ''}`} {...props} />; }
