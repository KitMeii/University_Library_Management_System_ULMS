// Bộ máy chung cho các giao diện danh mục SPEC 6.1.x và 6.3.2 (cùng mẫu control/nút):
//   frm…(Load): ô nhập ReadOnly; btnThem, btnTimKiem → True; btnSua, btnXoa, btnLuu, btnHuy → False
//   dgv… click dòng: đổ lên ô (chỉ đọc); btnSua, btnXoa → True
//   btnThem: xóa trắng, mở ô; btnLuu, btnHuy → True; btnThem, btnSua, btnXoa, btnTimKiem → False
//   btnSua: mở ô trừ mã; (như btnThem)          btnXoa: hỏi xác nhận → xóa → về trạng thái Load
//   btnLuu: kiểm soát dữ liệu → thêm/cập nhật → thông báo → tải lại lưới → về trạng thái Load
//   btnHuy: bỏ thao tác, ô chỉ đọc → trạng thái Load      btnDong: về màn hình chính
// Nhóm chỉ có quyền R: btnThem, btnSua, btnXoa, btnLuu → Visible = False (server không render).
import { api, LoiApi } from '../api.js';
import { TrangThaiForm } from '../form-state.js';
import { LuoiDuLieu } from './luoi.js';
import { xacNhan, thongBao } from './hop-thoai.js';
import { motion, giamChuyenDong } from '../motion.config.js';
import { laOChonNgay, layNgay, datNgay } from '../chon-ngay.js';

const $ = (id) => document.getElementById(id);

/**
 * @typedef {{ id: string, ten: string, khoa?: boolean, macDinh?: () => any, loai?: 'chu'|'so'|'ngay', khongGui?: boolean }} TruongCfg
 */
export class FormDanhMuc {
  /**
   * @param {{ api: string, khoa: string, doiTuong: string, truong: TruongCfg[],
   *           luoi: { id: string, cot: any[], sapXep?: string, coTrang?: number, thamSoThem?: () => object },
   *           moTa: (dong:any) => string, nutThem?: string[], timKiem?: boolean }} cfg
   */
  constructor(cfg) {
    this.cfg = { timKiem: true, nutThem: [], ...cfg };
    this.chon = null;
    this.cheDo = null; // 'them' | 'sua' | null
    // luonChiDoc: ô tự sinh (vd ma_nd) – chỉ đọc cả khi thêm; khoa: không sửa khi btnSua
    const o = this.cfg.truong.map((t) => t.id);
    const oThem = this.cfg.truong.filter((t) => !t.luonChiDoc).map((t) => t.id);
    const oSua = this.cfg.truong.filter((t) => !t.khoa && !t.luonChiDoc).map((t) => t.id);
    const oKhoa = this.cfg.truong.filter((t) => t.khoa || t.luonChiDoc).map((t) => t.id);
    const oLuon = this.cfg.truong.filter((t) => t.luonChiDoc).map((t) => t.id);
    // Ô + nút tìm kiếm (mặc định txtTimKiem, btnTimKiem; 6.1.3 dùng txtMaSV, btnTimKiem)
    const tim = this.cfg.timKiem ? (this.cfg.oTim ?? ['txtTimKiem', 'btnTimKiem']) : [];
    this.oTim = tim[0];
    const them = this.cfg.nutThem; // nút riêng của từng giao diện, bật khi đã chọn dòng
    this.trangThai = new TrangThaiForm({
      TaiForm: { bat: ['btnThem', ...tim, this.cfg.luoi.id], tat: ['btnSua', 'btnXoa', 'btnLuu', 'btnHuy', ...them], chiDoc: o },
      ChonDong: { bat: ['btnThem', ...tim, this.cfg.luoi.id, 'btnSua', 'btnXoa', ...them], tat: ['btnLuu', 'btnHuy'], chiDoc: o },
      Them: { bat: ['btnLuu', 'btnHuy'], tat: ['btnThem', 'btnSua', 'btnXoa', ...tim, this.cfg.luoi.id, ...them], moNhap: oThem, chiDoc: oLuon },
      Sua: { bat: ['btnLuu', 'btnHuy'], tat: ['btnThem', 'btnSua', 'btnXoa', ...tim, this.cfg.luoi.id, ...them], moNhap: oSua, chiDoc: oKhoa },
    });
    this.luoi = new LuoiDuLieu($(this.cfg.luoi.id), {
      url: this.cfg.api, cot: this.cfg.luoi.cot, khoa: this.cfg.khoa, sapXep: this.cfg.luoi.sapXep,
      coTrang: this.cfg.luoi.coTrang, thamSoThem: this.cfg.luoi.thamSoThem,
      khiChon: (d) => this.khiChonDong(d)
    });
  }

