import React from 'react';
import { AdminLayout } from './components/layout/AdminLayout';
import type { View } from './components/layout/Sidebar';
import { DashboardPage } from './features/dashboard/DashboardPage';
import { PostsPage } from './features/posts/PostsPage';
import { ProjectsPage } from './features/projects/ProjectsPage';
import { MediaPage } from './features/media/MediaPage';

const views: View[] = ['dashboard', 'posts', 'projects', 'media'];

function readView(): View {
  const value = window.location.hash.slice(1);
  return views.includes(value as View) ? value as View : 'dashboard';
}

export function App() {
  const [view, setView] = React.useState<View>(readView);
  React.useEffect(() => {
    const onHash = () => setView(readView());
    window.addEventListener('hashchange', onHash);
    return () => window.removeEventListener('hashchange', onHash);
  }, []);
  const navigate = (next: View) => { window.location.hash = next; };
  const page = view === 'dashboard' ? <DashboardPage /> : view === 'posts' ? <PostsPage /> : view === 'projects' ? <ProjectsPage /> : <MediaPage />;
  return <AdminLayout view={view} onNavigate={navigate}>{page}</AdminLayout>;
}
