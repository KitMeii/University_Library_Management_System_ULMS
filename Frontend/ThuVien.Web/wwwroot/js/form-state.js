// Trạng thái nút/ô của form nghiệp vụ khai báo DẠNG DỮ LIỆU (file 08 mục 3), một hàm chuyển trạng thái duy nhất.
// Mỗi trạng thái: { bat: [id], tat: [id], moNhap: [id], chiDoc: [id] }.
//   bat/tat  → Enable = True/False (button, select, lưới…)   moNhap/chiDoc → ReadOnly = False/True (ô nhập)
// Nguyên tắc 5 (file 01): mỗi lúc chỉ bật nút của trạng thái hiện tại (trừ Đóng/Thoát).
import { motion, giamChuyenDong } from './motion.config.js';

export class TrangThaiForm {
  /** @param {Record<string, {bat?:string[], tat?:string[], moNhap?:string[], chiDoc?:string[]}>} dinhNghia */
  constructor(dinhNghia, { tenBuoc = null } = {}) {
    this.dinhNghia = dinhNghia;
    this.hienTai = null;
    this.tenBuoc = tenBuoc; // phần tử thanh bước (form chứng từ – Pha 3)
  }

  chuyen(ten) {
    const tt = this.dinhNghia[ten];
    if (!tt) throw new Error('Trạng thái không tồn tại: ' + ten);
    const vuaBat = [];
    for (const id of tt.bat || []) { if (datBat(id, true)) vuaBat.push(id); }
    for (const id of tt.tat || []) datBat(id, false);
    for (const id of tt.moNhap || []) datChiDoc(id, false);
    for (const id of tt.chiDoc || []) datChiDoc(id, true);
    this.hienTai = ten;
    document.querySelectorAll('[data-trang-thai-form]').forEach((el) => { el.dataset.trangThaiForm = ten; });
    // Control vừa được bật: vòng sáng đỏ nhạt 1 lần 600ms (file 10 mục 4)
    if (!giamChuyenDong() && window.gsap) {
      vuaBat.map((id) => document.getElementById(id)).filter((el) => el && el.tagName === 'BUTTON' && el.offsetParent)
        .forEach((el) => {
          el.classList.add('vua-bat');
          gsap.delayedCall(motion.dur.lg, () => el.classList.remove('vua-bat'));
        });
    }
    return this;
  }
}

/** @returns {boolean} true nếu chuyển từ tắt sang bật */
function datBat(id, bat) {
  const el = document.getElementById(id);
  if (!el || el.hidden) return false;
  const truoc = !(el.disabled || el.getAttribute('aria-disabled') === 'true');
  if (el.matches('button, select, input, textarea')) el.disabled = !bat;
  el.setAttribute('aria-disabled', String(!bat));
  if (el.classList.contains('qt-luoi')) el.classList.toggle('qt-luoi--khoa', !bat);
  return bat && !truoc;
}

function datChiDoc(id, chiDoc) {
  const el = document.getElementById(id);
  if (!el) return;
  if (el.tagName === 'SELECT' || el.type === 'checkbox' || el.type === 'file') {
    el.disabled = chiDoc;
    el.setAttribute('aria-readonly', String(chiDoc));
  } else {
    el.readOnly = chiDoc;
  }
}
