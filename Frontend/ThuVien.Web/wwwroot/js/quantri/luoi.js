// DataGrid dùng chung cho các control dgv… (file 03 mục D, file 08 mục 3):
// phân trang + sắp xếp phía server, chọn dòng (chuột/bàn phím), double click = “double click vào dòng” trong SPEC,
// skeleton khi tải, trạng thái rỗng, “yêu cầu mới nhất thắng” (AbortController + requestId).
import { api, chuoiTruyVan } from '../api.js';
import { motion, giamChuyenDong } from '../motion.config.js';

/**
 * @typedef {{ ten: string, nhan: string, sapXep?: string, canPhai?: boolean, dinhDang?: (v:any, dong:any) => string|Node }} Cot
 */
export class LuoiDuLieu {
  /**
   * @param {HTMLElement} el      phần tử .qt-luoi có id = tên control (vd dgvKhuVuc)
   * @param {{ url?: string, cot: Cot[], khoa: string, coTrang?: number, sapXep?: string, thamSoThem?: () => object,
   *           duLieuTinh?: any[], khiChon?: (dong:any)=>void, khiNhapDup?: (dong:any)=>void, rong?: string }} tc
   */
  constructor(el, tc) {
    this.el = el;
    this.tc = { ...tc, coTrang: tc.coTrang ?? 15, rong: tc.rong ?? 'Không có dữ liệu.' };
    this.trang = 1;
    this.sapXep = tc.sapXep || null;
    this.tang = true;
    this.tuKhoa = '';
    this.dong = [];
    this.tongSo = 0;
    this.chon = null;
    this.lanTai = 0;
    this.daTaiLanDau = false;
    this.dungYeuCau = null;
    this.veKhung();
  }

