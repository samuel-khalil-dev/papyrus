import type { ReactNode } from 'react';
export function DropdownMenu({ children }: { children: ReactNode }) { return <div className="relative">{children}</div>; }
export function DropdownMenuTrigger({ children }: { children: ReactNode }) { return <>{children}</>; }
export function DropdownMenuContent({ children }: { children: ReactNode }) { return <div className="absolute right-0 z-20 mt-2 min-w-40 rounded-md border bg-white p-1 shadow-md dark:bg-slate-950">{children}</div>; }
export function DropdownMenuItem({ children }: { children: ReactNode }) { return <button className="w-full rounded px-2 py-1.5 text-left text-sm hover:bg-slate-100 dark:hover:bg-slate-800">{children}</button>; }
