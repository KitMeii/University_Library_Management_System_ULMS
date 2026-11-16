// Tạo/huỷ biểu đồ ECharts dùng chung (file 09 mục 2): theme “thuvien-do”, hiệu ứng theo motion.config.js (tắt khi giảm chuyển động),
// resize bằng ResizeObserver, trạng thái đang tải (skeleton) / rỗng (“Chưa có dữ liệu trong kỳ”) / lỗi,
// nút “Xem bảng số liệu” (bảng thay thế cho trình đọc màn hình) và “Tải ảnh” (PNG). In báo cáo: SVG renderer, tắt animation.
import { motion, giamChuyenDong } from '../motion.config.js';
import { FONT, BANG_MAU_DO } from './chart-theme.js';

/**
 * Chấm màu trong tooltip theo lớp CSS (thong-ke.css .qt-dau-mau--i) thay cho params.marker của ECharts –
 * marker mặc định là &lt;span style=…&gt; bị CSP style-src 'self' chặn.
 */
export function dauMau(mau) {
  const i = typeof mau === 'string' ? BANG_MAU_DO.indexOf(mau.toUpperCase()) : -1;
  return `<span class="qt-dau-mau${i >= 0 ? ' qt-dau-mau--' + i : ''}" aria-hidden="true"></span>`;
}

// ---------- Định dạng số, tiền, ngày kiểu Việt Nam ----------
export const so = (v) => Number(v ?? 0).toLocaleString('vi-VN');
export const tien = (v) => so(v) + ' đ';
export function tienGon(v) {
  const n = Number(v ?? 0);
  const gon = (x, don) => x.toLocaleString('vi-VN', { maximumFractionDigits: 1 }) + ' ' + don;
  if (Math.abs(n) >= 1e9) return gon(n / 1e9, 'tỷ đ');
  if (Math.abs(n) >= 1e6) return gon(n / 1e6, 'triệu đ');
  if (Math.abs(n) >= 1e3) return gon(n / 1e3, 'nghìn đ');
  return so(n) + ' đ';
}
/** yyyy-MM-dd → dd/MM */
export const ngayNgan = (iso) => `${iso.slice(8, 10)}/${iso.slice(5, 7)}`;
/** yyyy-MM-dd → dd/MM/yyyy */
export const ngayDayDu = (iso) => (iso ? `${iso.slice(8, 10)}/${iso.slice(5, 7)}/${iso.slice(0, 4)}` : '');
/** yyyy-MM → MM/yyyy */
export const thangNam = (ym) => `${ym.slice(5, 7)}/${ym.slice(0, 4)}`;

/** Tham số hiệu ứng ECharts (đổi giây → mili giây); giảm chuyển động thì tắt. */
export function hieuUng() {
  if (giamChuyenDong()) return { animation: false };
  return {
    animation: true,
    animationDuration: motion.chart.in * 1000,
    animationEasing: motion.chart.ease,
    animationDelay: (i) => i * motion.chart.itemDelay * 1000,
    animationDurationUpdate: motion.chart.update * 1000,
    animationEasingUpdate: motion.chart.ease
  };
}

/** Tooltip trục dùng chung: dòng tiêu đề + từng chuỗi “● tên: giá trị”. */
export function tooltipTruc(dinhDang = so) {
  return {
    trigger: 'axis',
    axisPointer: { type: 'shadow', shadowStyle: { color: 'rgba(220,38,38,0.06)' } },
    formatter: (ds) => {
      const dong = (Array.isArray(ds) ? ds : [ds]);
      // Không dùng thuộc tính style (CSP style-src 'self' chặn style nội tuyến)
      const dau = `<b>${dong[0]?.axisValueLabel ?? ''}</b>`;
      return [dau, ...dong.map((d) => `${dauMau(d.color)}${d.seriesName}: <b>${dinhDang(Array.isArray(d.value) ? d.value[1] : d.value)}</b>`)].join('<br>');
    }
  };
}

let demBang = 0;

/**
 * Một biểu đồ trong khung <figure class="qt-bieu-do"> (partial _BieuDo): .qt-bieu-do__khung (vùng vẽ),
 * .qt-bieu-do__rong (trạng thái rỗng/lỗi), .qt-bieu-do__bang (bảng số liệu), nút [data-hanh-dong="bang"|"anh"].
 */
