# Demo hướng dẫn — BTH9 Phần 7 (Triển khai hệ thống)

## Trạng thái: HỆ THỐNG ĐANG CHẠY (Docker)

| Container | Port | Trạng thái |
|---|---|---|
| `postgres` | 5432 | UP |
| `eureka-server` | 8761 | UP |
| `elasticsearch` | 9200 | UP |
| `rabbit` | 5672 / **15672** (guest/guest) | UP |
| `dotnet-web-vue` (Frontend VueJS) | **8080** | UP |
| `dotnet-agent-portal-gateway` (Ocelot) | 8099 | UP |
| `dotnet-auth-service` | 6060 | UP |
| `dotnet-chat-service` | 4099 | UP |
| `dotnet-pricing-service` | 5040 | UP |
| `dotnet-product-service` | 5030 | UP |
| `dotnet-policy-service` | 5050 | UP |
| `dotnet-policy-search-service` | 5065 | UP |
| `dotnet-payment-service` | 5070 | UP |
| `dotnet-dashboard-service` | 5035 | UP |

## Tài khoản đăng nhập
- **Username:** `jimmy.solid` — **Password:** `secret`
- (thêm: `danny.solid/secret`, `admin/admin`)

## Chức năng demo #1 — Quy trình bán hàng trọn vẹn (Frontend `:8080`)

1. Vào **Account** → đăng nhập `jimmy.solid / secret`.
2. Vào **Products** → chọn sản phẩm **Safe Traveller (TRI)** → *Buy*.
3. Nhập:
   - Policy from: `2027-05-01`, Policy to: `2027-05-15`
   - Destination: **World**, Number of adults: `2`, Number of children: `2`
   - → **Calculate price** → giá hiện `216 EUR` (PricingService tính).
4. Bấm **Buy** → nhập Policy Holder:
   - First name: `Nhat`, Last name: `Anh Le`, Tax id: `2324801030042`
   - Country: `Poland` (mặc định), Zip: `31-002`, City: `Krakow`, Street: `Rynek Glowny 12`
   - → **Confirm** → chuyển tới trang **Policy details** (PolicyService tạo hợp đồng, lưu PostgreSQL; event `PolicyCreated` gửi qua RabbitMQ Outbox).
5. Vào **Policies** → ô Search nhập `Nhat` → Search → bảng hiện danh sách hợp đồng (PolicySearchService + ElasticSearch; event từ RabbitMQ đã index).
6. Bấm vào hợp đồng vừa tạo trong bảng → xem **Policy details** đầy đủ, gồm cả **Account number** (PaymentService đã tạo tài khoản thanh toán tự động qua event, PolicyService lấy qua REST).
7. Kiểm chứng RabbitMQ Management UI: `http://localhost:15672` (guest/guest) — xem queue `lab-payments-service-PolicyCreated`, `lab-policy-search-service-PolicyCreated`.
8. Kiểm chứng Eureka: `http://localhost:8761` — các service đã UP.

## Chức năng demo #2 — Dashboard + số liệu thật
- Đăng nhập → **Dashboard** → xem **TOTAL SALES**, **NUM. POLICIES**, phân bố theo sản phẩm → du liệu từ `DashboardService.Api` (query ElasticSearch `policy_lab_stats` index do `PolicyCreated` event).
- (API: `POST http://localhost:8099/api/Dashboard/total-sales` qua Gateway đã kiểm chứng 200, ví dụ `{"policiesCount":3,"premiumAmount":540}`.)

## Chức năng demo #3 (tùy chọn) — Chat trực tuyến
- **Chat for Agents** → trang chat kết nối SignalR `/agentsChat` (hub negotiate 200 với JWT).
- Mở 2 cửa sổ trình duyệt, đăng nhập 2 user khác nhau → chat qua lại.

## Ghi chú kỹ thuật (đã xử lý trong BTH9)
- Eureka Dockerfile: image `openjdk:8-jre-slim` bị gỡ → đổi `eclipse-temurin:8-jre`.
- `postgres` volume conflict (PG18) → build lại image `postgres:11.4` của repo, đổi volume sang `/tmp/data/postgresql-new`.
- `PricingService` thiếu Dockerfile → đã viết Dockerfile + `appsettings.docker.json`.
- PaymentService: lỗi `ObjectDisposedException` (Marten session bị dispose sớm) → handler mở session riêng cho mỗi event (`PolicyCreatedHandler`, `PolicyTerminatedHandler`).
- PolicyService: `PricingServiceUri` trong config docker trỏ sai host → sửa thành container name.
- PolicySearchService: gọi search không có `q` gây NRE (UI load toàn bộ danh sách) → thêm `FindAll()`; `Find()` null-safe.
- PolicyService: `GetPolicyDetailsHandler` nay gọi REST sang PaymentService lấy số tài khoản thanh toán (thay cho `null` hardcode).
- Web Vue: container khởi động trước gateway → Nginx không resolve được upstream → `docker restart dotnet-web-vue` sau khi gateway UP.

## Lệnh chạy lại sau khi tắt máy
```powershell
Start-Process "C:\Users\chigi\AppData\Local\Programs\DockerDesktop\Docker Desktop.exe"
# đợi Docker Ready
cd scripts
docker compose -f infra.yml up -d
docker compose -f app.yml up -d
docker restart dotnet-web-vue   # để Nginx resolve gateway sau
```
