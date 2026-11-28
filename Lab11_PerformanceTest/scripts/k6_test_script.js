import http from 'k6/http';
import { check, sleep } from 'k6';

// Cấu hình kịch bản kiểm thử k6 cho 12 kịch bản hiệu năng
// Hỗ trợ truyền biến môi trường VUS, DURATION, FUNCTION_ID, TEST_RUN_ID
export const options = {
    vus: __ENV.VUS ? parseInt(__ENV.VUS) : 10,
    duration: __ENV.DURATION || '15s',
    thresholds: {
        http_req_failed: ['rate<0.01'], // Tỷ lệ lỗi < 1%
        http_req_duration: ['p(95)<3000'], // p95 < 3s (tối đa theo SLA)
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://127.0.0.1:5050';
const FUNCTION_ID = __ENV.FUNCTION_ID ? parseInt(__ENV.FUNCTION_ID) : 1;
const TEST_RUN_ID = __ENV.TEST_RUN_ID || 'RUN-2324801030042-DEFAULT';

const params = {
    headers: {
        'Content-Type': 'application/json',
        'X-Test-Run-Id': TEST_RUN_ID,
    },
};

export default function () {
    if (FUNCTION_ID === 1) {
        // Chức năng 1 - Tạo đề nghị bảo hiểm và tính phí
        const payload = JSON.stringify({
            productCode: 'TRI',
            policyFrom: '2026-11-23T00:00:00Z',
            policyTo: '2027-11-23T00:00:00Z',
            selectedCovers: ['C1', 'C2'],
            answers: [
                { questionCode: 'NUM_OF_ADULTS', questionType: 'Choice', answer: '2' },
                { questionCode: 'DESTINATION', questionType: 'Choice', answer: 'EUR' },
            ],
        });
        const res = http.post(`${BASE_URL}/api/offers`, payload, params);
        check(res, {
            'status is 200': (r) => r.status === 200,
            'p95 SLA <= 2s': (r) => r.timings.duration <= 2000,
        });
    } else if (FUNCTION_ID === 2) {
        // Chức năng 2 - Chuyển đề nghị thành hợp đồng
        const payload = JSON.stringify({
            offerNumber: `OFF-2026-${Math.floor(Math.random() * 900000 + 100000)}`,
            policyHolder: {
                firstName: 'Nhat Anh',
                lastName: 'Le',
                taxId: '2324801030042',
            },
            policyHolderAddress: {
                country: 'Vietnam',
                zipCode: '820000',
                city: 'Thu Dau Mot',
                street: 'Tran Van On',
            },
        });
        const res = http.post(`${BASE_URL}/api/policies`, payload, params);
        check(res, {
            'status is 200': (r) => r.status === 200,
            'p95 SLA <= 3s': (r) => r.timings.duration <= 3000,
        });
    } else if (FUNCTION_ID === 3) {
        // Chức năng 3 - Tìm kiếm đề nghị/hợp đồng
        const queries = ['Le Nhat Anh', 'TRI', 'CAR', 'POL-2026'];
        const q = queries[Math.floor(Math.random() * queries.length)];
        const res = http.get(`${BASE_URL}/api/PolicySearch?q=${encodeURIComponent(q)}`, params);
        check(res, {
            'status is 200': (r) => r.status === 200,
            'p95 SLA <= 2s': (r) => r.timings.duration <= 2000,
        });
    }

    sleep(0.1);
}
