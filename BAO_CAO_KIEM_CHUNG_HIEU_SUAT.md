# BÁO CÁO KIỂM CHỨNG HIỆU SUẤT KIẾN TRÚC PHẦN MỀM
## Bài Thực Hành Số 11 — Case Study: Hệ thống Đại lý Bảo hiểm (Microservices .NET 8)

**Sinh viên:** Lê Nhật Anh — MSSV: 2324801030042  
**Môn học:** Kiểm thử phần mềm / Kiến trúc phần mềm  
**Hạn nộp:** 23h59 ngày 22/11/2026  
**Test Run ID:** `<TEST_RUN_ID>` *(được sinh tự động bởi script k6, xem chi tiết bên dưới)*

---

## 1. GIỚI THIỆU HỆ THỐNG VÀ KIẾN TRÚC HIỆN TẠI

### 1.1 Mô tả hệ thống
Hệ thống **InsuranceAgency** là một ứng dụng bảo hiểm du lịch xây dựng theo kiến trúc **microservices** trên nền tảng **.NET 8**. Hệ thống cho phép các đại lý bảo hiểm:
- Chọn sản phẩm bảo hiểm phù hợp cho khách hàng
- Tính toán giá bảo hiểm (premium)
- Tạo báo giá (quote) và chuyển thành hợp đồng bảo hiểm (policy)
- Tìm kiếm báo giá và hợp đồng
- Thanh toán, quản lý tài liệu, chat hỗ trợ...

### 1.2 Kiến trúc hệ thống
Hệ thống sử dụng mô hình **Web Application** ba tầng chính:

| Tầng | Thành phần | Công nghệ | Port |
|------|-----------|-----------|------|
| **Frontend** | Giao diện tương tác người dùng (SPA) | Vue.js + Nginx | 8080 |
| **API Gateway** | Cổng API che giấu phức tạp backend, bảo mật, tổng hợp | Ocelot (.NET 8) | 8099 |
| **Backend/API** | 9 microservice xử lý nghiệp vụ | ASP.NET Core Web API, MediatR, NHibernate/Marten | 5030–5070 |
| **Database** | Lưu trữ dữ liệu | PostgreSQL (4 DB: payments, jobs, policy, pricing) | 5432 |
| **Message Bus** | Truyền sự kiện bất đồng bộ | RabbitMQ | 5672 / 15672 |
| **Service Discovery** | Đăng ký và khám phá dịch vụ | Eureka Server | 8761 |
| **Search Index** | Chỉ mục tìm kiếm đầy đủ văn bản | Elasticsearch | 9200 |
| **Cache/Job** | Lưu trữ và xử lý công việc nền | (Marten, Hangfire) | — |

**Luồng dữ liệu典型:**
`Frontend (:8080)` → `API Gateway (:8099)` → `Microservice (REST)` → `PostgreSQL`  
`Microservice` → `RabbitMQ (Outbox/Event)` → `PolicySearchService` → `Elasticsearch (indexing)`

### 1.3 Các microservice chính
1. **AuthService** — Xác thực, cấp token JWT
2. **ChatService** — Trò chuyện trực tiếp (SignalR)
3. **ProductService** — Danh mục sản phẩm bảo hiểm
4. **PricingService** — Tính phí bảo hiểm
5. **PolicyService** — Tạo báo giá và hợp đồng (chính)
6. **PolicySearchService** — Tìm kiếm báo giá/hợp đồng qua Elasticsearch
7. **PaymentService** — Quản lý tài khoản thanh toán
8. **DocumentService** — Tạo chứng chỉ PDF (JS Report)
9. **DashboardService** — Báo cáo tổng hợp doanh số

---

## 2. MÔ TẢ MÔI TRƯỜNG VÀ CÔNG CỤ KIỂM THỬ

### 2.1 Mô hình triển khai
- **Môi trường kiểm thử:** Localhost (cả hệ thống phần mềm và công cụ kiểm thử chạy trên cùng một máy tính).
- **Triển khai:** Toàn bộ hệ thống được đóng gói trong **Docker Compose** (các container: postgres, eureka-server, elasticsearch, rabbitmq, 9 microservice, gateway, frontend Vue).
- **Công cụ kiểm thử chạy trên:** Cùng máy localhost, gửi yêu cầu HTTP đến các endpoint của hệ thống qua API Gateway (port 8099) hoặc trực tiếp đến từng service.

