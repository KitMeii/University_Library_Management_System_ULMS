// Kỳ báo cáo MB06/MB07 (SPEC 6.4: chọn ngày, không gõ tay – TT058): dtpTuNgay, dtpDenNgay + chọn nhanh; đọc kỳ từ URL
// (?ky=hom-nay|7-ngay|thang-nay|thang-truoc hoặc ?tuNgay=yyyy-MM-dd&denNgay=yyyy-MM-dd).
import { layNgay, datNgay } from '../chon-ngay.js';
import { thongBao } from './hop-thoai.js';

const $ = (id) => document.getElementById(id);
const p2 = (n) => String(n).padStart(2, '0');
export const iso = (d) => `${d.getFullYear()}-${p2(d.getMonth() + 1)}-${p2(d.getDate())}`;

/** Khoảng ngày của một lựa chọn nhanh. */
export function kyNhanh(loai, homNay = new Date()) {
  const y = homNay.getFullYear(), m = homNay.getMonth();
  switch (loai) {
    case 'hom-nay': return [iso(homNay), iso(homNay)];
    case '7-ngay': return [iso(new Date(y, m, homNay.getDate() - 6)), iso(homNay)];
    case 'thang-truoc': return [iso(new Date(y, m - 1, 1)), iso(new Date(y, m, 0))];
    default: return [iso(new Date(y, m, 1)), iso(homNay)]; // tháng này
  }
}

const NGAY_ISO = /^\d{4}-\d{2}-\d{2}$/;

/**
 * Gắn ô kỳ + nút chọn nhanh; trả về true nếu URL đã chỉ định kỳ (trang tự tổng hợp/xem ngay).
 * Mặc định: tháng này.
 */
export function ganKy({ tuNgay, denNgay } = {}) {
  const ts = new URLSearchParams(location.search);
  const tu = tuNgay || ts.get('tuNgay'), den = denNgay || ts.get('denNgay');
  let coSan = false;
  let nhanh = 'thang-nay';
  if (tu && den && NGAY_ISO.test(tu) && NGAY_ISO.test(den)) { datNgay($('dtpTuNgay'), tu); datNgay($('dtpDenNgay'), den); nhanh = null; coSan = true; }
  else {
    if (ts.get('ky')) { nhanh = ts.get('ky'); coSan = true; }
    const [a, b] = kyNhanh(nhanh);
    datNgay($('dtpTuNgay'), a);
    datNgay($('dtpDenNgay'), b);
  }
  const nut = document.querySelectorAll('[data-ky]');
  const danhDau = (k) => nut.forEach((b) => b.setAttribute('aria-pressed', String(b.dataset.ky === k)));
  danhDau(nhanh);
  nut.forEach((b) => b.addEventListener('click', () => {
    const [a, c] = kyNhanh(b.dataset.ky);
    datNgay($('dtpTuNgay'), a);
    datNgay($('dtpDenNgay'), c);
    danhDau(b.dataset.ky);
    b.dispatchEvent(new CustomEvent('ky-nhanh', { bubbles: true }));
  }));
  ['dtpTuNgay', 'dtpDenNgay'].forEach((id) => $(id).addEventListener('change', () => danhDau(null)));
  return coSan;
}

/** Kiểm soát dữ liệu nhập kỳ ở trình duyệt (server kiểm tra lại); trả về {tuNgay, denNgay} hoặc null. */
export function docKy() {
  const tu = layNgay($('dtpTuNgay')), den = layNgay($('dtpDenNgay'));
  let loi = null;
  if (!tu) loi = ['dtpTuNgay', 'Vui lòng chọn từ ngày của kỳ báo cáo.'];
  else if (!den) loi = ['dtpDenNgay', 'Vui lòng chọn đến ngày của kỳ báo cáo.'];
  else if (tu > den) loi = ['dtpTuNgay', 'Từ ngày phải nhỏ hơn hoặc bằng đến ngày.'];
  else if (den > iso(new Date())) loi = ['dtpDenNgay', 'Đến ngày không được sau ngày hiện tại.'];
  ['dtpTuNgay', 'dtpDenNgay'].forEach((id) => $(id).removeAttribute('aria-invalid'));
  if (loi) {
    $(loi[0]).setAttribute('aria-invalid', 'true');
    thongBao(loi[1], 'loi');
    return null;
  }
  return { tuNgay: tu, denNgay: den };
}

/** Lỗi 422 của server theo trường → đánh dấu ô kỳ. */
export function baoLoiKy(e) {
  const truong = e?.truong || {};
  if (truong.tuNgay) $('dtpTuNgay').setAttribute('aria-invalid', 'true');
  if (truong.denNgay) $('dtpDenNgay').setAttribute('aria-invalid', 'true');
  const ct = Object.values(truong);
  thongBao(ct.length ? ct.join(' ') : (e?.message || 'Có lỗi xảy ra.'), 'loi');
}
