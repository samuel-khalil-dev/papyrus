import type { TableHTMLAttributes, HTMLAttributes } from 'react';
export function Table(props: TableHTMLAttributes<HTMLTableElement>) { return <div className="w-full overflow-auto"><table className="w-full caption-bottom text-sm" {...props} /></div>; }
export function TableHeader(props: HTMLAttributes<HTMLTableSectionElement>) { return <thead className="border-b" {...props} />; }
export function TableBody(props: HTMLAttributes<HTMLTableSectionElement>) { return <tbody {...props} />; }
export function TableRow(props: HTMLAttributes<HTMLTableRowElement>) { return <tr className="border-b transition-colors hover:bg-slate-50 dark:hover:bg-slate-900" {...props} />; }
export function TableHead(props: HTMLAttributes<HTMLTableCellElement>) { return <th className="h-12 px-4 text-left align-middle font-medium" {...props} />; }
export function TableCell(props: HTMLAttributes<HTMLTableCellElement>) { return <td className="p-4 align-middle" {...props} />; }
