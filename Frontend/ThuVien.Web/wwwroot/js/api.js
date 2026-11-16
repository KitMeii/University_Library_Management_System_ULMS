// Gọi API cùng nguồn (/api/** → BFF → gateway). Cookie + AntiForgery header; lỗi theo định dạng { ma, thongBao, truong }.

export class LoiApi extends Error {
  constructor(maHttp, ma, thongBao, truong) {
    super(thongBao);
    this.maHttp = maHttp;
    this.ma = ma;
    this.truong = truong || {};
  }
}

const csrf = () => document.querySelector('meta[name="csrf-token"]')?.content || '';

/**
 * @param {string} method GET | POST | PUT | DELETE
 * @param {string} url    đường dẫn /api/...
 * @param {object} [body]
 * @param {{signal?: AbortSignal}} [tuyChon]
 */
export async function goi(method, url, body, tuyChon = {}) {
  let res;
  try {
    res = await fetch(url, {
      method,
      credentials: 'same-origin',
      signal: tuyChon.signal,
      headers: {
        'Accept': 'application/json',
        ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
        ...(method !== 'GET' ? { 'RequestVerificationToken': csrf() } : {})
      },
      body: body !== undefined ? JSON.stringify(body) : undefined
    });
  } catch (e) {
    if (e.name === 'AbortError') throw e;
    throw new LoiApi(0, 'MAT_KET_NOI', 'Không kết nối được máy chủ. Vui lòng kiểm tra mạng và thử lại.');
  }

  if (res.status === 401) {
    // Cookie đã bị web xóa (BFF) nên trang đăng nhập không quay vòng. Phiên bị hủy sau phục hồi dữ liệu (D36) → báo rõ lý do.
    const loi = (res.headers.get('content-type') || '').includes('json') ? await res.json().catch(() => null) : null;
    const quanTri = location.pathname.startsWith('/quan-tri');
    const thongBao = loi?.ma === 'PHIEN_DA_BI_HUY' ? '&thongBao=' + encodeURIComponent(loi.thongBao) : '';
    location.href = (quanTri ? '/quan-tri/dang-nhap' : '/dang-nhap') + '?returnUrl=' + encodeURIComponent(location.pathname + location.search) + thongBao;
    throw new LoiApi(401, loi?.ma || 'CHUA_DANG_NHAP', loi?.thongBao || 'Phiên đăng nhập đã hết hạn.');
  }
  if (res.status === 204) return null;

  const loai = res.headers.get('content-type') || '';
  const duLieu = loai.includes('json') ? await res.json() : null;
  if (!res.ok) {
    throw new LoiApi(res.status, duLieu?.ma || 'LOI_HE_THONG',
      duLieu?.thongBao || (res.status === 403 ? 'Tài khoản không có quyền thực hiện chức năng này.' : 'Hệ thống gặp sự cố, vui lòng thử lại.'),
      duLieu?.truong);
  }
  return duLieu;
}

export const api = {
  lay: (url, tc) => goi('GET', url, undefined, tc),
  them: (url, body) => goi('POST', url, body),
  sua: (url, body) => goi('PUT', url, body),
  xoa: (url) => goi('DELETE', url)
};

/** Tạo query string bỏ các giá trị rỗng. */
export function chuoiTruyVan(thamSo) {
  const p = new URLSearchParams();
  for (const [k, v] of Object.entries(thamSo)) if (v !== undefined && v !== null && v !== '') p.set(k, v);
  const s = p.toString();
  return s ? '?' + s : '';
}
