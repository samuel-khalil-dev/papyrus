import type { ReactNode } from 'react';
export function Tabs({ children }: { children: ReactNode }) { return <div>{children}</div>; }
export function TabsList({ children }: { children: ReactNode }) { return <div className="inline-flex h-10 items-center justify-center rounded-md bg-slate-100 p-1 dark:bg-slate-800">{children}</div>; }
export function TabsTrigger({ children }: { children: ReactNode }) { return <button className="rounded-sm px-3 py-1.5 text-sm font-medium">{children}</button>; }
export function TabsContent({ children }: { children: ReactNode }) { return <div className="mt-2">{children}</div>; }
