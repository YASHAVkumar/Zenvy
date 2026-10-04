import http from 'k6/http';
import { check, sleep } from 'k6';

const baseUrl = __ENV.BASE_URL || 'http://localhost:5000';
const token = __ENV.ACCESS_TOKEN;

export const options = {
  stages: [
    { duration: '30s', target: Number(__ENV.VUS || 10) },
    { duration: '1m', target: Number(__ENV.VUS || 10) },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<750'],
  },
};

export default function () {
  const root = http.get(`${baseUrl}/`);
  check(root, { 'root responds 200': (response) => response.status === 200 });

  if (token) {
    const categories = http.get(`${baseUrl}/api/v1/products`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    check(categories, {
      'categories responds 200': (response) => response.status === 200,
      'categories response is an API envelope': (response) => response.json('success') === true,
    });
  }

  sleep(1);
}
