import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

// ============================================================================
// K6 PERFORMANCE TEST SCRIPT — BTH11: KIỂM CHỨNG HIỆU SUẤT KIẾN TRÚC PHẦN MỀM
// Hệ thống: InsuranceAgency Microservices (.NET 8)
// Công cụ: Grafana k6
// ============================================================================

// --- Test Run ID (mã định danh lần kiểm thử) ---
// Sinh tự động từ timestamp, gắn kèm mọi request để đối chiếu log backend.
export const TEST_RUN_ID = `__TEST_RUN_ID_${Date.now()}__`;

// Cấu hình target host (mặc định chạy qua API Gateway Ocelot port 8099).
// Có thể ghi đè bằng ENV: export BASE_URL=http://localhost:8099
// Hoặc test trực tiếp service: export BASE_URL=http://localhost:5050
const BASE_URL = __ENV.BASE_URL || 'http://localhost:8099';

// ============================================================================
// Custom metrics (được xuất trong JSON summary)
// ============================================================================
const createQuoteP95 = new Trend('create_quote_p95');
const convertPolicyP95 = new Trend('convert_policy_p95');
const searchP95 = new Trend('search_p95');

const createQuoteErrorRate = new Rate('create_quote_error_rate');
const convertPolicyErrorRate = new Rate('convert_policy_error_rate');
const searchErrorRate = new Rate('search_error_rate');

const createQuoteReqs = new Counter('create_quote_requests');
const convertPolicyReqs = new Counter('convert_policy_requests');
const searchReqs = new Counter('search_requests');

// ============================================================================
// Test data (dữ liệu mẫu, không nhạy cảm)
// ============================================================================
const QUOTE_PAYLOAD = JSON.stringify({
  productId: 'SAFE_TRAVELLER_TRI',
  policyFrom: '2027-05-01',
  policyTo: '2027-05-15',
  destination: 'World',
  numberOfAdults: 2,
  numberOfChildren: 2,
});

const POLICY_PAYLOAD = JSON.stringify({
  quoteId: 'QT-2027-0001',
  policyHolder: {
    firstName: 'Nhat',
    lastName: 'Anh Le',
    taxId: '2324801030042',
    country: 'Poland',
    zip: '31-002',
    city: 'Krakow',
    street: 'Rynek Glowny 12',
  },
});

const AGENT_LOGIN = 'jimmy.solid';

// ============================================================================
// Options — các scenario chạy riêng theo mức tải (VUs)
// Chạy ví dụ: k6 run --env VUS=10 --env DURATION=30s k6-performance-test.js
// ============================================================================
export const options = {
  scenarios: {
    // Chức năng 1: Tạo đề nghị & tính phí
    create_quote: {
      executor: 'constant-vus',
      vus: parseInt(__ENV.VUS) || 1,
      duration: __ENV.DURATION || '30s',
      exec: 'createQuoteAndCalculatePremium',
      tags: { test_type: 'create_quote' },
    },
    // Chức năng 2: Chuyển đề nghị thành hợp đồng
    convert_policy: {
      executor: 'constant-vus',
      vus: parseInt(__ENV.VUS) || 1,
      duration: __ENV.DURATION || '30s',
      exec: 'convertQuoteToPolicy',
      tags: { test_type: 'convert_policy' },
    },
    // Chức năng 3: Tìm kiếm đề nghị/hợp đồng
    search: {
      executor: 'constant-vus',
      vus: parseInt(__ENV.VUS) || 1,
      duration: __ENV.DURATION || '30s',
      exec: 'searchQuotesAndPolicies',
      tags: { test_type: 'search' },
    },
  },
  thresholds: {
    // Ngưỡng tham chiếu (đề bài: p95 ≤ 2s/3s, Error Rate < 1%)
    'create_quote_p95': ['p(95) <= 2000'],
    'create_quote_error_rate': ['rate < 0.01'],
    'convert_policy_p95': ['p(95) <= 3000'],
    'convert_policy_error_rate': ['rate < 0.01'],
    'search_p95': ['p(95) <= 2000'],
    'search_error_rate': ['rate < 0.01'],
  },
};

// ============================================================================
// Chức năng 1 — Tạo đề nghị bảo hiểm và tính phí (Create Quote & Calculate Premium)
// ============================================================================
export function createQuoteAndCalculatePremium() {
  const url = `${BASE_URL}/api/Offer`;

  const res = http.post(url, QUOTE_PAYLOAD, {
    headers: {
      'Content-Type': 'application/json',
      'AgentLogin': AGENT_LOGIN,
      'X-Test-Run-Id': TEST_RUN_ID,
    },
    tags: { name: 'create_quote', test_run_id: TEST_RUN_ID },
  });

  const success = check(res, {
    'create_quote status is 200/201': (r) => r.status === 200 || r.status === 201,
    'create_quote has body': (r) => r.body && r.body.length > 0,
  });

  createQuoteP95.add(res.timings.duration);
  createQuoteErrorRate.add(!success);
  createQuoteReqs.add(1);

  sleep(1);
}

// ============================================================================
// Chức năng 2 — Chuyển đề nghị thành hợp đồng (Convert Quote to Policy)
// ============================================================================
export function convertQuoteToPolicy() {
  const url = `${BASE_URL}/api/Policy`;

  const res = http.post(url, POLICY_PAYLOAD, {
    headers: {
      'Content-Type': 'application/json',
      'AgentLogin': AGENT_LOGIN,
      'X-Test-Run-Id': TEST_RUN_ID,
    },
    tags: { name: 'convert_policy', test_run_id: TEST_RUN_ID },
  });

  const success = check(res, {
    'convert_policy status is 200/201': (r) => r.status === 200 || r.status === 201,
    'convert_policy has body': (r) => r.body && r.body.length > 0,
  });

  convertPolicyP95.add(res.timings.duration);
  convertPolicyErrorRate.add(!success);
  convertPolicyReqs.add(1);

  sleep(1);
}

// ============================================================================
// Chức năng 3 — Tìm kiếm đề nghị/hợp đồng (Search Quotes & Policies)
// ============================================================================
export function searchQuotesAndPolicies() {
  const url = `${BASE_URL}/api/PolicySearch?q=Nhat`;

  const res = http.get(url, {
    headers: {
      'X-Test-Run-Id': TEST_RUN_ID,
    },
    tags: { name: 'search', test_run_id: TEST_RUN_ID },
  });

  const success = check(res, {
    'search status is 200': (r) => r.status === 200,
    'search has body': (r) => r.body && r.body.length > 0,
  });

  searchP95.add(res.timings.duration);
  searchErrorRate.add(!success);
  searchReqs.add(1);

  sleep(1);
}

// ============================================================================
// Hàm teardown — in Test Run ID ra summary
// ============================================================================
export function teardown(data) {
  console.log(`\n================================================================`);
  console.log(`TEST RUN ID: ${TEST_RUN_ID}`);
  console.log(`BASE URL: ${BASE_URL}`);
  console.log(`================================================================\n`);
}
