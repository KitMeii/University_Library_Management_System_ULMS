// SPEC 6.2.2 Lập Phiếu gia hạn sách (frmLapPhieuGiaHan – 01.02.01).
import { api } from '../../api.js';
import { LuoiDuLieu } from '../luoi.js';
import { xacNhan, thongBao } from '../hop-thoai.js';
import { dinhDangNgay, nhanTrangThai } from '../danh-muc.js';
import { layNgay, datNgay } from '../../chon-ngay.js';
import { $, taoTrangThai, luuVaIn, baoLoi, hienKetQua, ganNutDong, homNayIso } from '../chung-tu.js';

const tt = taoTrangThai({
  TT1: { bat: ['txtSoPhieuMuon', 'btnTimPhieuMuon', 'txtSoPhieu', 'btnTimPhieu'], tat: ['dgvChiTiet', 'dtpNgayDeNghi', 'btnIn', 'btnHuy'] },
  TT2: { bat: ['dgvChiTiet', 'dtpNgayDeNghi', 'btnHuy'], tat: ['txtSoPhieuMuon', 'btnTimPhieuMuon', 'txtSoPhieu', 'btnTimPhieu', 'btnIn'] },
  TT3: { bat: ['dgvChiTiet', 'dtpNgayDeNghi', 'btnIn', 'btnHuy'], tat: ['txtSoPhieuMuon', 'btnTimPhieuMuon', 'txtSoPhieu', 'btnTimPhieu'] }
});

let s = { soPMuon: null, soPGiaHan: null, soNgayGiaHan: null, dong: [] };
let lanKiemTra = 0;

const hopChon = (d) => {
  const c = document.createElement('input');
  c.type = 'checkbox';
  c.className = 'form-check-input';
  c.checked = d.chon;
  c.disabled = d.daLuu; // dòng đã lưu không bỏ khỏi phiếu
  c.setAttribute('aria-label', `Gia hạn bản sách ${d.maBanSach}`);
  c.addEventListener('change', () => { d.chon = c.checked; if (!d.chon) { d.ketQuaGiaHan = null; d.hanTraMoi = null; d.lyDoKhongGh = null; d.tuChoiTay = false; } kiemTra(); });
  return c;
};

const luoi = new LuoiDuLieu($('dgvChiTiet'), {
  khoa: 'maBanSach', duLieuTinh: [], rong: 'Chưa nạp Phiếu mượn sách.',
  cot: [
    { ten: 'chon', nhan: 'Gia hạn', dinhDang: (v, d) => hopChon(d) },
    { ten: 'maBanSach', nhan: 'Mã bản sách' },
    { ten: 'tenSach', nhan: 'Tên sách' },
    { ten: 'hanTraCu', nhan: 'Hạn trả cũ', dinhDang: dinhDangNgay },
    { ten: 'soLanDaGiaHan', nhan: 'Đã gia hạn', canPhai: true, dinhDang: (v) => `${v} lần` },
    { ten: 'ketQuaGiaHan', nhan: 'Kết quả', dinhDang: (v) => !v ? '' : nhanTrangThai(v, v === 'Đồng ý' ? 'qt-nhan-tt--ok' : 'qt-nhan-tt--loi') },
    { ten: 'hanTraMoi', nhan: 'Hạn trả mới', dinhDang: dinhDangNgay },
    { ten: 'lyDoKhongGh', nhan: 'Lý do không gia hạn' }
  ],
  khiNhapDup: (d) => tuChoi(d)
});

function veLuoi() {
  luoi.tc.duLieuTinh = s.dong;
  luoi.tai();
  const chon = s.dong.filter((d) => d.chon && d.ketQuaGiaHan);
  $('lblDongY').textContent = String(chon.filter((d) => d.ketQuaGiaHan === 'Đồng ý').length);
  $('lblKhongDongY').textContent = String(chon.filter((d) => d.ketQuaGiaHan === 'Không đồng ý').length);
  $('lblSoNgayGiaHan').textContent = s.soNgayGiaHan ? `${s.soNgayGiaHan} ngày` : '–';
  tt.chuyen(chon.length > 0 ? 'TT3' : 'TT2');
}