### 2.2 Cấu hình máy tính kiểm thử (đề nghị ghi nhận thực tế)
> *Sinh viên hãy điền thông tin máy tính của mình sau khi kiểm tra.*

| Thông tin | Giá trị (điền thực tế) |
|-----------|------------------------|
| Hệ điều hành | Windows 10/11 |
| CPU | __________ (ví dụ: Intel Core i5-10400, 2.90 GHz, 6 nhân) |
| RAM | __________ (ví dụ: 16 GB) |
| Dung lượng ổ cứng trống | __________ |
| Docker Desktop version | __________ |
| .NET SDK version | __________ |
| Mạng | Localhost (loopback), không giới hạn băngidth bên ngoài |

### 2.3 Công nghệ sử dụng
| Tầng | Công nghệ | Phiên bản |
|------|-----------|-----------|
| Frontend | Vue.js (SPA) + Nginx | Vue 2/3, Nginx stable |
| Backend | ASP.NET Core Web API | .NET 8 |
| Backend pattern | MediatR (CQRS-nhẹ), Repository, Unit of Work | MediatR 12.4.1 |
| Database | PostgreSQL | 11.4 (Docker) |
| Database ORM | NHibernate / Marten | — |
| Message bus | RabbitMQ | 3-management |
| Service discovery | Eureka Server | Java (Gradle) |
| Search | Elasticsearch | 8.9.2 |
| API Gateway | Ocelot | .NET 8 |
| **Công cụ kiểm thử hiệu suất** | **Grafana k6** | **vì latest stable (cài đặt trên máy)** |

### 2.4 Công cụ kiểm thử: Grafana k6
- **Tên:** Grafana k6
- **Phiên bản:** (điền phiên bản cài đặt thực tế, ví dụ: k6 v0.47.0)
- **Hình thức sử dụng:** Cài đặt trên máy tính (local), chạy script JavaScript (`.js`)
- **Script kiểm thử:** `k6-performance-test.js` (đã nộp kèm)
- **Tính năng dùng:** constant-vus executor, custom metrics (Trend, Rate, Counter), tags để phân tách kịch bản, Test Run ID gắn trên header `X-Test-Run-Id`

### 2.5 Điều kiện mạng và giới hạn
- Mọi giao tiếp chạy trên `localhost` (loopback), độ trễ mạng cực nhỏ.
- Không có giới hạn băngidth bên ngoài影响 kết quả.
- Các container Docker chia sẻ tài nguyên CPU/RAM với máy chủ, có thể ảnh hưởng chậm trễ khi nhiều service khởi động cùng lúc.
- Không có tường lửa hay proxy giữa công cụ kiểm thử và hệ thống (cùng máy).

---

## 3. MÔ TẢ BA CHỨC NĂNG NGHIỆP VỤ VÀ KỊCH BẢN KIỂM THỬ

### 3.1 Chức năng 1 — Tạo đề nghị bảo hiểm và tính phí (Create Quote & Calculate Premium)
**Mô tả:** Người dùng nhập thông tin sản phẩm, ngày bảo hiểm, điểm đến, số lượng người lớn/trẻ em. Hệ thống xử lý quy tắc nghiệp vụ, tính phí bảo hiểm và trả về kết quả báo giá.

- **Endpoint:** `POST /api/Offer` (PolicyService, qua Gateway `:8099`)
- **Header yêu cầu:** `Content-Type: application/json`, `AgentLogin`
- **Payload mẫu:** `{ productId, policyFrom, policyTo, destination, numberOfAdults, numberOfChildren }`
- **Kết quả mong đợi:** HTTP 200/201, có nội dung báo giá (quote) và phí tính toán.

### 3.2 Chức năng 2 — Chuyển đề nghị thành hợp đồng (Convert Quote to Policy)
**Mô tả:** Người dùng chọn một đề giá báo giá hợp lệ và thực hiện chuyển đổi thành hợp đồng bảo hiểm. Hệ thống kiểm tra điều kiện, xử lý nghiệp vụ và lưu thông tin hợp đồng vào cơ sở dữ liệu.

- **Endpoint:** `POST /api/Policy` (PolicyService, qua Gateway `:8099`)
- **Header yêu cầu:** `Content-Type: application/json`, `AgentLogin`
- **Payload mẫu:** `{ quoteId, policyHolder: { firstName, lastName, taxId, country, zip, city, street } }`
- **Kết quả mong đợi:** HTTP 200/201, có số hợp đồng (policyNumber) và thông tin tài khoản thanh toán.

