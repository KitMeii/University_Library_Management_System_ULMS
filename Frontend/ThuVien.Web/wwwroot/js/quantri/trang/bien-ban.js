// SPEC 6.2.4 Lập Biên bản vi phạm mượn trả sách (frmLapBienBan – 02.03.01).
import { api } from '../../api.js';
import { LuoiDuLieu } from '../luoi.js';
import { xacNhan, thongBao } from '../hop-thoai.js';
import { dinhDangNgay, dinhDangTien, nhanTrangThai } from '../danh-muc.js';
import { $, taoTrangThai, luuVaIn, baoLoi, hienKetQua, ganNutDong } from '../chung-tu.js';

const NHAP_CT = ['cboKhoanThu', 'cboBanSach', 'txtSoLuong', 'btnThemCT', 'dgvChiTiet', 'cboHTThanhToan'];
const TIM = ['txtSoPhieuMuon', 'btnTimPhieuMuon', 'txtSoBienBan', 'btnTimBienBan', 'dgvTruongHop'];
const tt = taoTrangThai({
  TT1: { bat: TIM, tat: [...NHAP_CT, 'btnIn', 'btnHuy'] },
  TT2: { bat: [...NHAP_CT, 'btnHuy'], tat: [...TIM, 'btnIn'] },
  TT3: { bat: [...NHAP_CT, 'btnIn', 'btnHuy'], tat: TIM }
});

let khoanThu = [];
let s = { soPMuon: null, soBienBan: null, sach: [], dong: [] };

const luoiTruongHop = new LuoiDuLieu($('dgvTruongHop'), {
  khoa: 'soPMuon', duLieuTinh: [], rong: 'Không có trường hợp vi phạm nào chờ lập biên bản.',
  cot: [
    { ten: 'soPMuon', nhan: 'Số phiếu mượn' },
    { ten: 'hoTen', nhan: 'Bạn đọc' },
    { ten: 'soPTra', nhan: 'Phiếu trả' },
    { ten: 'ngayTra', nhan: 'Ngày trả', dinhDang: dinhDangNgay },
    { ten: 'soSach', nhan: 'Số bản', canPhai: true },
    { ten: 'noiDung', nhan: 'Vi phạm', dinhDang: (v) => nhanTrangThai(v, 'qt-nhan-tt--loi') }
  ],
  khiChon: (d) => { $('txtSoPhieuMuon').value = d.soPMuon; },
  khiNhapDup: (d) => { $('txtSoPhieuMuon').value = d.soPMuon; timPhieuMuon(); }
});

const luoiSach = new LuoiDuLieu($('dgvSachViPham'), {
  khoa: 'maBanSach', duLieuTinh: [], rong: 'Chưa nạp trường hợp vi phạm.',
  cot: [
    { ten: 'maBanSach', nhan: 'Mã bản sách' },
    { ten: 'tenSach', nhan: 'Tên sách' },
    { ten: 'tinhTrangTra', nhan: 'Tình trạng khi trả' },
    { ten: 'soNgayQuaHan', nhan: 'Quá hạn', canPhai: true, dinhDang: (v) => `${v} ngày` },
    { ten: 'noiDung', nhan: 'Vi phạm', dinhDang: (v) => nhanTrangThai(v, 'qt-nhan-tt--loi') }
  ],
  khiChon: (d) => { $('cboBanSach').value = d.maBanSach; goiYSoLuong(); }
});

const luoi = new LuoiDuLieu($('dgvChiTiet'), {
  khoa: 'khoa', duLieuTinh: [], rong: 'Chưa có khoản thu. Chọn khoản thu, bản sách, số lượng rồi bấm “Thêm khoản thu”.',
  cot: [
    { ten: 'maBanSach', nhan: 'Mã bản sách' },
    { ten: 'maKhoanThu', nhan: 'Mã khoản thu' },
    { ten: 'noiDungTt', nhan: 'Nội dung' },
    { ten: 'soLuong', nhan: 'Số lượng', canPhai: true },
    { ten: 'donGia', nhan: 'Đơn giá', canPhai: true, dinhDang: dinhDangTien },
    { ten: 'thanhTien', nhan: 'Thành tiền', canPhai: true, dinhDang: dinhDangTien },
    { ten: 'daLuu', nhan: 'Trạng thái', dinhDang: (v, d) => v ? nhanTrangThai(d.doi ? 'Đã lưu – sửa' : 'Đã lưu', 'qt-nhan-tt--ok') : nhanTrangThai('Mới', 'qt-nhan-tt--canh-bao') }
  ],
  khiNhapDup: (d) => suaDong(d)
});

const tongTien = () => s.dong.reduce((t, d) => t + d.thanhTien, 0);

function veLuoi() {
  luoi.tc.duLieuTinh = s.dong;
  luoi.tai();
  $('lblTongTien').textContent = dinhDangTien(tongTien());
  // Trạng thái 3 khi lưới có ít nhất 1 dòng hợp lệ (biên bản đã lưu mở để sửa: trạng thái 3 ngay)
  tt.chuyen(s.dong.length > 0 ? 'TT3' : 'TT2');
}

