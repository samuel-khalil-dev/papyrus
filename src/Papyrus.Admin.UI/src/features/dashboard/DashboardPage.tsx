import {
  Pulse as Activity,
  FileText,
  Folder as FolderKanban,
  Image,
  TrendUp as TrendingUp,
} from "@phosphor-icons/react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useMedia, usePosts, useProjects } from "@/hooks/useData";
import { LoadingState } from "@/components/shared/feedback";
export function DashboardPage() {
  const posts = usePosts(1, "");
  const projects = useProjects(1, "");
  const media = useMedia();
  if (posts.isLoading || projects.isLoading || media.isLoading)
    return <LoadingState />;
  const postItems = posts.data?.items ?? [];
  const projectItems = projects.data?.items ?? [];
  const published = postItems.filter((p) => p.isPublished).length;
  const ratio = postItems.length
    ? Math.round((published / postItems.length) * 100)
    : 0;
  const metrics = [
    {
      label: "Blog posts",
      value: posts.data?.total ?? 0,
      icon: FileText,
      color: "text-primary",
    },
    {
      label: "Projects",
      value: projects.data?.total ?? 0,
      icon: FolderKanban,
      color: "text-primary",
    },
    {
      label: "Media assets",
      value: media.data?.length ?? 0,
      icon: Image,
      color: "text-secondary",
    },
    {
      label: "Published ratio",
      value: `${ratio}%`,
      icon: TrendingUp,
      color: "text-primary",
    },
  ];
  return (
    <div>
      <div className="mb-8 flex flex-col justify-between gap-3 sm:flex-row sm:items-end">
        <div>
          <p className="text-sm font-semibold text-primary">
            Good to see you
          </p>
          <h1 className="mt-1 font-display text-3xl font-bold tracking-tight sm:text-4xl">
            Your content, in focus.
          </h1>
        </div>
        <p className="text-sm text-muted-foreground">
          Tenant:{" "}
          <strong className="text-foreground">main</strong>
        </p>
      </div>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {metrics.map((metric) => {
          const Icon = metric.icon;
          return (
            <Card
              key={metric.label}
              className="reveal"
            >
              <CardContent className="flex items-center justify-between p-5">
                <div>
                  <p className="text-sm text-muted-foreground">{metric.label}</p>
                  <p className="mt-2 text-3xl font-bold">{metric.value}</p>
                </div>
                <Icon className={metric.color} size={26} />
              </CardContent>
            </Card>
          );
        })}
      </div>
      <div className="mt-6 grid gap-6 lg:grid-cols-5">
        <Card className="lg:col-span-3">
          <CardHeader>
            <CardTitle className="text-lg">Recent posts</CardTitle>
          </CardHeader>
          <CardContent className="space-y-1">
            {postItems.slice(0, 5).map((post) => (
              <div
                key={post.id}
                className="flex items-center justify-between rounded-none px-3 py-3 hover:bg-muted"
              >
                <div>
                  <p className="font-medium">{post.title}</p>
                  <p className="text-xs text-muted-foreground">
                    {post.publishedAt
                      ? new Date(post.publishedAt).toLocaleDateString()
                      : "Unpublished"}
                  </p>
                </div>
                <span
                  className={`h-2 w-2 ${post.isPublished ? "bg-primary" : "bg-secondary"}`}
                />
              </div>
            ))}
          </CardContent>
        </Card>
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              <Activity size={18} className="text-primary" />
              Latest activity
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4 text-sm text-muted-foreground">
            <p>
              <strong className="text-foreground">
                {projectItems.length}
              </strong>{" "}
              projects on this page
            </p>
            <p>
              <strong className="text-foreground">
                {published}
              </strong>{" "}
              posts published
            </p>
            <p>Media library is ready for your next story.</p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
