import type { ReactNode } from 'react';
export function Dialog({ open, children }: { open?: boolean; children: ReactNode }) { return open === false ? null : <>{children}</>; }
export function DialogContent({ children, className }: { children: ReactNode; className?: string }) { return <div role="dialog" aria-modal="true" tabIndex={-1} className={`fixed left-1/2 top-1/2 z-50 w-full max-w-lg -translate-x-1/2 -translate-y-1/2 rounded-lg border bg-white p-6 shadow-lg dark:bg-slate-950 ${className ?? ''}`}>{children}</div>; }
export function DialogHeader({ children }: { children: ReactNode }) { return <div className="mb-4 flex flex-col space-y-1.5">{children}</div>; }
export function DialogTitle({ children }: { children: ReactNode }) { return <h2 className="text-lg font-semibold">{children}</h2>; }
