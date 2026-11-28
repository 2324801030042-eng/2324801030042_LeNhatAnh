import os
import sys
import subprocess
import csv
import json
import time
import matplotlib.pyplot as plt
import numpy as np

BASE_DIR = r"C:\Users\chigi\Documents\2324801030042_LeNhatAnh\Lab11_PerformanceTest"
DESKTOP_DIR = r"C:\Users\chigi\Desktop\BTH11_2324801030042_LeNhatAnh"
SCRIPTS_DIR = os.path.join(BASE_DIR, "scripts")
RAW_RESULTS_DIR = os.path.join(BASE_DIR, "raw_results")
CHARTS_DIR = os.path.join(BASE_DIR, "charts")
LOGS_DIR = os.path.join(BASE_DIR, "logs")

os.makedirs(RAW_RESULTS_DIR, exist_ok=True)
os.makedirs(CHARTS_DIR, exist_ok=True)
os.makedirs(LOGS_DIR, exist_ok=True)

scenarios = [
    # Chức năng 1: Tạo đề nghị và tính phí (SLA: p95 <= 2.0s, Error < 1%)
    {"stt": 1, "fn_id": 1, "name": "Tạo đề nghị và tính phí", "vus": 1, "sla_p95": 2.0},
    {"stt": 2, "fn_id": 1, "name": "Tạo đề nghị và tính phí", "vus": 10, "sla_p95": 2.0},
    {"stt": 3, "fn_id": 1, "name": "Tạo đề nghị và tính phí", "vus": 50, "sla_p95": 2.0},
    {"stt": 4, "fn_id": 1, "name": "Tạo đề nghị và tính phí", "vus": 100, "sla_p95": 2.0},

    # Chức năng 2: Chuyển đề nghị thành hợp đồng (SLA: p95 <= 3.0s, Error < 1%)
    {"stt": 5, "fn_id": 2, "name": "Chuyển đề nghị thành hợp đồng", "vus": 1, "sla_p95": 3.0},
    {"stt": 6, "fn_id": 2, "name": "Chuyển đề nghị thành hợp đồng", "vus": 10, "sla_p95": 3.0},
    {"stt": 7, "fn_id": 2, "name": "Chuyển đề nghị thành hợp đồng", "vus": 50, "sla_p95": 3.0},
    {"stt": 8, "fn_id": 2, "name": "Chuyển đề nghị thành hợp đồng", "vus": 100, "sla_p95": 3.0},

    # Chức năng 3: Tìm kiếm đề nghị/hợp đồng (SLA: p95 <= 2.0s, Error < 1%)
    {"stt": 9, "fn_id": 3, "name": "Tìm kiếm đề nghị/hợp đồng", "vus": 1, "sla_p95": 2.0},
    {"stt": 10, "fn_id": 3, "name": "Tìm kiếm đề nghị/hợp đồng", "vus": 10, "sla_p95": 2.0},
    {"stt": 11, "fn_id": 3, "name": "Tìm kiếm đề nghị/hợp đồng", "vus": 50, "sla_p95": 2.0},
    {"stt": 12, "fn_id": 3, "name": "Tìm kiếm đề nghị/hợp đồng", "vus": 100, "sla_p95": 2.0},
]

summary_log_path = os.path.join(LOGS_DIR, "test_execution_summary.log")
results = []

print("================================================================================")
print("BẮT ĐẦU CHẠY 12 KỊCH BẢN KIỂM CHỨNG HIỆU SUẤT KIẾN TRÚC (BÀI THỰC HÀNH SỐ 11)")
print("Sinh viên: Lê Nhật Anh - MSSV: 2324801030042 - Lớp: D23KTPM01")
print("Hệ thống: InsuranceAgency Case Study (ASP.NET Core Web API / Localhost:5050)")
print("================================================================================")

with open(summary_log_path, "w", encoding="utf-8") as s_log:
    s_log.write(f"=== BẮT ĐẦU PHIÊN KIỂM THỬ HIỆU SUẤT - {time.strftime('%Y-%m-%d %H:%M:%S')} ===\n")