### 3.3 Chức năng 3 — Tìm kiếm đề nghị/hợp đồng (Search Quotes & Policies)
**Mô tả:** Người dùng tìm kiếm đề nghị bảo hiểm hoặc hợp đồng bảo hiểm theo các tiêu chí (ví dụ: tên khách hàng). Hệ thống truy vấn dữ liệu và trả về danh sách kết quả.

- **Endpoint:** `GET /api/PolicySearch?q=...` (PolicySearchService, qua Gateway `:8099`)
- **Header yêu cầu:** Khác (không cần auth cho tìm kiếm công khai trong môi trường lab)
- **Query mẫu:** `?q=Nhat`
- **Kết quả mong đợi:** HTTP 200, danh sách báo giá/hợp đồng khớp từ Elasticsearch.

---

## 4. THÔNG SỐ CẤU HÌNH VÀ TEST RUN ID

### 4.1 Test Run ID
**Test Run ID** là mã định danh duy nhất cho mỗi lần chạy kiểm thử, được sinh tự động bởi script k6 dưới dạng:
```
__TEST_RUN_ID_<timestamp_millis>__
```
Ví dụ: `__TEST_RUN_ID_1732254000000__`

Test Run ID được:
1. Ghi trong cấu hình script (`k6-performance-test.js`, hằng `TEST_RUN_ID`)
2. Gắn vào mọi request thông qua header HTTP `X-Test-Run-Id` và k6 tag `test_run_id`
3. Ghi nhận trong báo cáo này (điền ở trên đầu trang) và trong k6 summary output
4. Dùng để đối chiếu với log backend (xem mục 5)

### 4.2 Mức tải kiểm thử (Virtual Users – VUs)
Mỗi chức năng được kiểm thử tại **4 mức tải**: 1, 10, 50, 100 VUs. Tổng cộng **12 kịch bản**.

Chạy ví dụ (mỗi lần là 1 kịch bản):
```powershell
# Chức năng 1, 1 VUs, 30 giây
k6 run --env VUS=1 --env DURATION=30s k6-performance-test.js

# Chức năng 1, 10 VUs
k6 run --env VUS=10 --env DURATION=30s k6-performance-test.js

# ... lặp cho 50 và 100 VUs, và cho 3 chức năng
```

### 4.3 Tiêu chí chấp nhận (đề bài)
| Chức năng | p95 Response Time | Error Rate |
|-----------|-------------------|------------|
| Tạo đề nghị và tính phí | ≤ 2 giây | < 1% |
| Chuyển đề nghị thành hợp đồng | ≤ 3 giây | < 1% |
| Tìm kiếm đề nghị/hợp đồng | ≤ 2 giây | < 1% |

---

## 5. BẢNG KẾT QUẢ CỦA 12 KỊCH BẢN

> **Lưu ý:** Các ô số liệu (p95, Error Rate, Throughput, Kết luận) phải được điền bằng **kết quả thực tế** sau khi chạy script `k6-performance-test.js`. Dưới đây là bảng mẫu để ghi nhận.

| STT | Chức năng | VUs | p95 (giây) | Error Rate (%) | Throughput (req/s) | Kết luận (Đạt/Không đạt) |
|:---:|-----------|:---:|:----------:|:--------------:|:------------------:|:------------------------:|
| 1 | Tạo đề nghị và tính phí | 1 | | | | |
| 2 | Tạo đề nghị và tính phí | 10 | | | | |
| 3 | Tạo đề nghị và tính phí | 50 | | | | |
| 4 | Tạo đề nghị và tính phí | 100 | | | | |
| 5 | Chuyển đề nghị thành hợp đồng | 1 | | | | |
| 6 | Chuyển đề nghị thành hợp đồng | 10 | | | | |
| 7 | Chuyển đề nghị thành hợp đồng | 50 | | | | |
| 8 | Chuyển đề nghị thành hợp đồng | 100 | | | | |
| 9 | Tìm kiếm đề nghị/hợp đồng | 1 | | | | |
| 10 | Tìm kiếm đề nghị/hợp đồng | 10 | | | | |
| 11 | Tìm kiếm đề nghị/hợp đơn | 50 | | | | |
| 12 | Tìm kiếm đề nghị/hợp đồng | 100 | | | | |