async function taiTruongHop() {
  try {
    luoiTruongHop.tc.duLieuTinh = await api.lay('/api/quan-tri/bien-ban/truong-hop');
    luoiTruongHop.tai();
  } catch (e) { baoLoi(e); }
}

// ---------- cboKhoanThu ----------
function chonKhoanThu() {
  const k = khoanThu.find((x) => x.maKhoanThu === $('cboKhoanThu').value);
  $('lblDonGia').textContent = k ? dinhDangTien(k.donGia) : '–';
  goiYSoLuong();
}
/** Gợi ý số lượng: khoản phạt theo ngày quá hạn → số ngày quá hạn của bản sách; khoản khác → 1. */
function goiYSoLuong() {
  const k = khoanThu.find((x) => x.maKhoanThu === $('cboKhoanThu').value);
  const b = s.sach.find((x) => x.maBanSach === $('cboBanSach').value);
  if (!k || !b) return;
  $('txtSoLuong').value = /quá hạn/i.test(k.noiDungTt) && b.soNgayQuaHan > 0 ? b.soNgayQuaHan : 1;
}

function napBanSach() {
  const cbo = $('cboBanSach');
  cbo.replaceChildren(...s.sach.map((b) => new Option(`${b.maBanSach} – ${b.noiDung}`, b.maBanSach)));
  luoiSach.tc.duLieuTinh = s.sach;
  luoiSach.tai();
}

function hienBienBan(bb) {
  s.soPMuon = bb.soPMuon;
  s.sach = bb.sachViPham;
  $('txtSoPhieuMuon').value = bb.soPMuon;
  $('txtMaBanDoc').value = bb.banDoc.maBanDoc;
  $('txtHoTen').value = bb.banDoc.hoTen;
  $('txtLopDonVi').value = bb.banDoc.lopDonVi;
  $('cboHTThanhToan').value = bb.htThanhToan;
  napBanSach();
  chonKhoanThu();
}

function xoaTrang() {
  s = { soPMuon: null, soBienBan: null, sach: [], dong: [] };
  for (const id of ['txtSoPhieuMuon', 'txtSoBienBan', 'txtThoiGianLap', 'txtMaBanDoc', 'txtHoTen', 'txtLopDonVi']) $(id).value = '';
  $('txtNguoiLap').value = $('txtNguoiLap').defaultValue;
  $('cboHTThanhToan').value = 'Chưa thanh toán';
  $('txtSoLuong').value = 1;
  $('khuThongTin').hidden = true;
  napBanSach();
  veLuoi();
  tt.chuyen('TT1');
  taiTruongHop();
  $('txtSoPhieuMuon').focus();
}

// ---------- txtSoPhieuMuon, btnTimPhieuMuon ----------
async function timPhieuMuon() {
  const so = $('txtSoPhieuMuon').value.trim().toUpperCase();
  if (!so) { thongBao('Vui lòng nhập số Phiếu mượn sách của trường hợp vi phạm.', 'loi'); $('txtSoPhieuMuon').focus(); return; }
  try {
    const bb = await api.lay('/api/quan-tri/bien-ban/phieu-muon/' + encodeURIComponent(so));
    hienBienBan(bb);
    s.dong = [];
    hienKetQua($('khuThongTin'), { loai: 'loi', tieuDe: `${bb.banDoc.hoTen} – Phiếu mượn sách ${bb.soPMuon}: ${bb.sachViPham.length} bản sách vi phạm.`,
      dong: bb.sachViPham.map((x) => `${x.maBanSach} ${x.tenSach}: ${x.noiDung}`) });
    veLuoi();
  } catch (e) { baoLoi(e); }
}

// ---------- btnTimBienBan ----------
async function timBienBan() {
  const so = $('txtSoBienBan').value.trim().toUpperCase();
  if (!so) { thongBao('Vui lòng nhập số Biên bản vi phạm cần sửa.', 'loi'); $('txtSoBienBan').focus(); return; }
  try {
    const bb = await api.lay('/api/quan-tri/bien-ban/' + encodeURIComponent(so));
    hienBienBan(bb);
    s.soBienBan = bb.soBienBan;
    $('txtSoBienBan').value = bb.soBienBan;
    $('txtThoiGianLap').value = new Date(bb.thoiGianLap).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });
    $('txtNguoiLap').value = bb.tenNd ?? '';
    s.dong = bb.chiTiet.map((d) => ({ ...d, khoa: d.maBanSach + '|' + d.maKhoanThu, daLuu: true, doi: false }));
    hienKetQua($('khuThongTin'), { loai: 'tin', tieuDe: `Đang sửa Biên bản vi phạm ${bb.soBienBan} (${bb.htThanhToan}).`,
      dong: ['Sửa số lượng, thêm khoản thu hoặc ghi nhận thanh toán; dòng đã lưu không bỏ khỏi biên bản được.'] });
    veLuoi();
  } catch (e) { baoLoi(e); }
}

