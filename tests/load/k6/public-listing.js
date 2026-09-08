import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';

const blogUrl = (__ENV.PAPYRUS_LOAD_BLOG_URL || 'http://127.0.0.1:5075').replace(/\/$/, '');
const portfolioUrl = (__ENV.PAPYRUS_LOAD_PORTFOLIO_URL || 'http://127.0.0.1:5024').replace(/\/$/, '');
const responsePayloadBytes = new Trend('response_payload_bytes', true);

export const options = {
  scenarios: {
    public_feed: {
      executor: 'constant-vus',
      vus: 4,
      duration: '20s',
      exec: 'feed',
    },
    public_search: {
      executor: 'constant-vus',
      vus: 4,
      duration: '20s',
      startTime: '22s',
      exec: 'search',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<2000'],
    checks: ['rate>0.99'],
  },
};

function getAndCheck(url, workload) {
  const response = http.get(url, { tags: { workload } });
  responsePayloadBytes.add(response.body ? response.body.length : 0, { workload });
  check(response, {
    [`${workload} returns HTTP 200`]: (result) => result.status === 200,
  });
  sleep(0.1);
}

export function feed() {
  const target = __ITER % 2 === 0 ? blogUrl : portfolioUrl;
  getAndCheck(`${target}/?page=${(__ITER % 3) + 1}`, 'public-feed');
}

export function search() {
  const target = __ITER % 2 === 0 ? blogUrl : portfolioUrl;
  getAndCheck(`${target}/?q=papyrus&page=${(__ITER % 3) + 1}`, 'public-search');
}