const nhap = () => ({
  soPMuon: s.soPMuon, soPGiaHan: s.soPGiaHan, ngayDeNghi: layNgay($('dtpNgayDeNghi')) || null,
  chiTiet: s.dong.filter((d) => d.chon).map((d) => ({ maBanSach: d.maBanSach, ketQuaGiaHan: d.tuChoiTay ? 'Không đồng ý' : null, lyDoKhongGh: d.tuChoiTay ? d.lyDoKhongGh : null }))
});

/** dgvChiTiet: chọn bản sách + ngay_denghi → server tính số lần đã gia hạn, kiểm tra điều kiện, hạn trả mới / lý do. */
async function kiemTra() {
  const lan = ++lanKiemTra;
  const n = nhap();
  if (!n.chiTiet.length) { veLuoi(); return; }
  if (!n.ngayDeNghi) { thongBao('Vui lòng chọn ngày đề nghị gia hạn.', 'loi'); veLuoi(); return; }
  try {
    const kq = await api.them('/api/quan-tri/phieu-gia-han/kiem-tra', n);
    if (lan !== lanKiemTra) return;
    for (const k of kq) Object.assign(s.dong.find((d) => d.maBanSach === k.maBanSach), {
      ketQuaGiaHan: k.ketQuaGiaHan, hanTraMoi: k.hanTraMoi, lyDoKhongGh: k.lyDoKhongGh, soLanGiaHan: k.soLanGiaHan
    });
  } catch (e) {
    baoLoi(e);
    for (const d of s.dong) if (d.chon && !d.daLuu) { d.ketQuaGiaHan = null; d.hanTraMoi = null; }
  }
  veLuoi();
}

/** Double click dòng đã chọn: ghi “Không đồng ý” + lý do (ketqua_giahan, lydo_khonggh C, E). */
async function tuChoi(d) {
  if (!d.chon) { thongBao('Đánh dấu bản sách cần gia hạn trước.', 'tin'); return; }
  const form = document.createElement('div');
  form.innerHTML = `
    <div class="mb-3"><label class="qt-nhan" for="dlgKetQua">Kết quả gia hạn</label>
      <select class="qt-o" id="dlgKetQua"><option value="Đồng ý">Đồng ý (nếu đủ điều kiện)</option><option value="Không đồng ý">Không đồng ý</option></select></div>
    <div><label class="qt-nhan" for="dlgLyDo">Lý do không gia hạn</label><input class="qt-o" id="dlgLyDo" maxlength="200" /></div>`;
  form.querySelector('#dlgKetQua').value = d.tuChoiTay ? 'Không đồng ý' : 'Đồng ý';
  form.querySelector('#dlgLyDo').value = d.tuChoiTay ? d.lyDoKhongGh ?? '' : '';
  if (!(await xacNhan({ tieuDe: `Bản sách ${d.maBanSach} – ${d.tenSach}`, noiDung: form, nutDongY: 'Cập nhật dòng' }))) return;
  const tuChoiTay = form.querySelector('#dlgKetQua').value === 'Không đồng ý';
  const lyDo = form.querySelector('#dlgLyDo').value.trim();
  if (tuChoiTay && !lyDo) { thongBao('Lý do không gia hạn là bắt buộc khi chọn “Không đồng ý”.', 'loi'); return; }
  d.tuChoiTay = tuChoiTay;
  d.lyDoKhongGh = tuChoiTay ? lyDo : null;
  kiemTra();
}

