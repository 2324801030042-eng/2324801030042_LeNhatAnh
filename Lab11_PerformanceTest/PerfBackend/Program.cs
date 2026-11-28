using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure port: 5050
builder.WebHost.UseUrls("http://127.0.0.1:5050");

var app = builder.Build();

var docLogPath = @"C:\Users\chigi\Documents\2324801030042_LeNhatAnh\Lab11_PerformanceTest\logs\backend_system.log";
var deskLogPath = @"C:\Users\chigi\Desktop\BTH11_2324801030042_LeNhatAnh\logs\backend_system.log";
var logLock = new object();

void WriteLog(string testRunId, string method, string path, int statusCode, double elapsedMs, string detail)
{
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    var line = $"[{timestamp}] [RUN_ID: {testRunId}] [{method}] {path} | Status: {statusCode} | Latency: {elapsedMs:F1}ms | {detail}\n";
    lock (logLock)
    {
        try { File.AppendAllText(docLogPath, line); } catch { }
        try { File.AppendAllText(deskLogPath, line); } catch { }
    }
}

// In-memory data structures simulating PostgreSQL & Elasticsearch
var offers = new ConcurrentDictionary<string, OfferRecord>();
var policies = new ConcurrentDictionary<string, PolicyRecord>();

// Seed initial policies
for (int i = 1; i <= 100; i++)
{
    var pNum = $"POL-2026-{10000 + i}";
    policies[pNum] = new PolicyRecord(
        pNum,
        i % 2 == 0 ? "TRI" : "CAR",
        DateTime.UtcNow.AddDays(-i),
        DateTime.UtcNow.AddYears(1).AddDays(-i),
        i % 3 == 0 ? "Le Nhat Anh" : $"Customer {i}",
        100.0m + i * 5.5m
    );
}

// Middleware for Test Run Tracking
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    var testRunId = context.Request.Headers["X-Test-Run-Id"].FirstOrDefault() ?? "RUN-DEFAULT";
    context.Items["TestRunId"] = testRunId;

    await next();

    sw.Stop();
    var detail = context.Items["LogDetail"] as string ?? "OK";
    WriteLog(testRunId, context.Request.Method, context.Request.Path + context.Request.QueryString, context.Response.StatusCode, sw.Elapsed.TotalMilliseconds, detail);
});

// Chức năng 1: Tạo đề nghị bảo hiểm và tính phí (Create Quote & Calculate Premium)
// POST /api/offers
app.MapPost("/api/offers", async (HttpContext ctx, CreateOfferRequest req) =>
{
    var testRunId = ctx.Items["TestRunId"] as string ?? "RUN-DEFAULT";
    
    // Simulate domain rules & DB computation
    await Task.Delay(Random.Shared.Next(15, 30));

    decimal basePrice = req.ProductCode == "CAR" ? 250.0m : 120.0m;
    int coversCount = req.SelectedCovers?.Count ?? 1;
    decimal totalPrice = basePrice + (coversCount * 25.5m);

    var offerNum = $"OFF-2026-{Random.Shared.Next(100000, 999999)}";
    var offer = new OfferRecord(offerNum, req.ProductCode ?? "TRI", totalPrice, DateTime.UtcNow);
    offers[offerNum] = offer;

    ctx.Items["LogDetail"] = $"Generated Quote: {offerNum}, TotalPremium: ${totalPrice:F2}";
    return Results.Ok(new CreateOfferResponse(offerNum, totalPrice));
});

// Chức năng 2: Chuyển đề nghị thành hợp đồng (Convert Quote to Policy)
// POST /api/policies
app.MapPost("/api/policies", async (HttpContext ctx, CreatePolicyRequest req) =>
{
    var testRunId = ctx.Items["TestRunId"] as string ?? "RUN-DEFAULT";
    
    // Simulate ACID database transaction & event publishing
    await Task.Delay(Random.Shared.Next(25, 45));

    var policyNum = $"POL-2026-{Random.Shared.Next(100000, 999999)}";
    var holderName = $"{req.PolicyHolder?.LastName} {req.PolicyHolder?.FirstName}".Trim();
    if (string.IsNullOrWhiteSpace(holderName)) holderName = "Le Nhat Anh";

    var policy = new PolicyRecord(
        policyNum,
        "TRI",
        DateTime.UtcNow,
        DateTime.UtcNow.AddYears(1),
        holderName,
        145.50m
    );
    policies[policyNum] = policy;

    ctx.Items["LogDetail"] = $"Policy Converted: {policyNum} for {holderName} (Ref: {req.OfferNumber})";
    return Results.Ok(new CreatePolicyResponse(policyNum));
});

// Chức năng 3: Tìm kiếm đề nghị/hợp đồng (Search Quotes & Policies)
// GET /api/PolicySearch?q={queryString}
app.MapGet("/api/PolicySearch", async (HttpContext ctx, string? q) =>
{
    var testRunId = ctx.Items["TestRunId"] as string ?? "RUN-DEFAULT";
    
    // Simulate Elasticsearch query indexing
    await Task.Delay(Random.Shared.Next(10, 25));

    var query = (q ?? string.Empty).Trim().ToLowerInvariant();
    var results = policies.Values
        .Where(p => string.IsNullOrEmpty(query) 
                 || p.PolicyNumber.ToLowerInvariant().Contains(query) 
                 || p.PolicyHolder.ToLowerInvariant().Contains(query)
                 || p.ProductCode.ToLowerInvariant().Contains(query))
        .Take(10)
        .ToList();

    if (!results.Any())
    {
        results = policies.Values.Take(5).ToList();
    }

    ctx.Items["LogDetail"] = $"Search Query '{q}' returned {results.Count} matches";
    return Results.Ok(new FindPolicyResult(results));
});

// Health check
app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }));

app.Run();

// Records & DTOs
record QuestionAnswer(string QuestionCode, string QuestionType, string Answer);
record CreateOfferRequest(string? ProductCode, DateTime PolicyFrom, DateTime PolicyTo, List<string>? SelectedCovers, List<QuestionAnswer>? Answers);
record CreateOfferResponse(string OfferNumber, decimal TotalPrice);

record PersonDto(string FirstName, string LastName, string TaxId);
record AddressDto(string Country, string ZipCode, string City, string Street);
record CreatePolicyRequest(string OfferNumber, PersonDto? PolicyHolder, AddressDto? PolicyHolderAddress);
record CreatePolicyResponse(string PolicyNumber);

record PolicyRecord(string PolicyNumber, string ProductCode, DateTime PolicyStartDate, DateTime PolicyEndDate, string PolicyHolder, decimal PremiumAmount);
record OfferRecord(string OfferNumber, string ProductCode, decimal TotalPrice, DateTime CreatedAt);
record FindPolicyResult(List<PolicyRecord> Policies);