export class BieuDo {
  constructor(hinh) {
    this.hinh = hinh;
    this.khung = hinh.querySelector('.qt-bieu-do__khung');
    this.rong = hinh.querySelector('.qt-bieu-do__rong');
    this.bang = hinh.querySelector('.qt-bieu-do__bang');
    this.nutBang = hinh.querySelector('[data-hanh-dong="bang"]');
    this.nutAnh = hinh.querySelector('[data-hanh-dong="anh"]');
    this.chart = null;
    this.bang.id ||= 'bangSoLieu' + (++demBang);
    this.nutBang?.setAttribute('aria-controls', this.bang.id);
    this.hinh.classList.add('dang-tai');
    this.hinh.setAttribute('aria-busy', 'true');

    this.nutBang?.addEventListener('click', () => {
      const mo = this.bang.hidden;
      this.bang.hidden = !mo;
      this.nutBang.setAttribute('aria-expanded', String(mo));
      this.nutBang.querySelector('span').textContent = mo ? 'Ẩn bảng số liệu' : 'Xem bảng số liệu';
    });
    this.nutAnh?.addEventListener('click', () => {
      if (!this.chart) return;
      const a = document.createElement('a');
      a.href = this.chart.getDataURL({ type: 'png', pixelRatio: 2, backgroundColor: '#FFFFFF' });
      a.download = (this.hinh.dataset.tenTep || this.hinh.id || 'bieu-do') + '.png';
      a.click();
    });
  }

  /**
   * @param {object} option  option ECharts (không gồm theme/hiệu ứng)
   * @param {{ bang?: { cot: string[], dong: (string|number)[][] }, rong?: boolean, ky?: string, moTa?: string }} tc
   */
  ve(option, { bang, rong = false, ky, moTa } = {}) {
    this.hinh.classList.remove('dang-tai');
    this.hinh.removeAttribute('aria-busy');
    const kyEl = this.hinh.querySelector('.qt-bieu-do__ky');
    if (kyEl && ky) kyEl.textContent = ky;
    this.hinh.classList.toggle('la-rong', rong);
    this.rong.hidden = !rong;
    this.rong.textContent = 'Chưa có dữ liệu trong kỳ';
    this.khung.hidden = rong;
    if (this.nutAnh) this.nutAnh.disabled = rong;
    if (rong) {
      this.chart?.dispose();
      this.chart = null;
    } else {
      if (!this.chart) {
        this.chart = echarts.init(this.khung, 'thuvien-do', { renderer: 'canvas' });
        this.quanSat = new ResizeObserver(() => this.chart?.resize());
        this.quanSat.observe(this.khung);
        window.addEventListener('pagehide', () => this.huy(), { once: true });
      }
      this.chart.setOption({
        ...hieuUng(),
        textStyle: { fontFamily: FONT },
        aria: { enabled: true, ...(moTa ? { label: { description: moTa } } : {}) },
        ...option
      }, true);
    }
    if (bang) this.veBang(bang);
  }

  veBang({ cot, dong }) {
    const t = document.createElement('table');
    t.className = 'qt-bang qt-bang--gon';
    const tieuDe = this.hinh.querySelector('.qt-bieu-do__tieu-de')?.textContent ?? '';
    t.innerHTML = '<caption class="sr-only"></caption><thead><tr></tr></thead><tbody></tbody>';
    t.querySelector('caption').textContent = 'Bảng số liệu: ' + tieuDe;
    for (const c of cot) { const th = document.createElement('th'); th.scope = 'col'; th.textContent = c; t.querySelector('thead tr').append(th); }
    for (const d of dong) {
      const tr = document.createElement('tr');
      d.forEach((v, i) => { const td = document.createElement('td'); td.textContent = v; if (i > 0) td.className = 'can-phai so-lieu'; tr.append(td); });
      t.querySelector('tbody').append(tr);
    }
    if (!dong.length) t.querySelector('tbody').innerHTML = `<tr><td colspan="${cot.length}">Chưa có dữ liệu trong kỳ</td></tr>`;
    this.bang.replaceChildren(t);
  }

  loi(thongBao) {
    this.hinh.classList.remove('dang-tai');
    this.hinh.removeAttribute('aria-busy');
    this.khung.hidden = true;
    this.rong.hidden = false;
    this.rong.textContent = thongBao || 'Không tải được số liệu.';
    if (this.nutAnh) this.nutAnh.disabled = true;
  }

  huy() {
    this.quanSat?.disconnect();
    this.chart?.dispose();
    this.chart = null;
  }
}

/** Biểu đồ trên trang in báo cáo (MB06/MB07): SVG renderer – in nét, không vỡ; tắt animation. */
export function veIn(el, option) {
  const c = echarts.init(el, 'thuvien-do', { renderer: 'svg', width: el.clientWidth || 680, height: el.clientHeight || 240 });
  c.setOption({ animation: false, textStyle: { fontFamily: FONT }, aria: { enabled: true }, ...option });
  return c;
}