// ---------- btnThemCT ----------
function themCT() {
  const k = khoanThu.find((x) => x.maKhoanThu === $('cboKhoanThu').value);
  const b = s.sach.find((x) => x.maBanSach === $('cboBanSach').value);
  const sl = Number($('txtSoLuong').value);
  if (!k) { thongBao('Vui lòng chọn khoản thu.', 'loi'); return; }
  if (!b) { thongBao('Vui lòng chọn bản sách vi phạm.', 'loi'); return; }
  if (!Number.isInteger(sl) || sl <= 0 || sl > 99999) { thongBao('Số lượng phải là số nguyên lớn hơn 0 (tối đa 5 chữ số).', 'loi'); $('txtSoLuong').focus(); return; }
  if (s.dong.some((d) => d.maBanSach === b.maBanSach && d.maKhoanThu === k.maKhoanThu)) {
    thongBao(`Khoản thu ${k.maKhoanThu} đã có cho bản sách ${b.maBanSach}. Nhấp đúp vào dòng để sửa số lượng.`, 'loi');
    return;
  }
  s.dong.push({ khoa: b.maBanSach + '|' + k.maKhoanThu, maBanSach: b.maBanSach, tenSach: b.tenSach, maKhoanThu: k.maKhoanThu, noiDungTt: k.noiDungTt,
    donGia: k.donGia, soLuong: sl, thanhTien: sl * k.donGia, daLuu: false, doi: false });
  veLuoi();
  luoi.nhanManh(s.dong.length - 1);
}

// ---------- dgvChiTiet (double click dòng i) ----------
async function suaDong(d) {
  const form = document.createElement('div');
  form.innerHTML = `
    <div class="mb-2"><label class="qt-nhan" for="dlgSoLuong">Số lượng</label><input class="qt-o so-lieu" id="dlgSoLuong" type="number" min="1" max="99999" step="1" /></div>
    ${d.daLuu ? '' : '<div class="form-check mt-3"><input class="form-check-input" type="checkbox" id="dlgBoDong" /><label class="form-check-label" for="dlgBoDong">Bỏ dòng này khỏi biên bản (dòng chưa lưu)</label></div>'}`;
  form.querySelector('#dlgSoLuong').value = d.soLuong;
  if (!(await xacNhan({ tieuDe: `${d.maKhoanThu} – ${d.noiDungTt} (${d.maBanSach})`, noiDung: form, nutDongY: 'Cập nhật dòng' }))) return;
  if (form.querySelector('#dlgBoDong')?.checked) s.dong = s.dong.filter((x) => x !== d);
  else {
    const sl = Number(form.querySelector('#dlgSoLuong').value);
    if (!Number.isInteger(sl) || sl <= 0 || sl > 99999) { thongBao('Số lượng phải là số nguyên lớn hơn 0 (tối đa 5 chữ số).', 'loi'); return; }
    if (d.daLuu && sl !== d.soLuong) d.doi = true;
    d.soLuong = sl;
    d.thanhTien = sl * d.donGia;
  }
  veLuoi();
}

// ---------- btnIn ----------
async function inBienBan() {
  try {
    await luuVaIn({
      tieuDe: 'Lưu và in Biên bản vi phạm', tenChungTu: 'Biên bản vi phạm',
      luu: () => api.them('/api/quan-tri/bien-ban', {
        soPMuon: s.soPMuon, soBienBan: s.soBienBan, htThanhToan: $('cboHTThanhToan').value,
        chiTiet: s.dong.map((d) => ({ maKhoanThu: d.maKhoanThu, maBanSach: d.maBanSach, soLuong: d.soLuong }))
      }),
      duongDanIn: (so) => '/quan-tri/in/bien-ban/' + encodeURIComponent(so)
    });
    xoaTrang();
  } catch (e) { baoLoi(e); }
}

$('btnTimPhieuMuon').addEventListener('click', timPhieuMuon);
$('txtSoPhieuMuon').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); timPhieuMuon(); } });
$('btnTimBienBan').addEventListener('click', timBienBan);
$('txtSoBienBan').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); timBienBan(); } });
$('cboKhoanThu').addEventListener('change', chonKhoanThu);
$('cboBanSach').addEventListener('change', goiYSoLuong);
$('btnThemCT').addEventListener('click', themCT);
$('btnIn').addEventListener('click', inBienBan);
$('btnHuy').addEventListener('click', xoaTrang);
ganNutDong();

// frmLapBienBan (Load): trạng thái 1, nạp danh mục khoản thu và trường hợp vi phạm chờ lập biên bản
tt.chuyen('TT1');
luoi.tai();
luoiSach.tai();
try {
  khoanThu = await api.lay('/api/quan-tri/bien-ban/khoan-thu');
  $('cboKhoanThu').replaceChildren(...khoanThu.map((k) => new Option(`${k.maKhoanThu} – ${k.noiDungTt} (${dinhDangTien(k.donGia)})`, k.maKhoanThu)));
  chonKhoanThu();
} catch (e) { baoLoi(e); }
taiTruongHop();
