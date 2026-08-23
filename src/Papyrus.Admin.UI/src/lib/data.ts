import { authStore } from './auth-store';
import type { BlogPost, BlogPostUpsertRequest, ContentCategory, ContentTag, MediaFileResult, MediaUploadResult, PageResponse, Project, ProjectUpsertRequest } from '@/types';

const base = '/api/v1';

export class ApiError extends Error {
  readonly status: number;
  readonly details: unknown;

  constructor(status: number, details?: unknown) {
    super(`Request failed (${status})`);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

async function readErrorDetails(response: Response): Promise<unknown> {
  const text = await response.text();
  if (!text) return undefined;
  try {
    return JSON.parse(text) as unknown;
  } catch {
    return text;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  headers.set('X-Admin-Api-Key', authStore.getApiKey());
  headers.set('X-Tenant-Id', authStore.getTenantId());
  if (init?.body && !(init.body instanceof FormData)) headers.set('Content-Type', 'application/json');
  const response = await fetch(`${base}${path}`, { ...init, headers });
  if ([401, 403, 429].includes(response.status)) {
    window.dispatchEvent(new CustomEvent('papyrus:api-error', { detail: { status: response.status } }));
  }
  if (!response.ok) throw new ApiError(response.status, await readErrorDetails(response));
  if (response.status === 204) return undefined as T;
  const text = await response.text();
  if (!text) return undefined as T;
  return JSON.parse(text) as T;
}
const query = (params: Record<string, string | number | undefined>) => { const s = new URLSearchParams(); Object.entries(params).forEach(([k, v]) => v !== undefined && s.set(k, String(v))); return `?${s}`; };
export const dataApi = {
  posts: (page = 1, search = '') => request<PageResponse<BlogPost>>(`/blog-posts${query({ page, pageSize: 10, search })}`),
  projects: (page = 1, search = '') => request<PageResponse<Project>>(`/projects${query({ page, pageSize: 10, search })}`),
  categories: () => request<PageResponse<ContentCategory>>('/categories?page=1&pageSize=100'),
  tags: () => request<PageResponse<ContentTag>>('/tags?page=1&pageSize=100'),
  media: () => request<MediaFileResult[]>('/media'),
  savePost: (body: BlogPostUpsertRequest, id?: string) => request<BlogPost>(id ? `/blog-posts/${id}` : '/blog-posts', { method: id ? 'PUT' : 'POST', body: JSON.stringify(body) }),
  deletePost: (id: string) => request<void>(`/blog-posts/${id}`, { method: 'DELETE' }),
  saveProject: (body: ProjectUpsertRequest, id?: string) => request<Project>(id ? `/projects/${id}` : '/projects', { method: id ? 'PUT' : 'POST', body: JSON.stringify(body) }),
  deleteProject: (id: string) => request<void>(`/projects/${id}`, { method: 'DELETE' }),
  uploadMedia: (file: File) => { const form = new FormData(); form.append('file', file); return request<MediaUploadResult>('/media', { method: 'POST', body: form }); },
  deleteMedia: (name: string) => request<void>(`/media/${encodeURIComponent(name)}`, { method: 'DELETE' }),
};
