import { useState } from 'react';
import { Trash2, UploadCloud } from 'lucide-react';
import { useDataMutations, useMedia } from '@/hooks/useData';
import { RasterDropzone } from '@/components/shared/raster-dropzone';
import { ErrorState, LoadingState } from '@/components/shared/feedback';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { toast } from '@/components/ui/toast';
import { ApiError } from '@/lib/data';
import type { MediaFileResult } from '@/types';

export function MediaPage() {
  const query = useMedia(); const mutations = useDataMutations(); const [deleting, setDeleting] = useState<MediaFileResult | null>(null);
  const remove = () => { if (!deleting) return; mutations.deleteMedia.mutate(deleting.fileName, { onSuccess: () => { setDeleting(null); toast.success('Media deleted.'); }, onError: error => { if (error instanceof ApiError && error.status === 409) toast.error('This asset is referenced by content and cannot be deleted.'); else toast.error('Unable to delete this asset.'); } }); };
  if (query.isLoading) return <LoadingState />; if (query.isError) return <ErrorState />;
  return <div><div className="mb-6"><h1 className="text-3xl font-black">Media library</h1><p className="mt-1 text-sm text-slate-500">Raster assets, ready for publication.</p></div><div className="mb-8"><RasterDropzone onFile={file => mutations.uploadMedia.mutate(file, { onSuccess: () => toast.success('Media uploaded.'), onError: () => toast.error('Unable to upload this asset.') })} />{mutations.uploadMedia.isPending && <p className="mt-2 text-sm text-slate-500">Uploading…</p>}</div><div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">{query.data?.map(file => <Card key={file.fileName} className="overflow-hidden"><div className="aspect-video bg-slate-100 dark:bg-slate-800"><img src={file.url} alt={file.fileName} className="h-full w-full object-cover" /></div><div className="p-4"><p className="truncate text-sm font-semibold" title={file.fileName}>{file.fileName}</p><p className="mt-1 text-xs text-slate-500">{(file.size / 1024).toFixed(1)} KB · {new Date(file.lastModified).toLocaleDateString()}</p><Button aria-label={`Delete ${file.fileName}`} onClick={() => setDeleting(file)} className="mt-4 bg-red-600"><Trash2 size={14} className="mr-2" />Delete asset</Button></div></Card>)}{query.data?.length === 0 && <div className="col-span-full rounded-2xl border border-dashed p-12 text-center text-sm text-slate-500"><UploadCloud className="mx-auto mb-2" />No media uploaded yet.</div>}</div>
    <Dialog open={deleting !== null}><DialogContent><DialogHeader><DialogTitle>Delete media asset?</DialogTitle></DialogHeader><p className="text-sm text-slate-600">Delete “{deleting?.fileName}”? The API checks references and will refuse deletion if this asset is used by a project or post.</p><div className="mt-5 flex justify-end gap-2"><Button className="bg-slate-200 text-slate-900" onClick={() => setDeleting(null)}>Cancel</Button><Button className="bg-red-600" onClick={remove} disabled={mutations.deleteMedia.isPending}>Delete asset</Button></div></DialogContent></Dialog>
  </div>;
}
