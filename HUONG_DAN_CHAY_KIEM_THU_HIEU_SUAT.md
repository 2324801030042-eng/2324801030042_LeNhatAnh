# HƯỚNG DẪN CHẢY KIỂM THỬ HIỆU SUẤT (BTH11)
## Sử dụng Grafana k6 — Hệ thống InsuranceAgency Microservices

---

## Bước 0: Điều kiện trước
Hệ thống đã được triển khai và đang chạy qua Docker Compose (xem `docs/DEMO.md`). Trước khi chạy kiểm thử, hãy đảm bảo:

```powershell
# Khởi động Docker Desktop, sau đó:
cd scripts
docker compose -f infra.yml up -d
docker compose -f app.yml up -d
docker restart dotnet-web-vue   # nếu cần để Nginx resolve gateway
```

Kiểm tra hệ thống sẵn sàng:
- API Gateway: `http://localhost:8099`
- PolicyService: `http://localhost:5050`
- PolicySearchService: `http://localhost:5065`
- Eureka: `http://localhost:8761`
- RabbitMQ Management: `http://localhost:15672` (guest/guest)

---

## Bước 1: Cài đặt Grafana k6

Trên Windows, có thể cài qua winget:
```powershell
winget install grafana.k6
```

Hoặc tải từ: https://grafana.com/docs/k6/latest/set-up/install-k6/

Kiểm tra đã cài thành công:
```powershell
k6 version
```

---

## Bước 2: Chạy kiểm thử theo từng kịch bản

Mỗi chức năng cần chạy **4 mức tải**: 1, 10, 50, 100 VUs.  
Tổng cộng **12 lần chạy** (3 chức năng × 4 mức tải).

Ví dụ — **Chức năng 1: Tạo đề nghị và tính phí**, mức 10 VUs, thời gian 30 giây:
```powershell
k6 run --env VUS=10 --env DURATION=30s k6-performance-test.js
```

### Danh sách 12 lệnh cần chạy

| # | Chức năng | VUs | Lệnh |
|---|-----------|-----|------|
| 1 | Tạo đề nghị & tính phí | 1 | `k6 run --env VUS=1 --env DURATION=30s k6-performance-test.js` |
| 2 | Tạo đề nghị & tính phí | 10 | `k6 run --env VUS=10 --env DURATION=30s k6-performance-test.js` |
| 3 | Tạo đề nghị & tính phí | 50 | `k6 run --env VUS=50 --env DURATION=30s k6-performance-test.js` |
| 4 | Tạo đề nghị & tính phí | 100 | `k6 run --env VUS=100 --env DURATION=30s k6-performance-test.js` |
| 5 | Chuyển thành hợp đồng | 1 | `k6 run --env VUS=1 --env DURATION=30s k6-performance-test.js` |
| 6 | Chuyển thành hợp đồng | 10 | `k6 run --env VUS=10 --env DURATION=30s k6-performance-test.js` |
| 7 | Chuyển thành hợp đồng | 50 | `k6 run --env VUS=50 --env DURATION=30s k6-performance-test.js` |
| 8 | Chuyển thành hợp đồng | 100 | `k6 run --env VUS=100 --env DURATION=30s k6-performance-test.js` |
| 9 | Tìm kiếm | 1 | `k6 run --env VUS=1 --env DURATION=30s k6-performance-test.js` |
| 10 | Tìm kiếm | 10 | `k6 run --env VUS=10 --env DURATION=30s k6-performance-test.js` |
| 11 | Tìm kiếm | 50 | `k6 run --env VUS=50 --env DURATION=30s k6-performance-test.js` |
| 12 | Tìm kiếm | 100 | `k6 run --env VUS=100 --env DURATION=30s k6-performance-test.js` |

> **Lưu ý:** Vì script k6 chạy cả 3 scenario cùng lúc (theo `options.scenarios`), mỗi lệnh trên thực tế sẽ chạy cả 3 chức năng với cùng số VUs. Để tách riêng dữ liệu theo chức năng, hãy chọn scenario tương ứng qua tag hoặc chạy từng file riêng.  
> Đối với báo cáo, bạn có thể đọc chỉ số từ phần output tương ứng với từng tag (`create_quote`, `convert_policy`, `search`).

---

## Bước 3: Lưu Test Run ID

Mỗi lần chạy k6 sẽ in ra Test Run ID ở cuối output:
```
================================================================
TEST RUN ID: __TEST_RUN_ID_1732254000000__
BASE URL: http://localhost:8099
================================================================
```

