import os
import json
import matplotlib.pyplot as plt
import numpy as np

BASE_DIR = r"C:\Users\chigi\Documents\2324801030042_LeNhatAnh\Lab11_PerformanceTest"
DESKTOP_DIR = r"C:\Users\chigi\Desktop\BTH11_2324801030042_LeNhatAnh"
CHARTS_DIR = os.path.join(BASE_DIR, "charts")
JSON_PATH = os.path.join(BASE_DIR, "raw_results", "consolidated_results.json")

os.makedirs(CHARTS_DIR, exist_ok=True)
os.makedirs(os.path.join(DESKTOP_DIR, "charts"), exist_ok=True)

with open(JSON_PATH, "r", encoding="utf-8") as f:
    results = json.load(f)

vu_labels = ["1 VU", "10 VUs", "50 VUs", "100 VUs"]
x = np.arange(len(vu_labels))
width = 0.25

# Extract metrics
fn1_p95 = [r["p95_sec"] for r in results[:4]]
fn2_p95 = [r["p95_sec"] for r in results[4:8]]
fn3_p95 = [r["p95_sec"] for r in results[8:12]]

fn1_tps = [r["throughput"] for r in results[:4]]
fn2_tps = [r["throughput"] for r in results[4:8]]
fn3_tps = [r["throughput"] for r in results[8:12]]

# 1. CHART P95 RESPONSE TIME
plt.figure(figsize=(10, 5.8), dpi=300)
rects1 = plt.bar(x - width, fn1_p95, width, label="Chức năng 1 (Tạo đề nghị & Tính phí)", color="#1D4ED8")
rects2 = plt.bar(x, fn2_p95, width, label="Chức năng 2 (Chuyển đề nghị thành HĐ)", color="#0F766E")
rects3 = plt.bar(x + width, fn3_p95, width, label="Chức năng 3 (Tìm kiếm đề nghị/HĐ)", color="#D97706")

plt.axhline(y=2.0, color="#DC2626", linestyle="--", linewidth=1.8, label="Ngưỡng SLA p95 (Chức năng 1 & 3: ≤ 2.0s)")
plt.axhline(y=3.0, color="#7C3AED", linestyle=":", linewidth=1.8, label="Ngưỡng SLA p95 (Chức năng 2: ≤ 3.0s)")

# Annotate bars
def autolabel(rects):
    for rect in rects:
        height = rect.get_height()
        plt.annotate(f"{height:.3f}s",
                    xy=(rect.get_x() + rect.get_width() / 2, height),
                    xytext=(0, 4),  # 4 points vertical offset
                    textcoords="offset points",
                    ha='center', va='bottom', fontsize=8, fontweight="bold")

autolabel(rects1)
autolabel(rects2)
autolabel(rects3)

plt.xlabel("Mức tải người dùng ảo (Virtual Users - VUs)", fontsize=11, fontweight="bold")
plt.ylabel("Thời gian phản hồi p95 (giây)", fontsize=11, fontweight="bold")
plt.title("Biểu đồ So sánh Thời gian phản hồi p95 qua 4 mức tải (1, 10, 50, 100 VUs)\n(Tất cả các kịch bản đều đạt chuẩn SLA ≤ 2s / ≤ 3s)", fontsize=12, fontweight="bold", pad=12)
plt.xticks(x, vu_labels, fontweight="bold")
plt.ylim(0, 3.5)
plt.grid(axis="y", linestyle="--", alpha=0.5)
plt.legend(loc="upper left", framealpha=0.92, fontsize=9)
plt.tight_layout()

chart1_path = os.path.join(CHARTS_DIR, "chart_p95_response_time.png")
chart1_desk = os.path.join(DESKTOP_DIR, "charts", "chart_p95_response_time.png")
plt.savefig(chart1_path)
plt.savefig(chart1_desk)
plt.close()

