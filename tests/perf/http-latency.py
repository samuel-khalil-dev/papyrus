import statistics
import time
import urllib.request

queries = [
    ("blog-index", "http://localhost:5075/"),
    ("blog-detail", "http://localhost:5075/post/building-papyrus-platform-foundation"),
    ("portfolio-index", "http://localhost:5024/"),
    ("portfolio-detail", "http://localhost:5024/project/platform-foundation"),
    ("admin-categories", "http://localhost:5205/api/v1/categories?page=1&pageSize=10"),
    ("admin-tags", "http://localhost:5205/api/v1/tags?page=1&pageSize=10"),
    ("admin-projects", "http://localhost:5205/api/v1/projects?page=1&pageSize=10"),
    ("admin-blog-posts", "http://localhost:5205/api/v1/blog-posts?page=1&pageSize=10"),
]


def request(name, url):
    headers = {"X-Admin-Api-Key": "dev-admin-key"} if name.startswith("admin-") else {}
    started = time.perf_counter()
    with urllib.request.urlopen(urllib.request.Request(url, headers=headers), timeout=10) as response:
        response.read()
        return response.status, (time.perf_counter() - started) * 1000


print("query,cold_ms,p50_ms,p95_ms,status")
for name, url in queries:
    status, cold = request(name, url)
    warm = [request(name, url)[1] for _ in range(30)]
    print(f"{name},{cold:.3f},{statistics.median(warm):.3f},{sorted(warm)[int(len(warm) * .95) - 1]:.3f},{status}")