  async batDau() {
    $('btnThem')?.addEventListener('click', () => this.them());
    $('btnSua')?.addEventListener('click', () => this.sua());
    $('btnXoa')?.addEventListener('click', () => this.xoa());
    $('btnLuu')?.addEventListener('click', () => this.luu());
    $('btnHuy')?.addEventListener('click', () => this.huy());
    $('btnDong')?.addEventListener('click', () => { location.href = '/quan-tri'; });
    const tim = () => (this.cfg.khiTimKiem ? this.cfg.khiTimKiem() : this.timKiem());
    $('btnTimKiem')?.addEventListener('click', tim);
    if (this.oTim) $(this.oTim)?.addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); tim(); } });
    $('frmChiTiet')?.addEventListener('submit', (e) => { e.preventDefault(); if (this.cheDo) this.luu(); });
    this.trangThai.chuyen('TaiForm');
    await this.luoi.tai();
  }

  // ---------- dữ liệu ô nhập ----------
  doVao(dong) {
    for (const t of this.cfg.truong) {
      const el = $(t.id);
      if (!el) continue;
      const v = dong ? dong[t.ten] : (t.macDinh ? t.macDinh() : '');
      if (el.type === 'checkbox') el.checked = !!v;
      else if (laOChonNgay(el)) datNgay(el, v ?? '');
      else el.value = v ?? '';
    }
  }

  layRa() {
    const kq = {};
    for (const t of this.cfg.truong) {
      if (t.khongGui) continue;
      const el = $(t.id);
      if (!el) continue;
      let v = el.type === 'checkbox' ? el.checked : laOChonNgay(el) ? layNgay(el) : el.value.trim();
      if (t.loai === 'so') v = v === '' ? null : Number(String(v).replace(/\./g, '').replace(',', '.'));
      if (v === '') v = null;
      kq[t.ten] = v;
    }
    return kq;
  }

  xoaLoi() {
    document.querySelectorAll('.qt-loi-truong[data-tu-dong]').forEach((e) => e.remove());
    for (const t of this.cfg.truong) $(t.id)?.removeAttribute('aria-invalid');
    $('thongBaoForm')?.replaceChildren();
  }

  hienLoi(loi) {
    let dau = null;
    for (const [truong, thongBaoLoi] of Object.entries(loi.truong || {})) {
      const t = this.cfg.truong.find((x) => x.ten.toLowerCase() === truong.toLowerCase());
      const el = t && $(t.id);
      if (!el) continue;
      el.setAttribute('aria-invalid', 'true');
      const d = document.createElement('div');
      d.className = 'qt-loi-truong';
      d.dataset.tuDong = '';
      d.id = el.id + 'Loi';
      d.innerHTML = '<i class="bi bi-exclamation-circle" aria-hidden="true"></i><span></span>';
      d.querySelector('span').textContent = thongBaoLoi;
      el.setAttribute('aria-describedby', d.id);
      (el.closest('.qt-truong') || el.parentElement).append(d);
      dau ??= el;
      // Lỗi nhập liệu: ô rung ngang 6px 0.3s (chỉ trục x)
      if (window.gsap && !giamChuyenDong())
        gsap.fromTo(el, { x: 0 }, { x: 6, duration: 0.05, repeat: 5, yoyo: true, ease: 'none', clearProps: 'transform' });
    }
    if (!dau || Object.keys(loi.truong || {}).length > 1) thongBao(loi.message, 'loi');
    dau?.focus();
  }

  // ---------- xử lý control ----------
  khiChonDong(dong) {
    this.chon = dong;
    this.xoaLoi();
    this.doVao(dong);
    this.trangThai.chuyen('ChonDong');
    this.cfg.khiChon?.(dong);
  }

  them() {
    this.cheDo = 'them';
    this.xoaLoi();
    this.luoi.boChon();
    this.doVao(null);
    this.trangThai.chuyen('Them');
    this.cfg.khiThem?.();
    $(this.cfg.truong.find((t) => !t.luonChiDoc && !$(t.id)?.readOnly && !$(t.id)?.disabled)?.id)?.focus();
  }

  sua() {
    if (!this.chon) return;
    this.cheDo = 'sua';
    this.xoaLoi();
    this.trangThai.chuyen('Sua');
    $(this.cfg.truong.find((t) => !t.khoa && !t.luonChiDoc)?.id)?.focus();
  }

  async xoa() {
    if (!this.chon) return;
    const ok = await xacNhan({
      tieuDe: `Xóa ${this.cfg.doiTuong}`,
      noiDung: `Bạn có chắc muốn xóa ${this.cfg.doiTuong} ${this.cfg.moTa(this.chon)}? Thao tác này không hoàn tác được.`,
      nutDongY: 'Xóa', nguyHiem: true
    });
    if (!ok) return;
    try {
      await api.xoa(`${this.cfg.api}/${encodeURIComponent(this.chon[this.cfg.khoa])}`);
      thongBao(`Đã xóa ${this.cfg.doiTuong} ${this.cfg.moTa(this.chon)}.`);
      this.veTaiForm();
      await this.luoi.tai();
    } catch (e) {
      if (e instanceof LoiApi) thongBao(e.message, 'loi'); else throw e;
    }
  }

  async luu() {
    if (!this.cheDo) return;
    const nut = $('btnLuu');
    nut.disabled = true;
    nut.setAttribute('aria-busy', 'true');
    this.xoaLoi();
    const duLieu = this.layRa();
    try {
      const kq = this.cheDo === 'them'
        ? await api.them(this.cfg.api, duLieu)
        : await api.sua(`${this.cfg.api}/${encodeURIComponent(this.chon[this.cfg.khoa])}`, duLieu);
      const ma = kq?.[this.cfg.khoa] ?? duLieu[this.cfg.khoa];
      thongBao(this.cheDo === 'them'
        ? `Đã thêm ${this.cfg.doiTuong} ${this.cfg.moTa(kq ?? duLieu)}.${this.cfg.ghiChuSauThem ? ' ' + this.cfg.ghiChuSauThem : ''}`
        : `Đã cập nhật ${this.cfg.doiTuong} ${this.cfg.moTa(kq ?? duLieu)}.`);
      this.veTaiForm();
      await this.luoi.tai();
      const i = this.luoi.dong.findIndex((d) => d[this.cfg.khoa] === ma);
      if (i >= 0) this.luoi.nhanManh(i);
    } catch (e) {
      if (!(e instanceof LoiApi)) throw e;
      nut.disabled = false;
      this.hienLoi(e);
    } finally {
      nut.removeAttribute('aria-busy');
    }
  }

  huy() {
    this.xoaLoi();
    this.veTaiForm();
  }

  /** Về trạng thái khi mở form: ô chỉ đọc, bỏ chọn dòng, xóa trắng ô. */
  veTaiForm() {
    this.cheDo = null;
    this.chon = null;
    this.luoi.boChon();
    this.doVao(null);
    this.trangThai.chuyen('TaiForm');
    this.cfg.khiVeTaiForm?.();
  }

  async timKiem() {
    const k = $('txtTimKiem')?.value.trim() ?? '';
    this.veTaiForm();
    await this.luoi.timKiem(k);
    if (k && this.luoi.tongSo === 0) thongBao('Không tìm thấy kết quả phù hợp từ khóa “' + k + '”.', 'tin');
  }
}

/** Định dạng tiền: 1.234.000 đ (file 01 mục 5). */
export const dinhDangTien = (v) => (v === null || v === undefined) ? '' : Number(v).toLocaleString('vi-VN') + ' đ';
/** Định dạng ngày dd/MM/yyyy từ chuỗi yyyy-MM-dd. */
export const dinhDangNgay = (v) => {
  if (!v) return '';
  const [y, m, d] = String(v).slice(0, 10).split('-');
  return `${d}/${m}/${y}`;
};
export const nhanTrangThai = (v, lop) => {
  const s = document.createElement('span');
  s.className = 'qt-nhan-tt ' + (lop || '');
  s.textContent = v;
  return s;
};