for sc in scenarios:
    stt = sc["stt"]
    fn_id = sc["fn_id"]
    name = sc["name"]
    vus = sc["vus"]
    sla_p95 = sc["sla_p95"]
    test_run_id = f"RUN-2324801030042-SC{stt:02d}-VU{vus:03d}"
    csv_prefix = os.path.join(RAW_RESULTS_DIR, f"scenario_{stt:02d}_vu{vus}")

    print(f"\n[{stt}/12] Đang chạy Kịch bản {stt}: {name} | Mức tải: {vus} VUs | Test Run ID: {test_run_id}")

    env = os.environ.copy()
    env["TARGET_FUNCTION"] = str(fn_id)
    env["TEST_RUN_ID"] = test_run_id

    # Run locust for 10s per scenario
    cmd = [
        sys.executable, "-m", "locust",
        "-f", os.path.join(SCRIPTS_DIR, "locustfile.py"),
        "--headless",
        "-u", str(vus),
        "-r", str(min(vus, 20)),
        "--run-time", "10s",
        "--host", "http://127.0.0.1:5050",
        "--csv", csv_prefix,
        "--only-summary"
    ]

    t_start = time.time()
    res = subprocess.run(cmd, env=env, capture_output=True, text=True)
    t_end = time.time()

    # Read stats CSV
    stats_file = f"{csv_prefix}_stats.csv"
    p95_val = 0.0
    err_rate = 0.0
    throughput = 0.0
    total_reqs = 0
    total_fails = 0

    if os.path.exists(stats_file):
        with open(stats_file, mode="r", encoding="utf-8") as f:
            reader = csv.DictReader(f)
            for row in reader:
                if row.get("Name") == "Aggregated":
                    total_reqs = int(row.get("Request Count", 0))
                    total_fails = int(row.get("Failure Count", 0))
                    err_rate = (total_fails / total_reqs * 100.0) if total_reqs > 0 else 0.0
                    throughput = float(row.get("Requests/s", 0.0))
                    # 95% column in ms
                    p95_ms = float(row.get("95%", 0.0))
                    p95_val = round(p95_ms / 1000.0, 3) # in seconds
                    break

    conclusion = "Đạt" if (p95_val <= sla_p95 and err_rate < 1.0) else "Không đạt"

    row_data = {
        "stt": stt,
        "function_name": name,
        "vus": vus,
        "test_run_id": test_run_id,
        "p95_sec": p95_val,
        "error_rate": round(err_rate, 2),
        "throughput": round(throughput, 2),
        "requests": total_reqs,
        "failures": total_fails,
        "sla_p95": sla_p95,
        "conclusion": conclusion
    }
    results.append(row_data)

    log_entry = (
        f"Kịch bản {stt:02d} | Run ID: {test_run_id} | Chức năng: {name} | VUs: {vus:3d} | "
        f"p95: {p95_val:.3f}s (SLA: <={sla_p95}s) | Error: {err_rate:.2f}% | "
        f"Throughput: {throughput:6.2f} req/s | Kết luận: {conclusion}\n"
    )
    print("   -> " + log_entry.strip())
    with open(summary_log_path, "a", encoding="utf-8") as s_log:
        s_log.write(log_entry)

    time.sleep(1)

# Write consolidated CSV
csv_out_path = os.path.join(RAW_RESULTS_DIR, "consolidated_results.csv")
with open(csv_out_path, "w", newline="", encoding="utf-8-sig") as f:
    writer = csv.writer(f)
    writer.writerow(["STT", "Chức năng", "Mức tải (VUs)", "Test Run ID", "p95 Response Time (giây)", "Error Rate (%)", "Throughput (req/s)", "Kết luận"])
    for r in results:
        writer.writerow([r["stt"], r["function_name"], r["vus"], r["test_run_id"], f"{r['p95_sec']:.3f}", f"{r['error_rate']:.2f}%", f"{r['throughput']:.2f}", r["conclusion"]])

# Write JSON
json_out_path = os.path.join(RAW_RESULTS_DIR, "consolidated_results.json")
with open(json_out_path, "w", encoding="utf-8") as f:
    json.dump(results, f, indent=2, ensure_ascii=False)

print("\nĐã lưu bảng kết quả tổng hợp vào:")
print(f" - CSV: {csv_out_path}")
print(f" - JSON: {json_out_path}")

# GENERATE VISUALIZATION CHARTS
print("\nĐang tạo các biểu đồ trực quan hóa số liệu...")
vu_labels = ["1 VU", "10 VUs", "50 VUs", "100 VUs"]
x = np.arange(len(vu_labels))
width = 0.25

fn1_p95 = [r["p95_sec"] for r in results if r["fn_id"] == 1]
fn2_p95 = [r["p95_sec"] for r in results if r["fn_id"] == 2]
fn3_p95 = [r["p95_sec"] for r in results if r["fn_id"] == 3]

