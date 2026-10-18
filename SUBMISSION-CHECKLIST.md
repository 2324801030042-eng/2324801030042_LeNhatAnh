# Checklist nộp bài — BTH4 Phần 2 (hạn 18h00 11/10/2026)

## Đã làm xong bằng máy này
- [x] Solution `InsuranceAgency.sln`: 29 projects (9 services × 3 + Gateway + Blazor)
- [x] MediatR 12.4.1 trong 9 Implementation project
- [x] Copy đủ 6 thư mục đề bài: eureka-server, postgres, scripts, Web, BlazorWasmClient, AgentPortalApiGateway
- [x] `dotnet build` → **Build succeeded, 0 Error(s)** (xem `build.log`)
- [x] `dotnet test` → pass
- [x] Git repo đã `init` + commit baseline
- [ ] PostgreSQL native đang cài (winget, chạy nền) → xong sẽ tạo DB + `pg_dump` vào `db-backup/`

## Bạn phải tự làm (cần tài khoản/truy cập của bạn)
1. **Sản phẩm #1** — GitHub: tạo repo, push, add `caotxuan@gmail.com` collaborator.
   Không làm được thì nén project → Drive → nộp link eLearning (tối đa 8đ).
2. **Sản phẩm #2** — eLearning: nộp file trong `db-backup/`
   (`createdatabases.sql` đã có sẵn; các file `lab_netmicro_*.sql` sẽ có sau khi
   PostgreSQL cài xong — kiểm tra lại thư mục này trước khi nộp).
3. **Sản phẩm #3** — eLearning: chụp màn hình build thành công
   (mở solution bằng Visual Studio → Build → chụp Output `Build succeeded`,
   hoặc chụp nội dung `build.log` dòng `0 Error(s)`).
4. Hạ tầng Docker (RabbitMQ/Elasticsearch/Eureka/Postgres container):
   máy này chưa có WSL2 nên Docker daemon không chạy được.
   Trên máy có Docker Desktop + WSL2: `cd scripts && docker compose -f infra.yml up -d`.
