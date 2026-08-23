import http from 'k6/http';
import { check } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

const adminUrl = (__ENV.PAPYRUS_LOAD_ADMIN_URL || 'http://127.0.0.1:5205').replace(/\/$/, '');
const apiKey = __ENV.PAPYRUS_LOAD_ADMIN_KEY || 'dev-admin-key';
const tenantId = __ENV.PAPYRUS_LOAD_TENANT_ID || 'main';
const admin429 = new Counter('admin_rate_limited');
const admin5xx = new Rate('admin_server_errors');
const responsePayloadBytes = new Trend('response_payload_bytes', true);

// The process-local fixed-window limiter allows expected 429 responses in the
// limiter probe while keeping all 5xx responses threshold failures.
http.setResponseCallback(http.expectedStatuses(200, 201, 429));

export const options = {
  scenarios: {
    admin_bounded: {
      executor: 'constant-arrival-rate',
      rate: 1,
      timeUnit: '1s',
      duration: '40s',
      preAllocatedVUs: 2,
      maxVUs: 4,
      exec: 'adminPath',
    },
    admin_limiter_probe: {
      executor: 'constant-arrival-rate',
      rate: 4,
      timeUnit: '1s',
      duration: '22s',
      startTime: '42s',
      preAllocatedVUs: 4,
      maxVUs: 8,
      exec: 'adminPath',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<1000'],
    checks: ['rate>0.99'],
    admin_server_errors: ['rate==0'],
  },
};

function headers() {
  return {
    headers: {
      'X-Admin-Api-Key': apiKey,
      'X-Tenant-Id': tenantId,
      'Content-Type': 'application/json',
    },
    tags: { workload: 'admin' },
  };
}

function runRead() {
  const response = http.get(`${adminUrl}/api/v1/blog-posts?page=1&pageSize=20`, headers());
  record(response, [200, 429], 'admin read returns HTTP 200 or expected 429');
}

function runWrite() {
  const suffix = `${Date.now()}-${__VU}-${__ITER}`;
  const body = JSON.stringify({
    slug: `k6-load-${suffix}`,
    title: `K6 load post ${suffix}`,
    summary: 'Temporary post created by the focused K6 load test.',
    contentMarkdown: '# K6 load test\n\nThis content is generated for a bounded load run.',
    coverImageUrl: null,
    projectId: null,
    categoryId: null,
    isFeatured: false,
    isPublished: false,
    publishedAt: null,
    tagIds: [],
  });
  const response = http.post(`${adminUrl}/api/v1/blog-posts`, body, headers());
  record(response, [201, 429], 'admin write returns HTTP 201 or expected 429');
}

function record(response, expectedStatuses, label) {
  responsePayloadBytes.add(response.body ? response.body.length : 0, { workload: 'admin' });
  if (response.status === 429) {
    admin429.add(1);
  }
  admin5xx.add(response.status >= 500);
  check(response, { [label]: (result) => expectedStatuses.includes(result.status) });
}

export function adminPath() {
  if ((__ITER + __VU) % 2 === 0) {
    runRead();
  } else {
    runWrite();
  }
}
