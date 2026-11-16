// SPEC 6.2.3 Lập Phiếu trả sách (frmLapPhieuTra – 01.02.02).
import { api } from '../../api.js';
import { LuoiDuLieu } from '../luoi.js';
import { thongBao } from '../hop-thoai.js';
import { dinhDangNgay, nhanTrangThai } from '../danh-muc.js';
import { layNgay, datNgay } from '../../chon-ngay.js';
import { $, taoTrangThai, luuVaIn, baoLoi, hienKetQua, ganNutDong, homNayIso, soNgay } from '../chung-tu.js';

const tt = taoTrangThai({
  TT1: { bat: ['txtSoPhieuMuon', 'btnTimPhieuMuon', 'txtSoPhieu', 'btnTimPhieu'], tat: ['dgvChiTiet', 'dtpNgayTra', 'btnIn', 'btnHuy'] },
  TT2: { bat: ['dgvChiTiet', 'dtpNgayTra', 'btnHuy'], tat: ['txtSoPhieuMuon', 'btnTimPhieuMuon', 'txtSoPhieu', 'btnTimPhieu', 'btnIn'] },
  TT3: { bat: ['dgvChiTiet', 'dtpNgayTra', 'btnIn', 'btnHuy'], tat: ['txtSoPhieuMuon', 'btnTimPhieuMuon', 'txtSoPhieu', 'btnTimPhieu'] }
});
const TINH_TRANG = ['Bình thường', 'Hư hỏng', 'Mất'];

let s = { soPMuon: null, soPTra: null, cheDo: null, dong: [] };

/** Dòng được gửi lên: dòng mới đánh dấu trả; khi sửa phiếu: dòng đã lưu có thay đổi. */
const dongGui = () => s.dong.filter((d) => (!d.daLuu && d.chon) || (d.daLuu && s.cheDo === 'sua' && d.doi));
const quaHan = (d) => {
  if (d.daLuu && s.cheDo !== 'sua') return d.soNgayQuaHan;
  const ngay = layNgay($('dtpNgayTra'));
  return ngay ? Math.max(0, soNgay(String(d.hanTra).slice(0, 10), ngay)) : 0;
};
const viPham = (d) => quaHan(d) > 0 || d.tinhTrangTra === 'Mất' || d.tinhTrangTra === 'Hư hỏng';

function oViPham(d) {
  const o = document.createElement('span');
  const q = quaHan(d);
  const ds = [];
  if (q > 0) ds.push(`Quá hạn ${q} ngày`);
  if (d.tinhTrangTra === 'Mất' || d.tinhTrangTra === 'Hư hỏng') ds.push(d.tinhTrangTra);
  if ((d.daLuu || d.chon) && ds.length) o.append(nhanTrangThai(ds.join(', '), 'qt-nhan-tt--loi'));
  else if (d.daLuu || d.chon) o.append(nhanTrangThai('Không', 'qt-nhan-tt--ok'));
  d._oViPham = o;
  return o;
}

const moSua = (d) => (!d.daLuu && d.chon) || (d.daLuu && s.cheDo === 'sua' && !d.daLapBienBan);

