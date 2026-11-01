# Lab 7 — Phần 5: Lưu trữ và truy cập dữ liệu hiệu quả cùng Marten

Case study: Cài đặt kiến trúc Microservices trên .NET Core (bài toán Đại lý Bảo hiểm).

## Mục tiêu

- Task #1: Cài **Marten** từ NuGet — thư viện client cho phép dùng PostgreSQL làm **document database** và **event store** với LINQ mở rộng, tuân thủ ACID đầy đủ.
- Task #2: Bổ sung code cho **PaymentService** (3 thư mục: `DataAccess/Marten`, `Listeners`, `Jobs`).
- Task #3: Bổ sung code cho **ProductService** (thư mục `DataAccess/EF`, `Domain`, `Queries`...).

## Chi tiết cài đặt

### Task #1 — Marten

`PaymentService.csproj`:
```xml
<PackageReference Include="Marten" Version="7.19.1" />
```

Các file Marten trong `PaymentService/DataAccess/Marten/`:
- `MartenInstaller.cs` — đăng ký `IDataStore` vào DI qua `AddMarten(...)`, connection string PostgreSQL.
- `MartenDataStore.cs` — triển khai `IDataStore`, mở phiên Marten (document session) cho `PolicyAccounts`.
- `MartenPolicyAccountRepository.cs` — repository lưu trữ/truy vấn `PolicyAccount` (tài khoản chính sách) trên PostgreSQL.
- `ProtectedSettersContractResolver.cs` — cấu hình JSON serialization cho phép gán property có private setter khi đọc document từ DB.

### Task #2 — PaymentService

- `Domain/`: `PolicyAccount`, `AccountingEntry`, `InPayment`, `OutPayment`, `ExpectedPayment`, `BankStatementFile`, `InPaymentRegistrationService`, `PolicyAccountNumberGenerator`... — nghiệp vụ tài khoản chính sách (tạo tài khoản khi chính sách được tạo, ghi nhận thanh toán đến, thanh toán ra).
- `DataAccess/Marten/`: bốn file ở trên (thay EF Core bằng Marten + PostgreSQL).
- `Listeners/`: `PolicyCreatedHandler`, `PolicyTerminatedHandler` — nhận event từ RabbitMQ (public bởi `PolicyService.Api.Events`) → MediatR khi có chính sách mới/kết thúc để tạo/đóng tài khoản chính sách.
- `Jobs/`: `InPaymentRegistrationJob` chạy bằng Hangfire — đọc file bank statement, gán các khoản thanh toán đến vào tài khoản chính sách phù hợp.
- `Queries/GetAccountBalanceHandler` — query số dư tài khoản chính sách.

### Task #3 — ProductService

- `Domain/`: `Product`, `Cover`, `Question`, `Choice`, `IProductRepository`.
- `DataAccess/EF/`: `ProductDbContext`, `ProductRepository`, `EFInstaller`, `Configuration/ProductConfig` (EF Core).
- `Commands/`: `CreateProductDraftHandler`, `ActivateProductHandler`, `DiscontinueProductHandler`.
- `Queries/`: `FindAllProductsHandler`, `FindProductByCodeHandler` (+ `Mappers/ProductMapper`).
- `Init/`: nạp dữ liệu demo khi start (`DataLoader`).

### Bổ sung cần thiết để build

- Thêm event `PolicyService.Api/Events/PolicyTerminated.cs` (PolicyNumber, PolicyTo, AmountToReturn) vì `PolicyTerminatedHandler` của PaymentService cần.
  - `PolicyCreated` đã có sẵn từ Lab 4/6.

## Kết quả build & test

```
dotnet build InsuranceAgency.sln  →  Build succeeded — 0 Error(s)
dotnet test ProductService.Test   →  Passed 14/14
dotnet test PaymentService.Test   →  Passed 6/6
```

Tổng: **20/20 test pass**, build toàn solution 0 lỗi.
