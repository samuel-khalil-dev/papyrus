import type { ButtonHTMLAttributes } from 'react';
import { cn } from '@/lib/utils';
export function Button({ className, ...props }: ButtonHTMLAttributes<HTMLButtonElement>) { return <button className={cn('inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium transition-colors disabled:pointer-events-none disabled:opacity-50 bg-slate-900 text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900', className)} {...props} />; }
