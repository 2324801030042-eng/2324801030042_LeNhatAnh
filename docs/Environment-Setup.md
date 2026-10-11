# Môi trường: PostgreSQL / RabbitMQ / Elasticsearch / Docker (Task #2.2–2.5)

## Docker & Compose (đã có)

- Docker 29.8.0, Compose v5.5.1 — kiểm tra: `docker --version && docker compose version`.

## Hạ tầng dùng chung (`scripts/infra.yml`)

```bash
cd scripts
docker compose -f infra.yml up -d        # eureka + postgres + elasticsearch + rabbitmq
docker compose -f infra.yml ps
```

| Dịch vụ | Image | Port |
|---|---|---|
| eureka-server | build `../eureka-server` (Gradle/Java) | 8761 |
| postgres | build `../postgres` (`postgres:11.4` + `createdatabases.sql`) | 5432 (`lab_pass`) |
| elasticsearch | `docker.elastic.co/elasticsearch/elasticsearch:8.9.2` | 9200 |
| rabbitmq | `rabbitmq:3-management` | 5672, 15672 |

Yêu cầu đề bài: PostgreSQL >= 10.0 (dùng 11.4 ✔), Elasticsearch >= 6.0 (dùng 8.9.2 ✔).

## Tạo CSDL PostgreSQL

Khi container postgres khởi động lần đầu, file `postgres/createdatabases.sql`
tự chạy (via `/docker-entrypoint-initdb.d/`), tạo:

- user `lab_user` / password `lab_pass`
- `lab_netmicro_payments`, `lab_netmicro_jobs`, `lab_netmicro_policy`, `lab_netmicro_pricing`

Tạo thủ công (nếu cài PostgreSQL native):

```bash
psql -U postgres -h localhost -f postgres/createdatabases.sql
```

## Backup để nộp (Sản phẩm #2) — thư mục `db-backup/`

```bash
pg_dump -U lab_user -h localhost -Fp -f db-backup/lab_netmicro_payments.sql lab_netmicro_payments
pg_dump -U lab_user -h localhost -Fp -f db-backup/lab_netmicro_jobs.sql lab_netmicro_jobs
pg_dump -U lab_user -h localhost -Fp -f db-backup/lab_netmicro_policy.sql lab_netmicro_policy
pg_dump -U lab_user -h localhost -Fp -f db-backup/lab_netmicro_pricing.sql lab_netmicro_pricing
```

Thầy restore:

```bash
psql -U postgres -h localhost -f db-backup/createdatabases.sql
psql -U lab_user -h localhost -d lab_netmicro_payments -f db-backup/lab_netmicro_payments.sql
# ... tương tự các DB còn lại
```

Nếu máy chấm chưa kịp chạy Docker/Postgres, file `db-backup/createdatabases.sql`
(copy từ `postgres/`) vẫn chứng minh được việc cài đặt CSDL theo yêu cầu.
