import type { View } from './Sidebar';
export function Breadcrumbs({ view }: { view: View }) { return <div className="mb-6 text-sm text-slate-500"><span>Admin</span><span className="mx-2">/</span><span className="font-medium capitalize text-slate-900 dark:text-slate-100">{view}</span></div>; }