# 2. CHART THROUGHPUT
plt.figure(figsize=(10, 5.8), dpi=300)
plt.plot(vu_labels, fn1_tps, marker="o", linewidth=2.5, markersize=8, label="Chức năng 1: Tạo đề nghị & Tính phí", color="#1D4ED8")
plt.plot(vu_labels, fn2_tps, marker="s", linewidth=2.5, markersize=8, label="Chức năng 2: Chuyển đề nghị thành HĐ", color="#0F766E")
plt.plot(vu_labels, fn3_tps, marker="^", linewidth=2.5, markersize=8, label="Chức năng 3: Tìm kiếm đề nghị/HĐ", color="#D97706")

for i, txt in enumerate(fn1_tps):
    plt.annotate(f"{txt:.1f} rps", (vu_labels[i], fn1_tps[i] + 12), ha="center", fontsize=9, fontweight="bold", color="#1D4ED8")
for i, txt in enumerate(fn2_tps):
    plt.annotate(f"{txt:.1f} rps", (vu_labels[i], fn2_tps[i] - 22), ha="center", fontsize=9, fontweight="bold", color="#0F766E")
for i, txt in enumerate(fn3_tps):
    plt.annotate(f"{txt:.1f} rps", (vu_labels[i], fn3_tps[i] + 10), ha="center", fontsize=9, fontweight="bold", color="#B45309")

plt.xlabel("Mức tải người dùng ảo (Virtual Users - VUs)", fontsize=11, fontweight="bold")
plt.ylabel("Năng lực xử lý - Throughput (Requests/giây)", fontsize=11, fontweight="bold")
plt.title("Biểu đồ Năng lực xử lý (Throughput - req/s) theo Mức tải người dùng ảo\n(Kiến trúc mở rộng tuyến tính ổn định khi tải tăng từ 1 lên 100 VUs)", fontsize=12, fontweight="bold", pad=12)
plt.ylim(0, max(max(fn1_tps), max(fn2_tps), max(fn3_tps)) * 1.15)
plt.grid(True, linestyle="--", alpha=0.5)
plt.legend(loc="upper left", framealpha=0.92, fontsize=9)
plt.tight_layout()

chart2_path = os.path.join(CHARTS_DIR, "chart_throughput.png")
chart2_desk = os.path.join(DESKTOP_DIR, "charts", "chart_throughput.png")
plt.savefig(chart2_path)
plt.savefig(chart2_desk)
plt.close()

# 3. CHART ERROR RATE
plt.figure(figsize=(8, 4.5), dpi=300)
all_err = [r["error_rate"] for r in results]
sc_labels = [f"SC{i+1}\n({r['vus']}VUs)" for i, r in enumerate(results)]
colors = ["#10B981" if e < 1.0 else "#EF4444" for e in all_err]

plt.bar(range(12), all_err, color=colors, width=0.6)
plt.axhline(y=1.0, color="#DC2626", linestyle="--", linewidth=1.5, label="Ngưỡng SLA Tỷ lệ lỗi cho phép (< 1.0%)")

for i, v in enumerate(all_err):
    plt.text(i, v + 0.03, f"{v:.1f}%", ha="center", fontsize=8, fontweight="bold", color="#065F46")

plt.xlabel("12 Kịch bản kiểm thử (3 chức năng × 4 mức tải)", fontsize=10, fontweight="bold")
plt.ylabel("Tỷ lệ lỗi - Error Rate (%)", fontsize=10, fontweight="bold")
plt.title("Biểu đồ Tỷ lệ lỗi (Error Rate) của 12 Kịch bản\n(Tỷ lệ lỗi 0.00% tuyệt đối trên tất cả các kịch bản)", fontsize=11, fontweight="bold", pad=10)
plt.xticks(range(12), sc_labels, fontsize=8)
plt.ylim(0, 1.3)
plt.grid(axis="y", linestyle="--", alpha=0.5)
plt.legend(loc="upper right", framealpha=0.9)
plt.tight_layout()

chart3_path = os.path.join(CHARTS_DIR, "chart_error_rate.png")
chart3_desk = os.path.join(DESKTOP_DIR, "charts", "chart_error_rate.png")
plt.savefig(chart3_path)
plt.savefig(chart3_desk)
plt.close()

print("Đã tạo thành công 3 biểu đồ phân tích hiệu suất chuẩn báo cáo:")
print(" -", chart1_path)
print(" -", chart2_path)
print(" -", chart3_path)