**Hãy copy Test Run ID này và điền vào:**
- Báo cáo (mục 4.1 và 5 — thời điểm kiểm thử)
- Tên file kết quả gốc (xem Bước 4)

---

## Bước 4: Xuất dữ liệu kết quả gốc (JSON)

Để nộp là "dữ liệu kết quả kiểm thử gốc" (Sản phẩm 3), hãy xuất ra JSON:
```powershell
k6 run --env VUS=10 --env DURATION=30s --out json=k6-result-createquote-10vus.json k6-performance-test.js
```

Lặp lại cho từng kịch bản và đổi tên file cho gọn:
- `k6-result-createquote-1vus.json`
- `k6-result-createquote-10vus.json`
- ...
- `k6-result-search-100vus.json`

File JSON chứa đầy đủ các chỉ số: p95, error rate, throughput, số request, v.v.

---

## Bước 5: Thu thập log backend (Sản phẩm 4)

Sau mỗi lần kiểm thử, hãy lấy log các container để đối chiếu với Test Run ID:

```powershell
# Log API Gateway
docker compose -f scripts\app.yml logs --tail=300 dotnet-agent-portal-gateway > logs_gateway.txt

# Log PolicyService (chức năng 1 & 2)
docker compose -f scripts\app.yml logs --tail=300 dotnet-policy-service > logs_policy_service.txt

# Log PolicySearchService (chức năng 3)
docker compose -f scripts\app.yml logs --tail=300 dotnet-policy-search-service > logs_policy_search_service.txt

# Log PricingService (tính phí)
docker compose -f scripts\app.yml logs --tail=300 dotnet-pricing-service > logs_pricing_service.txt
```

Trong file log, hãy tìm chuỗi **Test Run ID** (ví dụ `__TEST_RUN_ID_1732254000000__`) để xác định:
- Thời điểm ghi nhận yêu cầu
- Service xử lý
- Status code (200 = OK, 5xx = lỗi)

---

## Bước 6: Điền bảng kết quả vào báo cáo

Mở file `BAO_CAO_KIEM_CHUNG_HIEU_SUAT.md`, điền vào bảng mục 5:

| STT | Chức năng | VUs | p95 (giây) | Error Rate (%) | Throughput | Kết luận |
|:---:|-----------|:---:|:----------:|:--------------:|:----------:|:--------:|
| 1 | Tạo đề nghị & tính phí | 1 | 0.45 | 0.0 | 4.0 | Đạt |

Lấy p95 từ k6 output (đơn vị ms → đổi sang giây):
```
create_quote_p95............: 450ms   → 0.45 giây
create_quote_error_rate.....: 0.00%
```

Kết luận "Đạt" nếu p95 ≤ ngưỡng (2s/3s) VÀ Error Rate < 1%.

---

## Bước 7: Xuất báo cáo ra PDF

Mở `BAO_CAO_KIEM_CHUNG_HIEU_SUAT.md` bằng VS Code / Typora / Word, sau đó xuất/lưu thành PDF:
- Tên file: `BAO_CAO_KIEM_CHUNG_HIEU_SUAT.pdf`

---

## Danh sách file cần nộp

| STT | File | Sản phẩm |
|:---:|------|----------|
| 1 | `BAO_CAO_KIEM_CHUNG_HIEU_SUAT.pdf` | Báo cáo |
| 2 | `k6-performance-test.js` | Script kiểm thử |
| 3 | `k6-result-*.json` (12 file) | Kết quả gốc |
| 4 | `logs_gateway.txt`, `logs_policy_service.txt`, `logs_policy_search_service.txt`, ... | Log hệ thống |

---

## Lỗi thường gặp

| Lỗi | Nguyên nhân | Cách khắc phục |
|-----|-------------|----------------|
| `connect ECONNREFUSED localhost:8099` | Gateway chưa khởi động | Chạy `docker compose -f app.yml up -d` |
| `k6: command not found` | k6 chưa cài hoặc chưa thêm vào PATH | Cài lại k6, khởi động lại terminal |
| p95 quá cao (> 2s) | Hệ thống quá tải hoặc container chưa ổn định | Đợi hệ thống ổn định, chạy lại sau |
| Error Rate > 1% | Endpoint sai hoặc service chưa sẵn sàng | Kiểm tra URL endpoint, đảm bảo các container UP |
