# Cài MediatR (Task #2.1)

Đã thực hiện bằng Package Manager / CLI (tương đương `Install-Package MediatR`):

```bash
# Ví dụ 1 service; đã lặp cho cả 9 Implementation project
dotnet add ProductService/ProductService.csproj package MediatR --version 12.4.1
```

Kiểm tra: mở bất kỳ `*.Implementation.csproj` / `ProductService.csproj`...
sẽ thấy dòng:

```xml
<PackageReference Include="MediatR" Version="12.4.1" />
```

## Đăng ký vào DI container

Đề bài ghi theo mẫu .NET Core cũ (lớp `Startup`):

```csharp
// Startup.cs
using MediatR;

public void ConfigureServices(IServiceCollection services)
{
    services.AddMediatR(); // mẫu cũ
}
```

Trên .NET 8 (không còn `Startup`, dùng minimal API trong `Program.cs`),
cách đúng với MediatR 12.x là:

```csharp
// Program.cs của mỗi Implementation / API host (Phần 3 sẽ tách host riêng)
using MediatR;

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PolicyServiceMarker).Assembly));
```

trong đó `PolicyServiceMarker` là một class bất kỳ trong assembly Implementation
(ví dụ sau này sẽ có `CreateOfferCommandHandler : IRequestHandler<CreateOfferCommand, CreateOfferResult>`).

Phần 2 chỉ yêu cầu **cài package** (đã xong — build 0 lỗi).
Handler CQRS (command/query/notification) và wiring đầy đủ triển khai ở Phần 3.
