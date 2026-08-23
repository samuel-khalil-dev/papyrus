import type { TextareaHTMLAttributes } from 'react';
export function Textarea({ className, ...props }: TextareaHTMLAttributes<HTMLTextAreaElement>) { return <textarea className={`flex min-h-20 w-full rounded-md border border-slate-300 bg-transparent px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-slate-400 ${className ?? ''}`} {...props} />; }
