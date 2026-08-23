import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { dataApi } from '@/lib/data';
import type { BlogPostUpsertRequest, ProjectUpsertRequest } from '@/types';
export const usePosts = (page: number, search: string) => useQuery({ queryKey: ['posts', page, search], queryFn: () => dataApi.posts(page, search) });
export const useProjects = (page: number, search: string) => useQuery({ queryKey: ['projects', page, search], queryFn: () => dataApi.projects(page, search) });
export const useProjectOptions = () => useQuery({ queryKey: ['projects', 'options'], queryFn: () => dataApi.projects(1, '') });
export const useTaxonomy = () => ({ categories: useQuery({ queryKey: ['categories'], queryFn: dataApi.categories }), tags: useQuery({ queryKey: ['tags'], queryFn: dataApi.tags }) });
export const useMedia = () => useQuery({ queryKey: ['media'], queryFn: dataApi.media });
export function useDataMutations() { const client = useQueryClient(); const refresh = (key: string) => client.invalidateQueries({ queryKey: [key] }); return {
  savePost: useMutation({ mutationFn: ({ body, id }: { body: BlogPostUpsertRequest; id?: string }) => dataApi.savePost(body, id), onSuccess: () => refresh('posts') }),
  deletePost: useMutation({ mutationFn: dataApi.deletePost, onSuccess: () => refresh('posts') }),
  saveProject: useMutation({ mutationFn: ({ body, id }: { body: ProjectUpsertRequest; id?: string }) => dataApi.saveProject(body, id), onSuccess: () => refresh('projects') }),
  deleteProject: useMutation({ mutationFn: dataApi.deleteProject, onSuccess: () => refresh('projects') }),
  uploadMedia: useMutation({ mutationFn: dataApi.uploadMedia, onSuccess: () => refresh('media') }),
  deleteMedia: useMutation({ mutationFn: dataApi.deleteMedia, onSuccess: () => refresh('media') }),
}; }