fn1_tps = [r["throughput"] for r in results if r["fn_id"] == 1]
fn2_tps = [r["throughput"] for r in results if r["fn_id"] == 2]
fn3_tps = [r["throughput"] for r in results if r["fn_id"] == 3]

# 1. Chart p95 Response Time
plt.figure(figsize=(10, 5.5), dpi=300)
plt.bar(x - width, fn1_p95, width, label="Chức năng 1 (Tạo đề nghị & Tính phí)", color="#1E40AF")
plt.bar(x, fn2_p95, width, label="Chức năng 2 (Chuyển đề nghị thành HĐ)", color="#0D9488")
plt.bar(x + width, fn3_p95, width, label="Chức năng 3 (Tìm kiếm đề nghị/HĐ)", color="#F59E0B")

plt.axhline(y=2.0, color="#DC2626", linestyle="--", linewidth=1.5, label="Ngưỡng SLA p95 (Chức năng 1 & 3: ≤ 2.0s)")
plt.axhline(y=3.0, color="#7C3AED", linestyle=":", linewidth=1.5, label="Ngưỡng SLA p95 (Chức năng 2: ≤ 3.0s)")

plt.xlabel("Mức tải người dùng ảo (Virtual Users - VUs)", fontsize=11, fontweight="bold")
plt.ylabel("p95 Response Time (giây)", fontsize=11, fontweight="bold")
plt.title("Biểu đồ so sánh Thời gian phản hồi p95 qua 4 mức tải (1, 10, 50, 100 VUs)", fontsize=13, fontweight="bold", pad=12)
plt.xticks(x, vu_labels, fontweight="bold")
plt.ylim(0, 3.5)
plt.grid(axis="y", linestyle="--", alpha=0.5)
plt.legend(loc="upper left", framealpha=0.9)
plt.tight_layout()
chart_p95_path = os.path.join(CHARTS_DIR, "chart_p95_response_time.png")
plt.savefig(chart_p95_path)
plt.close()

# 2. Chart Throughput
plt.figure(figsize=(10, 5.5), dpi=300)
plt.plot(vu_labels, fn1_tps, marker="o", linewidth=2.5, markersize=8, label="Chức năng 1: Tạo đề nghị & Tính phí", color="#1E40AF")
plt.plot(vu_labels, fn2_tps, marker="s", linewidth=2.5, markersize=8, label="Chức năng 2: Chuyển đề nghị thành HĐ", color="#0D9488")
plt.plot(vu_labels, fn3_tps, marker="^", linewidth=2.5, markersize=8, label="Chức năng 3: Tìm kiếm đề nghị/HĐ", color="#F59E0B")

for i, txt in enumerate(fn1_tps):
    plt.annotate(f"{txt:.1f}", (vu_labels[i], fn1_tps[i] + 3), ha="center", fontsize=9, fontweight="bold", color="#1E40AF")
for i, txt in enumerate(fn2_tps):
    plt.annotate(f"{txt:.1f}", (vu_labels[i], fn2_tps[i] - 12), ha="center", fontsize=9, fontweight="bold", color="#0D9488")
for i, txt in enumerate(fn3_tps):
    plt.annotate(f"{txt:.1f}", (vu_labels[i], fn3_tps[i] + 5), ha="center", fontsize=9, fontweight="bold", color="#B45309")

plt.xlabel("Mức tải người dùng ảo (Virtual Users - VUs)", fontsize=11, fontweight="bold")
plt.ylabel("Throughput (Requests/giây)", fontsize=11, fontweight="bold")
plt.title("Biểu đồ Năng lực xử lý (Throughput) theo mức tải người dùng ảo", fontsize=13, fontweight="bold", pad=12)
plt.grid(True, linestyle="--", alpha=0.5)
plt.legend(loc="upper left", framealpha=0.9)
plt.tight_layout()
chart_tps_path = os.path.join(CHARTS_DIR, "chart_throughput.png")
plt.savefig(chart_tps_path)
plt.close()

print(f"Đã tạo thành công biểu đồ:")
print(f" - {chart_p95_path}")
print(f" - {chart_tps_path}")

# Copy to Desktop folder for easy submission
print("\nĐồng bộ hóa kết quả và biểu đồ sang thư mục Desktop...")
subprocess.run(["robocopy", BASE_DIR, DESKTOP_DIR, "/E", "/NFL", "/NDL", "/NJH", "/NJS"], shell=True)
print("Hoàn tất!")