const luoi = new LuoiDuLieu($('dgvChiTiet'), {
  khoa: 'maBanSach', duLieuTinh: [], rong: 'Chưa nạp Phiếu mượn sách.',
  cot: [
    { ten: 'chon', nhan: 'Trả', dinhDang: (v, d) => {
      if (d.daLuu) return nhanTrangThai(d.daLapBienBan ? 'Đã lập biên bản' : 'Đã trả', 'qt-nhan-tt--tin');
      const c = document.createElement('input');
      c.type = 'checkbox'; c.className = 'form-check-input'; c.checked = d.chon;
      c.setAttribute('aria-label', `Trả bản sách ${d.maBanSach}`);
      c.addEventListener('change', () => { d.chon = c.checked; if (d.chon && !d.tinhTrangTra) d.tinhTrangTra = 'Bình thường'; veLuoi(); });
      return c;
    } },
    { ten: 'maBanSach', nhan: 'Mã bản sách' },
    { ten: 'tenSach', nhan: 'Tên sách' },
    { ten: 'ngayMuon', nhan: 'Ngày mượn', dinhDang: dinhDangNgay },
    { ten: 'hanTra', nhan: 'Hạn trả', dinhDang: dinhDangNgay },
    { ten: 'tinhTrangTra', nhan: 'Tình trạng khi trả', dinhDang: (v, d) => {
      if (!moSua(d)) return v ?? '';
      const sel = document.createElement('select');
      sel.className = 'qt-o qt-o--nho';
      sel.setAttribute('aria-label', `Tình trạng khi trả của ${d.maBanSach}`);
      for (const t of TINH_TRANG) sel.add(new Option(t, t, false, t === v));
      sel.addEventListener('change', () => { d.tinhTrangTra = sel.value; d.doi = true; capNhatTong(); d._oViPham?.replaceWith(oViPham(d)); });
      return sel;
    } },
    { ten: 'ghiChuTra', nhan: 'Ghi chú', dinhDang: (v, d) => {
      if (!moSua(d)) return v ?? '';
      const o = document.createElement('input');
      o.className = 'qt-o qt-o--nho'; o.maxLength = 200; o.value = v ?? '';
      o.setAttribute('aria-label', `Ghi chú khi trả ${d.maBanSach}`);
      o.addEventListener('input', () => { d.ghiChuTra = o.value.trim() || null; d.doi = true; capNhatTong(); });
      return o;
    } },
    { ten: 'soNgayQuaHan', nhan: 'Quá hạn', canPhai: true, dinhDang: (v, d) => (d.daLuu || d.chon) ? `${quaHan(d)} ngày` : '' },
    { ten: 'viPham', nhan: 'Vi phạm', dinhDang: (v, d) => oViPham(d) }
  ]
});

function capNhatTong() {
  const gui = dongGui();
  $('lblSoTra').textContent = String(s.dong.filter((d) => !d.daLuu && d.chon).length);
  $('lblViPham').textContent = String(gui.filter(viPham).length);
  tt.chuyen(gui.length > 0 || (s.cheDo === 'sua' && s.dong.length > 0) ? 'TT3' : 'TT2');
}

function veLuoi() {
  luoi.tc.duLieuTinh = s.dong;
  luoi.tai();
  capNhatTong();
}

function hienPhieu(p) {
  s.soPMuon = p.soPMuon;
  s.soPTra = p.soPTra;
  $('txtSoPhieuMuon').value = p.soPMuon;
  $('txtSoPTra').value = p.soPTra ?? '';
  $('txtNgayLap').value = p.ngayLapPTra ? dinhDangNgay(p.ngayLapPTra) : '';
  $('txtMaBanDoc').value = p.banDoc.maBanDoc;
  $('txtHoTen').value = p.banDoc.hoTen;
  $('txtMaSvHvCb').value = p.banDoc.maSvHvCb;
  if (p.tenNd) $('txtNguoiLap').value = p.tenNd;
  datNgay($('dtpNgayTra'), p.ngayTra);
  s.dong = p.chiTiet.map((d) => ({ ...d, chon: false, doi: false }));
}

function xoaTrang() {
  s = { soPMuon: null, soPTra: null, cheDo: null, dong: [] };
  for (const id of ['txtSoPhieuMuon', 'txtSoPhieu', 'txtSoPTra', 'txtNgayLap', 'txtMaBanDoc', 'txtHoTen', 'txtMaSvHvCb', 'dtpNgayTra']) $(id).value = '';
  $('txtNguoiLap').value = $('txtNguoiLap').defaultValue;
  $('khuThongTin').hidden = true;
  veLuoi();
  tt.chuyen('TT1');
  $('txtSoPhieuMuon').focus();
}

