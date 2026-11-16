// Dùng chung cho 4 giao diện lập chứng từ SPEC 6.2 (file 08 mục 3):
//  - 3 trạng thái (SPEC mục 5) qua TrangThaiForm + thanh bước (bước hiện tại màu đỏ – file 03, file 10 mục 4);
//  - btnIn: kiểm soát → lưu trong transaction (server) → hiển thị tiến trình lưu (%) → in (trang /quan-tri/in/…) → về trạng thái 1.
import { TrangThaiForm } from '../form-state.js';
import { motion, giamChuyenDong } from '../motion.config.js';
import { thongBao } from './hop-thoai.js';

export const $ = (id) => document.getElementById(id);

/** TrangThaiForm + cập nhật aria-current của thanh bước. */
export function taoTrangThai(dinhNghia) {
  const tt = new TrangThaiForm(dinhNghia);
  const chuyenGoc = tt.chuyen.bind(tt);
  tt.chuyen = (ten) => {
    chuyenGoc(ten);
    const buoc = document.querySelector('.qt-buoc');
    if (buoc) {
      const thu = Object.keys(dinhNghia).indexOf(ten);
      buoc.querySelectorAll('li').forEach((li, i) => {
        li.toggleAttribute('aria-current', i === thu);
        if (i === thu) li.setAttribute('aria-current', 'step');
        li.classList.toggle('xong', i < thu);
      });
      if (window.gsap && !giamChuyenDong()) {
        const hien = buoc.querySelector('li[aria-current] .qt-buoc__so');
        if (hien) gsap.fromTo(hien, { scale: .85 }, { scale: 1, duration: motion.dur.sm, ease: motion.ease.out });
      }
    }
    return tt;
  };
  return tt;
}

/** Hộp tiến trình lưu (%) – SPEC 6.2.x btnIn “hiển thị tiến trình lưu (%) và thông báo”. */
function moTienTrinh(tieuDe) {
  const nen = document.createElement('div');
  nen.className = 'qt-hop-luu';
  nen.setAttribute('role', 'dialog');
  nen.setAttribute('aria-modal', 'true');
  nen.setAttribute('aria-labelledby', 'tienTrinhTieuDe');
  nen.innerHTML = `<div class="qt-hop-luu__hop">
      <h2 class="qt-hop-luu__tieu-de" id="tienTrinhTieuDe"></h2>
      <div class="qt-hop-luu__thanh" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow="0" aria-labelledby="tienTrinhTieuDe">
        <span></span>
      </div>
      <p class="qt-hop-luu__buoc" aria-live="polite"><span class="qt-hop-luu__pt so-lieu">0%</span> <span class="qt-hop-luu__mo-ta"></span></p>
    </div>`;
  nen.querySelector('h2').textContent = tieuDe;
  document.body.append(nen);
  const thanh = nen.querySelector('[role="progressbar"]');
  let hien = 0;
  const dat = (pt, moTa) => {
    hien = Math.max(hien, Math.min(100, Math.round(pt)));
    thanh.setAttribute('aria-valuenow', String(hien));
    thanh.querySelector('span').style.width = hien + '%';
    nen.querySelector('.qt-hop-luu__pt').textContent = hien + '%';
    if (moTa) nen.querySelector('.qt-hop-luu__mo-ta').textContent = moTa;
  };
  // Trong lúc chờ server: tăng dần tới 85% (không vượt khi chưa có kết quả)
  const nhip = setInterval(() => { if (hien < 85) dat(hien + Math.max(1, (85 - hien) / 8)); }, 120);
  return { dat, dong: () => { clearInterval(nhip); nen.remove(); } };
}

let khungIn = null;
/** In trong khung ẩn (không mở tab mới, không bị chặn popup); trả về sau khi hộp thoại in đóng. */
function inTrongKhung(url) {
  return new Promise((resolve) => {
    khungIn?.remove();
    khungIn = document.createElement('iframe');
    khungIn.className = 'qt-khung-in';
    khungIn.setAttribute('aria-hidden', 'true');
    khungIn.tabIndex = -1;
    khungIn.title = 'Bản in chứng từ';
    khungIn.addEventListener('load', async () => {
      try {
        await khungIn.contentDocument?.fonts?.ready;
        khungIn.contentWindow?.focus();
        khungIn.contentWindow?.print();
      } catch { /* trình duyệt chặn in – vẫn còn liên kết “Mở bản in” */ }
      resolve();
    }, { once: true });
    khungIn.src = url;
    document.body.append(khungIn);
  });
}

/**
 * btnIn: gọi API lưu (một tiến trình), hiển thị %, in chứng từ, thông báo; ghi liên kết “Mở bản in” để in lại.
 * @param {{ tieuDe: string, luu: () => Promise<{so:string, thongBao:string}>, duongDanIn: (so:string) => string, tenChungTu: string }} tc
 */
export async function luuVaIn(tc) {
  const tt = moTienTrinh(tc.tieuDe);
  tt.dat(10, 'Kiểm soát dữ liệu nhập…');
  try {
    const kq = await tc.luu();
    tt.dat(90, 'Đã lưu. Đang chuẩn bị bản in…');
    const url = tc.duongDanIn(kq.so);
    ganLienKetIn(url, `${tc.tenChungTu} ${kq.so}`);
    await inTrongKhung(url);
    tt.dat(100, 'Hoàn tất.');
    await new Promise((r) => setTimeout(r, 250));
    thongBao(kq.thongBao, 'ok');
    return kq;
  } finally {
    tt.dong();
  }
}

function ganLienKetIn(url, nhan) {
  const khu = $('khuInGanNhat');
  if (!khu) return;
  khu.hidden = false;
  const a = khu.querySelector('a');
  a.href = url + '?tuIn=1';
  a.textContent = nhan;
}

/** Hiển thị lỗi API (422 theo trường, 404/409 thông báo) bằng toast. */
export function baoLoi(e) {
  if (e?.name === 'AbortError') return;
  const chiTiet = e?.truong && Object.values(e.truong).length > 1 ? ' ' + Object.values(e.truong).join(' ') : '';
  thongBao((e?.message || 'Có lỗi xảy ra.') + chiTiet, 'loi');
}

/** Ô kết quả (đủ/không đủ điều kiện, không đáp ứng…) dưới khu tìm kiếm. */
export function hienKetQua(el, { loai, tieuDe, dong = [], them = null }) {
  el.hidden = false;
  el.className = 'qt-ket-qua ' + (loai === 'ok' ? 'qt-nhan-tt--ok' : loai === 'tin' ? 'qt-nhan-tt--tin' : 'qt-nhan-tt--loi');
  el.replaceChildren();
  const b = document.createElement('strong');
  b.textContent = tieuDe;
  el.append(b);
  if (dong.length) {
    const ul = document.createElement('ul');
    for (const d of dong) { const li = document.createElement('li'); li.textContent = d; ul.append(li); }
    el.append(ul);
  }
  if (them) el.append(them);
}

export const homNayIso = () => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

/** Số ngày giữa hai ngày yyyy-MM-dd (b − a). */
export const soNgay = (a, b) => Math.round((Date.parse(b + 'T00:00:00Z') - Date.parse(a + 'T00:00:00Z')) / 86400000);

/** btnDong: trở về menu chính (trang tổng quan). */
export function ganNutDong() {
  $('btnDong')?.addEventListener('click', () => { location.href = '/quan-tri'; });
}
