import { useState } from "react";
import { FileText, MagnifyingGlass as Search, Plus, Trash as Trash2 } from "@phosphor-icons/react";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useDebounce } from "@/hooks/useDebounce";
import {
  useDataMutations,
  usePosts,
  useProjectOptions,
  useTaxonomy,
} from "@/hooks/useData";
import { Pagination } from "@/components/shared/pagination";
import { StatusBadge } from "@/components/shared/status-badge";
import { ErrorState, LoadingState } from "@/components/shared/feedback";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { toast } from "@/components/ui/toast";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import type { BlogPost } from "@/types";

const schema = z.object({
  title: z.string().min(2),
  slug: z.string().regex(/^[a-z0-9-]+$/),
  summary: z.string().min(10),
  contentMarkdown: z.string().min(1),
  projectId: z.string(),
  categoryId: z.string(),
  tagIds: z.array(z.string()),
  isPublished: z.boolean(),
});
type PostForm = z.infer<typeof schema>;
const blank: PostForm = {
  title: "",
  slug: "",
  summary: "",
  contentMarkdown: "",
  projectId: "",
  categoryId: "",
  tagIds: [],
  isPublished: false,
};

export function PostsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [editing, setEditing] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<BlogPost | null>(null);
  const debounced = useDebounce(search, 300);
  const query = usePosts(page, debounced);
  const taxonomy = useTaxonomy();
  const projects = useProjectOptions();
  const mutations = useDataMutations();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PostForm>({
    resolver: zodResolver(schema),
    defaultValues: blank,
  });
  const edit = (post: BlogPost) => {
    setEditing(post.id);
    reset({
      title: post.title,
      slug: post.slug,
      summary: post.summary,
      contentMarkdown: post.contentMarkdown,
      projectId: post.project?.id ?? "",
      categoryId: post.category?.id ?? "",
      tagIds: post.tags.map((tag) => tag.id),
      isPublished: post.isPublished,
    });
  };
  const submit = (value: PostForm) =>
    mutations.savePost.mutate(
      {
        body: {
          ...value,
          coverImageUrl: null,
          projectId: value.projectId || null,
          categoryId: value.categoryId || null,
          isFeatured: false,
          publishedAt: value.isPublished ? new Date().toISOString() : null,
        },
        id: editing === "new" ? undefined : (editing ?? undefined),
      },
      {
        onSuccess: () => {
          setEditing(null);
          reset(blank);
          toast.success("Post saved.");
        },
        onError: () => toast.error("Unable to save the post."),
      },
    );
  const remove = () => {
    if (!deleting) return;
    mutations.deletePost.mutate(deleting.id, {
      onSuccess: () => {
        setDeleting(null);
        toast.success("Post deleted.");
      },
      onError: () => toast.error("Unable to delete the post."),
    });
  };
  if (query.isLoading) return <LoadingState />;
  if (query.isError) return <ErrorState />;
  return (
    <div>
      <div className="mb-6 flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <h1 className="font-display text-3xl font-bold">Blog posts</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Shape the ideas your audience comes back for.
          </p>
        </div>
        <Button
          onClick={() => {
            setEditing("new");
            reset(blank);
          }}
        >
          <Plus size={16} className="mr-2" />
          New post
        </Button>
      </div>
      {editing && (
        <form
          onSubmit={handleSubmit(submit)}
          className="mb-6 grid gap-4 rounded-none border border-border bg-card p-5 shadow-sm sm:grid-cols-2"
        >
          <h2 className="text-lg font-bold sm:col-span-2">
            {editing === "new" ? "Create post" : "Edit post"}
          </h2>
          {(["title", "slug", "summary"] as const).map((name) => (
            <label key={name} className="text-sm font-medium">
              {name}
              <Input {...register(name)} />
              {errors[name] && (
                <small className="text-red-600">Invalid value</small>
              )}
            </label>
          ))}
          <label className="text-sm font-medium sm:col-span-2">
            Markdown content
            <Textarea
              {...register("contentMarkdown")}
              rows={14}
              placeholder="# Heading\n\nWrite the post in Markdown…"
              className="font-mono"
            />
            {errors.contentMarkdown && (
              <small className="text-red-600">
                Markdown content is required
              </small>
            )}
            <small className="text-muted-foreground">
              Markdown is stored as source text and remains editable/exportable.
            </small>
          </label>
          <label className="text-sm font-medium">
            Project
            <Select {...register("projectId")}>
              <option value="">No project</option>
              {projects.data?.items.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.title}
                </option>
              ))}
            </Select>
          </label>
          <label className="text-sm font-medium">
            Category
            <Select {...register("categoryId")}>
              <option value="">No category</option>
              {taxonomy.categories.data?.items.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </Select>
          </label>
          <label className="text-sm font-medium sm:col-span-2">
            Tags
            <Select multiple {...register("tagIds")} className="h-24">
              {taxonomy.tags.data?.items.map((tag) => (
                <option key={tag.id} value={tag.id}>
                  {tag.name}
                </option>
              ))}
            </Select>
            <small className="text-muted-foreground">
              Use Ctrl/Cmd to select multiple tags.
            </small>
          </label>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" {...register("isPublished")} />
            Published
          </label>
          <div className="flex gap-2 sm:col-span-2">
            <Button type="submit" disabled={mutations.savePost.isPending}>
              Save post
            </Button>
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                setEditing(null);
                reset(blank);
              }}
            >
              Cancel
            </Button>
          </div>
        </form>
      )}
      <div className="mb-4 flex items-center gap-2">
        <Search size={16} />
        <Input
          value={search}
          onChange={(event) => {
            setSearch(event.target.value);
            setPage(1);
          }}
          placeholder="Search posts"
        />
      </div>
      <div className="space-y-3">
        {query.data?.items.map((post) => (
          <div
            key={post.id}
            className="flex flex-col gap-3 rounded-none border border-border bg-card p-4 shadow-sm transition-[border-color,box-shadow,transform] duration-250 hover:-translate-y-0.5 hover:border-primary/60 hover:shadow-md sm:flex-row sm:items-center sm:justify-between"
          >
            <div>
              <div className="flex items-center gap-2">
                <FileText size={16} />
                <button
                  className="font-semibold underline-offset-2 hover:underline"
                  onClick={() => edit(post)}
                >
                  {post.title}
                </button>
                <StatusBadge published={post.isPublished} />
              </div>
              <p className="text-sm text-muted-foreground">{post.summary}</p>
              <p className="text-xs text-muted-foreground">
                {post.category?.name ?? "No category"} ·{" "}
                {post.project?.title ?? "No project"} ·{" "}
                {post.tags.map((tag) => tag.name).join(", ") || "No tags"}
              </p>
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                onClick={() => edit(post)}
              >
                Edit
              </Button>
              <Button
                aria-label={`Delete ${post.title}`}
                variant="destructive"
                onClick={() => setDeleting(post)}
              >
                <Trash2 size={15} />
              </Button>
            </div>
          </div>
        ))}
      </div>
      <Pagination
        page={query.data?.page ?? page}
        totalPages={query.data?.totalPages ?? 1}
        onChange={setPage}
      />
      <Dialog open={deleting !== null}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete post?</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            This permanently removes “{deleting?.title}”. This action cannot be
            undone.
          </p>
          <div className="mt-5 flex justify-end gap-2">
            <Button
              variant="outline"
              onClick={() => setDeleting(null)}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={remove}
              disabled={mutations.deletePost.isPending}
            >
              Delete post
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
