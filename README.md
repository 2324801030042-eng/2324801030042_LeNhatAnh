# InsuranceAgency — Bài Thực Hành Số 4 (Phần 2: Cài đặt môi trường)

Case study: kiến trúc Microservices bài toán Đại lý Bảo hiểm trên .NET 8.

## 1. Task #1 — Quy tắc tổ chức source code

Mỗi microservice gồm **3 project**: API definition (ports), Implementation, Test.

| Service | API definition | Implementation | Test |
|---|---|---|---|
| AuthService | `AuthService.API` | `AuthService.Implementation` | `AuthService.Tests` |
| ChatService | `ChatService.API` | `ChatService.Implementation` | `ChatService.Tests` |
| ProductService | `ProductService.Api` | `ProductService` | `ProductService.Test` |
| PricingService | `PricingService.Api` | `PricingService` | `PricingService.Test` |
| PolicyService | `PolicyService.Api` | `PolicyService` | `PolicyService.Test` |
| PaymentService | `PaymentService.Api` | `PaymentService` | `PaymentService.Test` |
| PolicySearchService | `PolicySearchService.Api` | `PolicySearchService` | `PolicySearchService.Test` |
| DocumentService | `DocumentService.Api` | `DocumentService` | `DocumentService.Test` |
| DashboardService | `DashboardService.Api` | `DashboardService` | `DashboardService.Test` |

Tổng: 9 services × 3 = 27 projects + `AgentPortalApiGateway` + `BlazorWasmClient` = **29 projects** trong `InsuranceAgency.sln`.

Các thư mục add vào solution (theo Task #2.6–2.11):
- `eureka-server/` — Eureka service discovery (Java/Gradle, port 8761)
- `postgres/` — Dockerfile + `createdatabases.sql`
- `scripts/` — `app.yml`, `infra.yml`, script build/run
- `Web/` — Frontend VueJS (SPA)
- `BlazorWasmClient/` — Frontend Blazor WASM (đã có sẵn, build thành công)
- `AgentPortalApiGateway/` — Ocelot API Gateway (port 8099)

> Ghi chú đặt tên: các service mới dùng hậu tố `.Api` (theo `scripts/build.sh` và
> `BlazorWasmClient.csproj` yêu cầu `..\ProductService.Api\...`, `..\PolicyService.Api\...`).
> Hai service có sẵn (Auth/Chat) giữ tên gốc `.API` để không phá vỡ lịch sử — tương đương
> chữ hoa/thường trên Windows và vẫn thỏa quy tắc 3-project/service.

## 2. Task #2 — Code ban đầu & môi trường

1. **MediatR 12.4.1** đã cài vào cả 9 Implementation project
   (`dotnet add package MediatR`). Xem `docs/MediatR-Setup.md` cho code đăng ký DI
   (cả kiểu `Startup.ConfigureServices` theo đề bài và kiểu minimal API .NET 8).
2. **PostgreSQL >= 10**: Dockerfile dùng `postgres:11.4`; script tạo DB:
   `postgres/createdatabases.sql` (4 DB: payments, jobs, policy, pricing + user `lab_user`).
   File backup nộp thầy nằm ở `db-backup/` (xem `docs/Environment-Setup.md`).
3. **RabbitMQ**: `rabbitmq:3-management` (5672, 15672) qua `scripts/infra.yml`.
4. **Elasticsearch >= 6**: image `8.9.2` single-node (9200) qua `scripts/infra.yml`.
5. **Docker & Docker Compose**: Docker 29.8.0 + Compose v5.5.1 đã kiểm tra trên máy làm bài.
6–11. Các thư mục đã copy vào solution root (xem bảng trên).

API definition projects (`*.Api`) hiện chứa **stub DTO/Command/Query tối thiểu**
để `BlazorWasmClient` build được (Phần 2 chỉ yêu cầu tạo tên + build thành công;
logic nghiệp vụ, handler MediatR, EF Core, RabbitMQ subscriber, ElasticSearch
indexing triển khai ở Phần 3).

## 3. Build (Sản phẩm #3)

```
dotnet build InsuranceAgency.sln --nologo
```

Kết quả trên máy làm bài (.NET 9.0.306 SDK, target net8.0):
**Build succeeded — 0 Error(s)** (chi tiết xem `build.log`).
Test: `dotnet test` — tất cả Test project pass (xUnit stub Test1).

Ảnh chụp màn hình: mở `build.log` (dòng `Build succeeded` / `0 Error(s)`)
hoặc build lại trong Visual Studio và chụp cửa sổ Output.

## 4. Nộp sản phẩm

- **Sản phẩm #1 (code)**: push repo này lên GitHub, add `caotxuan@gmail.com`
  làm collaborator (Settings → Collaborators). Hoặc nén project nộp Drive (tối đa 8đ).
  ```bash
  git init && git add . && git commit -m "BTH4 P2: microservices env setup"
  gh repo create InsuranceAgency --private --source=. --push
  ```
- **Sản phẩm #2 (DB backup)**: nộp các file trong `db-backup/`
  (`createdatabases.sql` + các file `lab_netmicro_*.sql` từ `pg_dump`).
  Thầy restore bằng `psql -f <file>` hoặc `pg_restore`.
- **Sản phẩm #3 (build)**: nộp `build.log` + ảnh chụp màn hình build thành công.

Hạn nộp: trước 18h00 ngày 11/10/2026.