function hienPhieu(p) {
  s.soPMuon = p.soPMuon;
  s.soNgayGiaHan = p.soNgayGiaHan;
  $('txtSoPhieuMuon').value = p.soPMuon;
  $('txtMaBanDoc').value = p.banDoc.maBanDoc;
  $('txtHoTen').value = p.banDoc.hoTen;
  $('txtMaSvHvCb').value = p.banDoc.maSvHvCb;
  datNgay($('dtpNgayDeNghi'), p.ngayDeNghi);
}

function xoaTrang() {
  s = { soPMuon: null, soPGiaHan: null, soNgayGiaHan: null, dong: [] };
  for (const id of ['txtSoPhieuMuon', 'txtSoPhieu', 'txtSoPGiaHan', 'txtNgayLap', 'txtMaBanDoc', 'txtHoTen', 'txtMaSvHvCb', 'dtpNgayDeNghi']) $(id).value = '';
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
    const p = await api.lay('/api/quan-tri/phieu-gia-han/phieu-muon/' + encodeURIComponent(so));
    hienPhieu(p);
    datNgay($('dtpNgayDeNghi'), homNayIso());
    s.dong = p.chiTiet.map((d) => ({ ...d, chon: false, daLuu: false, tuChoiTay: false }));
    hienKetQua($('khuThongTin'), { loai: 'tin', tieuDe: `Phiếu mượn sách ${p.soPMuon} – ${p.banDoc.hoTen}: ${p.chiTiet.length} bản sách chưa trả.`,
      dong: [`Mỗi bản sách được gia hạn tối đa theo quy định, mỗi lần thêm ${p.soNgayGiaHan} ngày.`] });
    veLuoi();
  } catch (e) { baoLoi(e); }
}

// ---------- btnTimPhieu ----------
async function timPhieu() {
  const so = $('txtSoPhieu').value.trim().toUpperCase();
  if (!so) { thongBao('Vui lòng nhập số Phiếu gia hạn sách cần sửa.', 'loi'); $('txtSoPhieu').focus(); return; }
  try {
    const p = await api.lay('/api/quan-tri/phieu-gia-han/' + encodeURIComponent(so));
    hienPhieu(p);
    s.soPGiaHan = p.soPGiaHan;
    $('txtSoPGiaHan').value = p.soPGiaHan;
    $('txtNgayLap').value = dinhDangNgay(p.ngayLapPGiaHan);
    $('txtNguoiLap').value = p.tenNd ?? '';
    s.dong = p.chiTiet.map((d) => ({ ...d, chon: true, daLuu: true, tuChoiTay: d.ketQuaGiaHan === 'Không đồng ý' && !!d.lyDoKhongGh }));
    hienKetQua($('khuThongTin'), { loai: 'tin', tieuDe: `Đang sửa Phiếu gia hạn sách ${p.soPGiaHan} (Phiếu mượn ${p.soPMuon}).` });
    veLuoi();
    tt.chuyen('TT3');
  } catch (e) { baoLoi(e); }
}

// ---------- btnIn ----------
async function inPhieu() {
  try {
    await luuVaIn({
      tieuDe: 'Lưu và in Phiếu gia hạn sách', tenChungTu: 'Phiếu gia hạn sách',
      luu: () => api.them('/api/quan-tri/phieu-gia-han', nhap()),
      duongDanIn: (so) => '/quan-tri/in/phieu-gia-han/' + encodeURIComponent(so)
    });
    xoaTrang();
  } catch (e) { baoLoi(e); }
}

$('btnTimPhieuMuon').addEventListener('click', timPhieuMuon);
$('txtSoPhieuMuon').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); timPhieuMuon(); } });
$('btnTimPhieu').addEventListener('click', timPhieu);
$('txtSoPhieu').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); timPhieu(); } });
$('dtpNgayDeNghi').addEventListener('change', () => { if (s.soPMuon) kiemTra(); });
$('btnIn').addEventListener('click', inPhieu);
$('btnHuy').addEventListener('click', xoaTrang);
ganNutDong();

tt.chuyen('TT1');
luoi.tai();
