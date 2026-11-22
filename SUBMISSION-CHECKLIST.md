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

---

## BTH11 — Kiểm chứng hiệu suất kiến trúc phần mềm (hạn 23h59 22/11/2026)

- [x] Script kiểm thử: `k6-performance-test.js` (Grafana k6, 3 chức năng × 4 mức VUs: 1/10/50/100, Test Run ID gắn kèm)
- [x] Báo cáo mẫu: `BAO_CAO_KIEM_CHUNG_HIEU_SUAT.md` (các mục 1–7, chờ điền số liệu thực từ k6 output)
- [x] Hướng dẫn chạy: `HUONG_DAN_CHAY_KIEM_THU_HIEU_SUAT.md`
- [ ] Chạy k6 thu thập kết quả thực (12 kịch bản), xuất file JSON gốc (`k6 run --out json=...`)
- [ ] Thu thập log backend (`docker compose logs ...`) đối chiếu Test Run ID
- [ ] Điền bảng kết quả 12 kịch bản vào báo cáo, xuất ra PDF
- [ ] Nộp 4 sản phẩm lên eLearning: Báo cáo PDF, script .js, file JSON gốc, log hệ thống

## Lưu ý thêm (cần tài khoản GitHub của bạn)
- Thêm `caotxuan@gmail.com` làm collaborator vào repo GitHub
  (Settings → Collaborators → Add people) — yêu cầu của BTH4 P2.