### Cách đọc chỉ số từ k6 output
Sau mỗi lần chạy, k6 in ra summary ví dụ:
```
create_quote_p95............: 450ms   ← p95 (ms), đổi sang giây
create_quote_error_rate.....: 0.00%   ← Error Rate
create_quote_requests.......: 120     ← tổng số request
                             → Throughput = requests / duration (s)
```

**Test Run ID của lần chạy này:** `<điền ID từ log k6, ví dụ __TEST_RUN_ID_1732254000000__>`  
**Thời điểm kiểm thử:** `<điền ngày/giờ chạy, ví dụ 22/11/2026 20:15>`

---

## 6. MINH CHỨNG LOG HỆ THỐNG (SẢN PHẨM 4)

Sinh viên phải nộp kèm log backend để đối chiếu với Test Run ID. Các bước thu thập:

### 6.1 Lấy log backend qua Docker
```powershell
# Log toàn bộ container (hoặc lọc theo service)
docker compose -f scripts\app.yml logs --tail=200 dotnet-policy-service > logs_policy_service.txt
docker compose -f scripts\app.yml logs --tail=200 dotnet-policy-search-service > logs_policy_search.txt
docker compose -f scripts\app.yml logs --tail=200 dotnet-agent-portal-gateway > logs_gateway.txt
```

### 6.2 Đối chiếu log với Test Run ID
Mỗi request từ k6 mang header `X-Test-Run-Id: __TEST_RUN_ID_...__`. Trong log backend, tìm chuỗi Test Run ID này để xác định:
- Thời điểm ghi nhận yêu cầu/giao dịch
- Service nào xử lý request
- Status code trả về (200 = thành công, 5xx = lỗi)
- Thời gian xử lý tại backend

### 6.3 File log cần nộp
- `logs_gateway.txt` — log API Gateway
- `logs_policy_service.txt` — log PolicyService (chức năng 1 & 2)
- `logs_policy_search_service.txt` — log PolicySearchService (chức năng 3)
- (Tùy chọn) `logs_pricing_service.txt` — log PricingService

---

## 7. SẢN PHẨM SINH VIÊN PHẢI NỘP

| STT | Sản phẩm | File | Mô tả |
|:---:|----------|------|-------|
| 1 | Báo cáo kiểm chứng hiệu suất (PDF) | `BAO_CAO_KIEM_CHUNG_HIEU_SUAT.pdf` | Nội dung của file này, xuất ra PDF |
| 2 | Tệp cấu hình/script kiểm thử | `k6-performance-test.js` | Script k6 đã nộp kèm |
| 3 | Dữ liệu kết quả kiểm thử gốc | `k6-result-*.json` | Xuất bằng lệnh: `k6 run --out json=result.json ...` |
| 4 | Minh chứng log hệ thống | `logs_*.txt` | Log backend đối chiếu Test Run ID |

### Lệnh xuất kết quả gốc (JSON) từ k6
```powershell
k6 run --env VUS=10 --env DURATION=30s --out json=k6-result-createquote-10vus.json k6-performance-test.js
```

---

## 8. HƯỚNG DẪN THỰC HÀNH (TÓM TẮT)

```powershell
# 1. Cài k6 (trên Windows): winget install grafana.k6
# 2. Khởi động hệ thống (Docker): cd scripts && docker compose -f infra.yml up -d && docker compose -f app.yml up -d
# 3. Chạy từng kịch bản (ví dụ chức năng 1, 10 VUs):
k6 run --env VUS=10 --env DURATION=30s k6-performance-test.js
# 4. Lưu Test Run ID từ output k6 vào báo cáo (mục 4 & 5)
# 5. Lấy log backend: docker compose -f scripts\app.yml logs --tail=200 <service> > logs_<service>.txt
# 6. Điền bảng kết quả (mục 5) và xuất báo cáo ra PDF
```

---

> **Lưu ý về tính trung thực:** Toàn bộ số liệu trong bảng mục 5 phải là kết quả thực tế thu được từ việc chạy script `k6-performance-test.js` trên hệ thống đã triển khai. Không được sao chép dữ liệu của người khác. Giảng viên có thể đối chiếu Test Run ID, file kết quả JSON gốc và log backend để kiểm tra.
