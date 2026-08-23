import type { SelectHTMLAttributes } from 'react';
export function Select(props: SelectHTMLAttributes<HTMLSelectElement>) { return <select className={`h-10 rounded-md border border-slate-300 bg-transparent px-3 text-sm dark:border-slate-700 ${props.className ?? ''}`} {...props} />; }
