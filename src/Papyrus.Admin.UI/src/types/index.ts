export interface PageResponse<T> { items: T[]; page: number; pageSize: number; total: number; totalPages: number; }
export interface ContentCategory { id: string; slug: string; name: string; description: string | null; isPublished: boolean; createdAt: string; updatedAt: string | null; }
export interface ContentTag { id: string; slug: string; name: string; createdAt: string; updatedAt: string | null; }
export interface ProjectSummary { id: string; slug: string; title: string; summary: string; coverImageUrl: string | null; isFeatured: boolean; isPublished: boolean; publishedAt: string | null; }
export interface BlogPostSummary { id: string; slug: string; title: string; summary: string; isFeatured: boolean; isPublished: boolean; publishedAt: string | null; }
export interface Project { id: string; slug: string; title: string; summary: string; description: string | null; repositoryUrl: string | null; liveUrl: string | null; coverImageUrl: string | null; isFeatured: boolean; isPublished: boolean; publishedAt: string | null; createdAt: string; updatedAt: string | null; category: ContentCategory | null; tags: ContentTag[]; blogPosts: BlogPostSummary[]; }
export interface BlogPost { id: string; slug: string; title: string; summary: string; contentMarkdown: string; coverImageUrl: string | null; isFeatured: boolean; isPublished: boolean; publishedAt: string | null; createdAt: string; updatedAt: string | null; category: ContentCategory | null; project: ProjectSummary | null; tags: ContentTag[]; }
export interface MediaFileResult { fileName: string; url: string; size: number; lastModified: string; }
export interface MediaUploadResult { url: string; fileName: string; contentType: string; size: number; }
export interface CategoryUpsertRequest { slug: string; name: string; description: string | null; isPublished: boolean; }
export interface TagUpsertRequest { slug: string; name: string; }
export interface ProjectUpsertRequest { slug: string; title: string; summary: string; description: string | null; repositoryUrl: string | null; liveUrl: string | null; coverImageUrl: string | null; categoryId: string | null; isFeatured: boolean; isPublished: boolean; publishedAt: string | null; tagIds: string[]; }
export interface BlogPostUpsertRequest { slug: string; title: string; summary: string; contentMarkdown: string; coverImageUrl: string | null; projectId: string | null; categoryId: string | null; isFeatured: boolean; isPublished: boolean; publishedAt: string | null; tagIds: string[]; }