// ---------- txtSoPhieuMuon, btnTimPhieuMuon ----------
async function timPhieuMuon() {
  const so = $('txtSoPhieuMuon').value.trim().toUpperCase();
  if (!so) { thongBao('Vui lòng nhập số Phiếu mượn sách.', 'loi'); $('txtSoPhieuMuon').focus(); return; }
  try {
    const p = await api.lay('/api/quan-tri/phieu-tra/phieu-muon/' + encodeURIComponent(so));
    hienPhieu(p);
    s.cheDo = p.soPTra ? 'bosung' : 'moi';
    datNgay($('dtpNgayTra'), homNayIso());
    const chuaTra = p.chiTiet.filter((d) => !d.daLuu).length;
    hienKetQua($('khuThongTin'), {
      loai: 'tin', tieuDe: `Phiếu mượn sách ${p.soPMuon} – ${p.banDoc.hoTen}: ${chuaTra} bản sách chưa trả.`,
      dong: p.soPTra ? [`Phiếu mượn đã có Phiếu trả sách ${p.soPTra} – bản sách trả lần này được bổ sung vào phiếu đó, không cấp số mới.`] : []
    });
    veLuoi();
  } catch (e) { baoLoi(e); }
}

// ---------- btnTimPhieu ----------
async function timPhieu() {
  const so = $('txtSoPhieu').value.trim().toUpperCase();
  if (!so) { thongBao('Vui lòng nhập số Phiếu trả sách cần sửa.', 'loi'); $('txtSoPhieu').focus(); return; }
  try {
    const p = await api.lay('/api/quan-tri/phieu-tra/' + encodeURIComponent(so));
    hienPhieu(p);
    s.cheDo = 'sua';
    hienKetQua($('khuThongTin'), { loai: 'tin', tieuDe: `Đang sửa Phiếu trả sách ${p.soPTra} (Phiếu mượn ${p.soPMuon}).`,
      dong: ['Sửa tình trạng khi trả, ghi chú, ngày trả. Bản sách đã lập biên bản vi phạm không sửa được.'] });
    veLuoi();
  } catch (e) { baoLoi(e); }
}

// ---------- btnIn ----------
async function inPhieu() {
  const gui = s.cheDo === 'sua' ? s.dong.filter((d) => !d.daLapBienBan) : dongGui();
  if (!gui.length) { thongBao('Vui lòng chọn ít nhất một bản sách trả.', 'loi'); return; }
  try {
    await luuVaIn({
      tieuDe: 'Lưu và in Phiếu trả sách', tenChungTu: 'Phiếu trả sách',
      luu: () => api.them('/api/quan-tri/phieu-tra', {
        soPMuon: s.soPMuon, soPTra: s.cheDo === 'sua' ? s.soPTra : null, ngayTra: layNgay($('dtpNgayTra')) || null,
        chiTiet: gui.map((d) => ({ maBanSach: d.maBanSach, tinhTrangTra: d.tinhTrangTra, ghiChuTra: d.ghiChuTra }))
      }),
      duongDanIn: (so) => '/quan-tri/in/phieu-tra/' + encodeURIComponent(so)
    });
    xoaTrang();
  } catch (e) { baoLoi(e); }
}

$('btnTimPhieuMuon').addEventListener('click', timPhieuMuon);
$('txtSoPhieuMuon').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); timPhieuMuon(); } });
$('btnTimPhieu').addEventListener('click', timPhieu);
$('txtSoPhieu').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); timPhieu(); } });
$('dtpNgayTra').addEventListener('change', () => { if (s.soPMuon) { for (const d of s.dong) if (d.daLuu) d.doi = s.cheDo === 'sua'; veLuoi(); } });
$('btnIn').addEventListener('click', inPhieu);
$('btnHuy').addEventListener('click', xoaTrang);
ganNutDong();

tt.chuyen('TT1');
luoi.tai();