  veKhung() {
    const tieuDe = this.el.getAttribute('aria-label') || '';
    this.el.innerHTML = `
      <div class="qt-luoi__cuon" tabindex="-1">
        <table class="qt-bang" aria-label="${tieuDe}">
          <thead><tr></tr></thead>
          <tbody></tbody>
        </table>
      </div>
      <div class="qt-luoi__chan">
        <span class="qt-luoi__dem so-lieu" aria-live="polite"></span>
        <nav class="qt-luoi__trang" aria-label="Phân trang ${tieuDe}"></nav>
      </div>`;
    const tr = this.el.querySelector('thead tr');
    for (const c of this.tc.cot) {
      const th = document.createElement('th');
      th.scope = 'col';
      if (c.canPhai) th.classList.add('can-phai');
      if (c.sapXep) {
        const b = document.createElement('button');
        b.type = 'button';
        b.className = 'qt-bang__sap-xep';
        b.innerHTML = `<span></span><i class="bi bi-arrow-down-up" aria-hidden="true"></i>`;
        b.querySelector('span').textContent = c.nhan;
        b.addEventListener('click', () => {
          if (this.dangKhoa()) return;
          this.tang = this.sapXep === c.sapXep ? !this.tang : true;
          this.sapXep = c.sapXep;
          this.trang = 1;
          this.tai();
        });
        th.append(b);
        th.dataset.sapXep = c.sapXep;
      } else th.textContent = c.nhan;
      tr.append(th);
    }
    this.tbody = this.el.querySelector('tbody');
    this.tbody.addEventListener('click', (e) => this.xuLyChon(e));
    this.tbody.addEventListener('dblclick', (e) => {
      if (e.target.matches('input, select, textarea, button, label')) return;
      window.getSelection()?.removeAllRanges(); // nhấp đúp = mở dòng, không bôi đen chữ
      const tr = e.target.closest('tr[data-chi-so]');
      if (tr && !this.dangKhoa() && this.tc.khiNhapDup) this.tc.khiNhapDup(this.dong[+tr.dataset.chiSo]);
    });
    this.tbody.addEventListener('keydown', (e) => {
      const tr = e.target.closest('tr[data-chi-so]');
      if (!tr || e.target.matches('input, select, textarea, button')) return; // ô nhập trong dòng (lưới chi tiết chứng từ)
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); this.xuLyChon(e); }
      else if (e.key === 'ArrowDown') { e.preventDefault(); tr.nextElementSibling?.focus(); }
      else if (e.key === 'ArrowUp') { e.preventDefault(); tr.previousElementSibling?.focus(); }
    });
  }

  dangKhoa() { return this.el.getAttribute('aria-disabled') === 'true'; }

  xuLyChon(e) {
    const tr = e.target.closest('tr[data-chi-so]');
    if (!tr || this.dangKhoa()) return;
    this.chonDong(+tr.dataset.chiSo);
  }

  chonDong(chiSo) {
    this.tbody.querySelectorAll('tr[aria-selected="true"]').forEach((t) => t.setAttribute('aria-selected', 'false'));
    const tr = this.tbody.querySelector(`tr[data-chi-so="${chiSo}"]`);
    if (!tr) return;
    tr.setAttribute('aria-selected', 'true');
    this.chon = this.dong[chiSo];
    this.tc.khiChon?.(this.chon);
  }

  boChon() {
    this.chon = null;
    this.tbody.querySelectorAll('tr[aria-selected="true"]').forEach((t) => t.setAttribute('aria-selected', 'false'));
  }

  timKiem(tuKhoa) { this.tuKhoa = tuKhoa; this.trang = 1; return this.tai(); }

  /** Tải lại; nếu có khoaChon thì chọn lại dòng đó và nhấn nhẹ “hàng mới” (nền --red-50 nhạt dần). */
  async tai({ khoaChon = null, nhanManh = false } = {}) {
    if (this.tc.duLieuTinh) { this.ve(this.tc.duLieuTinh, this.tc.duLieuTinh.length); return; }
    this.dungYeuCau?.abort();
    const dk = new AbortController();
    this.dungYeuCau = dk;
    const lan = ++this.lanTai;
    this.el.setAttribute('aria-busy', 'true');
    if (!this.daTaiLanDau) this.veSkeleton();
    try {
      const ts = { tuKhoa: this.tuKhoa, trang: this.trang, coTrang: this.tc.coTrang, sapXep: this.sapXep, tang: this.tang,
        ...(this.tc.thamSoThem?.() || {}) };
      const kq = await api.lay(this.tc.url + chuoiTruyVan(ts), { signal: dk.signal });
      if (lan !== this.lanTai) return;
      this.ve(kq.duLieu, kq.tongSo);
      if (khoaChon !== null) {
        const i = this.dong.findIndex((d) => d[this.tc.khoa] === khoaChon);
        if (i >= 0) {
          this.chonDong(i);
          if (nhanManh) this.nhanManh(i);
        }
      }
    } catch (e) {
      if (e.name === 'AbortError') return;
      this.veLoi(e.message);
    } finally {
      if (lan === this.lanTai) this.el.removeAttribute('aria-busy');
    }
  }

  ve(dong, tongSo) {
    this.dong = dong;
    this.tongSo = tongSo;
    this.chon = null;
    this.tbody.replaceChildren();
    if (!dong.length) {
      const tr = document.createElement('tr');
      tr.className = 'qt-bang__rong';
      tr.innerHTML = `<td colspan="${this.tc.cot.length}"><i class="bi bi-inbox" aria-hidden="true"></i><span></span></td>`;
      tr.querySelector('span').textContent = this.tuKhoa ? 'Không tìm thấy kết quả phù hợp từ khóa.' : this.tc.rong;
      this.tbody.append(tr);
    }
    dong.forEach((d, i) => {
      const tr = document.createElement('tr');
      tr.dataset.chiSo = i;
      tr.tabIndex = 0;
      tr.setAttribute('aria-selected', 'false');
      for (const c of this.tc.cot) {
        const td = document.createElement('td');
        if (c.canPhai) td.classList.add('can-phai', 'so-lieu');
        const v = c.dinhDang ? c.dinhDang(d[c.ten], d) : d[c.ten];
        if (v instanceof Node) td.append(v); else td.textContent = v ?? '';
        tr.append(td);
      }
      this.tbody.append(tr);
    });
    this.el.querySelectorAll('th[data-sap-xep]').forEach((th) => {
      const dang = th.dataset.sapXep === this.sapXep;
      th.setAttribute('aria-sort', dang ? (this.tang ? 'ascending' : 'descending') : 'none');
      th.querySelector('.bi').className = 'bi ' + (dang ? (this.tang ? 'bi-sort-down-alt' : 'bi-sort-down') : 'bi-arrow-down-up');
    });
    this.vePhanTrang();
    // Lần tải đầu: hàng stagger (file 10 mục 4)
    if (!this.daTaiLanDau && window.gsap && !giamChuyenDong() && dong.length) {
      gsap.from(this.tbody.children, { opacity: 0, y: 6, duration: motion.dur.sm, stagger: motion.dashboard.rowStagger,
        ease: motion.ease.out, clearProps: 'opacity,transform' });
    }
    this.daTaiLanDau = true;
  }

  nhanManh(i) {
    const tr = this.tbody.querySelector(`tr[data-chi-so="${i}"]`);
    if (!tr) return;
    tr.classList.add('vua-luu');
    if (window.gsap) gsap.delayedCall(1.2, () => tr.classList.remove('vua-luu'));
    tr.scrollIntoView({ block: 'nearest' });
  }

  veSkeleton() {
    this.tbody.replaceChildren();
    for (let i = 0; i < 5; i++) {
      const tr = document.createElement('tr');
      tr.className = 'qt-bang__khung';
      tr.innerHTML = this.tc.cot.map(() => '<td><span class="qt-skeleton"></span></td>').join('');
      this.tbody.append(tr);
    }
  }

  veLoi(thongBao) {
    this.tbody.replaceChildren();
    const tr = document.createElement('tr');
    tr.className = 'qt-bang__rong qt-bang__loi';
    tr.innerHTML = `<td colspan="${this.tc.cot.length}"><i class="bi bi-exclamation-octagon" aria-hidden="true"></i><span></span>
      <button type="button" class="qt-nut qt-nut--phu qt-nut--nho">Thử lại</button></td>`;
    tr.querySelector('span').textContent = thongBao;
    tr.querySelector('button').addEventListener('click', () => this.tai());
    this.tbody.append(tr);
  }

  vePhanTrang() {
    if (this.tc.duLieuTinh) {
      // Dữ liệu tĩnh hiển thị đủ mọi dòng (không phân trang)
      this.el.querySelector('.qt-luoi__dem').textContent = `${this.tongSo.toLocaleString('vi-VN')} dòng`;
      this.el.querySelector('.qt-luoi__trang').replaceChildren();
      return;
    }
    const soTrang = Math.max(1, Math.ceil(this.tongSo / this.tc.coTrang));
    const tu = this.tongSo ? (this.trang - 1) * this.tc.coTrang + 1 : 0;
    const den = Math.min(this.trang * this.tc.coTrang, this.tongSo);
    this.el.querySelector('.qt-luoi__dem').textContent = `${tu.toLocaleString('vi-VN')}–${den.toLocaleString('vi-VN')} / ${this.tongSo.toLocaleString('vi-VN')} dòng`;
    const nav = this.el.querySelector('.qt-luoi__trang');
    nav.replaceChildren();
    if (this.tc.duLieuTinh || soTrang <= 1) return;
    const nut = (nhan, trang, tat, hienTai = false) => {
      const b = document.createElement('button');
      b.type = 'button';
      b.className = 'qt-trang';
      b.innerHTML = nhan;
      b.disabled = tat;
      if (hienTai) b.setAttribute('aria-current', 'page');
      b.addEventListener('click', () => { if (!this.dangKhoa()) { this.trang = trang; this.tai(); } });
      return b;
    };
    nav.append(nut('<i class="bi bi-chevron-left" aria-hidden="true"></i><span class="sr-only">Trang trước</span>', this.trang - 1, this.trang <= 1));
    const ds = new Set([1, soTrang, this.trang - 1, this.trang, this.trang + 1].filter((t) => t >= 1 && t <= soTrang));
    let truoc = 0;
    [...ds].sort((a, b) => a - b).forEach((t) => {
      if (t - truoc > 1) { const s = document.createElement('span'); s.className = 'qt-trang__cach'; s.textContent = '…'; nav.append(s); }
      nav.append(nut(String(t), t, false, t === this.trang));
      truoc = t;
    });
    nav.append(nut('<i class="bi bi-chevron-right" aria-hidden="true"></i><span class="sr-only">Trang sau</span>', this.trang + 1, this.trang >= soTrang));
  }
}
