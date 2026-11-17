"""Sinh Backend/src/BaseCore.ApiGateway/ocelot.json từ bảng định tuyến (KE_HOACH_PHA_0 mục 5).
Chạy lại khi thêm route:  python scripts/sinh_ocelot.py"""
import json, pathlib

GOC = pathlib.Path(__file__).resolve().parents[1]
# (tiền tố upstream, service, cần JWT ở gateway)
ROUTES = [
    ("/api/quan-tri/xac-thuc", "quantri", False),   # đăng nhập NV; đăng xuất/đổi MK service tự kiểm JWT
    ("/api/quan-tri/nhom-nguoi-dung", "quantri", True),
    ("/api/quan-tri/tai-khoan", "quantri", True),
    ("/api/quan-tri/phan-quyen", "quantri", True),
    ("/api/quan-tri/nhat-ky", "quantri", True),
    ("/api/quan-tri/sao-luu", "quantri", True),
    ("/api/quan-tri/thong-ke/n4", "quantri", True),
    ("/api/quan-tri/sach", "khosach", True),
    ("/api/quan-tri/khu-vuc", "khosach", True),
    ("/api/quan-tri/dau-sach", "khosach", True),
    ("/api/quan-tri/the-loai", "khosach", True),
    ("/api/quan-tri/thong-ke/n3", "khosach", True),
    ("/api/cong/sach", "khosach", False),
    ("/api/cong/ke-sach", "khosach", False),
    ("/api/cong/the-loai", "khosach", False),
    ("/api/quan-tri/ban-doc", "muontra", True),
    ("/api/quan-tri/phieu-muon", "muontra", True),
    ("/api/quan-tri/phieu-gia-han", "muontra", True),
    ("/api/quan-tri/phieu-tra", "muontra", True),
    ("/api/quan-tri/dat-cho", "muontra", True),
    ("/api/quan-tri/bao-cao/muon-tra", "muontra", True),
    ("/api/quan-tri/thong-ke/n1", "muontra", True),
    ("/api/cong/dang-nhap", "muontra", False),
    ("/api/cong/kich-hoat", "muontra", False),
    ("/api/cong/quy-dinh", "muontra", False),  # quy định mượn/đặt chỗ công khai (Pha 7)
    ("/api/cong/tai-khoan", "muontra", True),
    ("/api/cong/dat-cho", "muontra", True),
    ("/api/cong/thong-bao", "muontra", True),
    ("/api/cong/yeu-thich", "muontra", True),
    ("/api/quan-tri/khoan-thu", "vipham", True),
    ("/api/quan-tri/bien-ban", "vipham", True),
    ("/api/quan-tri/bao-cao/vi-pham", "vipham", True),
    ("/api/quan-tri/thong-ke/n2", "vipham", True),
]
METHODS = ["GET", "POST", "PUT", "DELETE", "PATCH"]

def route(path, host, auth):
    r = {
        "UpstreamPathTemplate": path,
        "UpstreamHttpMethod": METHODS,
        "DownstreamPathTemplate": path,
        "DownstreamScheme": "http",
        "DownstreamHostAndPorts": [{"Host": host, "Port": 8080}],
    }
    if auth:
        r["AuthenticationOptions"] = {"AuthenticationProviderKeys": ["Bearer"]}
    return r

routes = []
for p, h, a in ROUTES:
    routes.append(route(p, h, a))
    routes.append(route(p + "/{everything}", h, a))
cfg = {"Routes": routes, "GlobalConfiguration": {"RequestIdKey": "X-Request-Id"}}
out = GOC / "Backend/src/BaseCore.ApiGateway/ocelot.json"
out.write_text(json.dumps(cfg, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(len(routes), "routes")
