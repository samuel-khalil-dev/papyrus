import { useState } from 'react';
import { FolderKanban, Plus, Search, Trash2 } from 'lucide-react';
import { z } from 'zod';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useDebounce } from '@/hooks/useDebounce';
import { useDataMutations, useProjects, useTaxonomy } from '@/hooks/useData';
import { Pagination } from '@/components/shared/pagination';
import { StatusBadge } from '@/components/shared/status-badge';
import { ErrorState, LoadingState } from '@/components/shared/feedback';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { toast } from '@/components/ui/toast';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import type { Project } from '@/types';

const schema = z.object({
  title: z.string().min(2), slug: z.string().regex(/^[a-z0-9-]+$/), summary: z.string().min(10),
  description: z.string(), repositoryUrl: z.string().url().or(z.literal('')), liveUrl: z.string().url().or(z.literal('')),
  coverImageUrl: z.string().url().or(z.literal('')), categoryId: z.string(), tagIds: z.array(z.string()), isPublished: z.boolean(),
});
type ProjectForm = z.infer<typeof schema>;
const blank: ProjectForm = { title: '', slug: '', summary: '', description: '', repositoryUrl: '', liveUrl: '', coverImageUrl: '', categoryId: '', tagIds: [], isPublished: false };

export function ProjectsPage() {
  const [page, setPage] = useState(1); const [search, setSearch] = useState(''); const [editing, setEditing] = useState<string | null>(null); const [deleting, setDeleting] = useState<Project | null>(null);
  const debounced = useDebounce(search, 300); const query = useProjects(page, debounced); const taxonomy = useTaxonomy(); const mutations = useDataMutations();
  const { register, handleSubmit, reset, formState: { errors } } = useForm<ProjectForm>({ resolver: zodResolver(schema), defaultValues: blank });
  const edit = (project: Project) => { setEditing(project.id); reset({ title: project.title, slug: project.slug, summary: project.summary, description: project.description ?? '', repositoryUrl: project.repositoryUrl ?? '', liveUrl: project.liveUrl ?? '', coverImageUrl: project.coverImageUrl ?? '', categoryId: project.category?.id ?? '', tagIds: project.tags.map(tag => tag.id), isPublished: project.isPublished }); };
  const submit = (value: ProjectForm) => mutations.saveProject.mutate({ body: { ...value, description: value.description || null, repositoryUrl: value.repositoryUrl || null, liveUrl: value.liveUrl || null, coverImageUrl: value.coverImageUrl || null, categoryId: value.categoryId || null, isFeatured: false, publishedAt: value.isPublished ? new Date().toISOString() : null }, id: editing === 'new' ? undefined : editing ?? undefined }, { onSuccess: () => { setEditing(null); reset(blank); toast.success('Project saved.'); }, onError: () => toast.error('Unable to save the project.') });
  const remove = () => { if (!deleting) return; mutations.deleteProject.mutate(deleting.id, { onSuccess: () => { setDeleting(null); toast.success('Project deleted.'); }, onError: () => toast.error('Unable to delete the project.') }); };
  if (query.isLoading) return <LoadingState />; if (query.isError) return <ErrorState />;
  return <div><div className="mb-6 flex flex-col justify-between gap-4 sm:flex-row sm:items-end"><div><h1 className="text-3xl font-black">Projects</h1><p className="mt-1 text-sm text-slate-500">Show the work behind the work.</p></div><Button onClick={() => { setEditing('new'); reset(blank); }}><Plus size={16} className="mr-2" />New project</Button></div>
    {editing && <form onSubmit={handleSubmit(submit)} className="mb-6 grid gap-4 rounded-2xl border bg-white p-5 shadow-sm dark:bg-slate-900 sm:grid-cols-2"><h2 className="text-lg font-bold sm:col-span-2">{editing === 'new' ? 'Create project' : 'Edit project'}</h2>
      {(['title', 'slug', 'summary', 'repositoryUrl', 'liveUrl', 'coverImageUrl'] as const).map(name => <label key={name} className="text-sm font-medium">{name === 'coverImageUrl' ? 'Cover image URL' : name}<Input {...register(name)} />{errors[name] && <small className="text-red-600">Invalid value</small>}</label>)}
      <label className="text-sm font-medium sm:col-span-2">Description<Textarea {...register('description')} rows={5} /></label>
      <label className="text-sm font-medium">Category<Select {...register('categoryId')}><option value="">No category</option>{taxonomy.categories.data?.items.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}</Select></label>
      <label className="text-sm font-medium">Tags<Select multiple {...register('tagIds')} className="h-24">{taxonomy.tags.data?.items.map(tag => <option key={tag.id} value={tag.id}>{tag.name}</option>)}</Select><small className="text-slate-500">Use Ctrl/Cmd to select multiple tags.</small></label>
      <label className="flex items-center gap-2 text-sm"><input type="checkbox" {...register('isPublished')} />Published</label><div className="flex gap-2 sm:col-span-2"><Button type="submit" disabled={mutations.saveProject.isPending}>Save project</Button><Button type="button" className="bg-slate-200 text-slate-900" onClick={() => { setEditing(null); reset(blank); }}>Cancel</Button></div>
    </form>}
    <div className="mb-4 flex items-center gap-2"><Search size={16} /><Input value={search} onChange={event => { setSearch(event.target.value); setPage(1); }} placeholder="Search projects" /></div>
    <div className="space-y-3">{query.data?.items.map(project => <div key={project.id} className="flex flex-col gap-3 rounded-xl border p-4 sm:flex-row sm:items-center sm:justify-between"><div><div className="flex items-center gap-2"><FolderKanban size={16} /><button className="font-semibold underline-offset-2 hover:underline" onClick={() => edit(project)}>{project.title}</button><StatusBadge published={project.isPublished} /></div><p className="text-sm text-slate-500">{project.summary}</p><p className="text-xs text-slate-400">{project.category?.name ?? 'No category'} · {project.tags.map(tag => tag.name).join(', ') || 'No tags'}</p></div><div className="flex gap-2"><Button className="bg-slate-200 text-slate-900" onClick={() => edit(project)}>Edit</Button><Button aria-label={`Delete ${project.title}`} className="bg-red-600" onClick={() => setDeleting(project)}><Trash2 size={15} /></Button></div></div>)}</div>
    <Pagination page={query.data?.page ?? page} totalPages={query.data?.totalPages ?? 1} onChange={setPage} />
    <Dialog open={deleting !== null}><DialogContent><DialogHeader><DialogTitle>Delete project?</DialogTitle></DialogHeader><p className="text-sm text-slate-600">This permanently removes “{deleting?.title}”. This action cannot be undone.</p><div className="mt-5 flex justify-end gap-2"><Button className="bg-slate-200 text-slate-900" onClick={() => setDeleting(null)}>Cancel</Button><Button className="bg-red-600" onClick={remove} disabled={mutations.deleteProject.isPending}>Delete project</Button></div></DialogContent></Dialog>
  </div>;
}
