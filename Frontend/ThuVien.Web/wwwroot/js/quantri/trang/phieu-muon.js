// SPEC 6.2.1 Lập Phiếu mượn sách (frmLapPhieuMuon – 01.01.03). Trạng thái nút đúng bảng “Control và xử lý”.
import { api, chuoiTruyVan } from '../../api.js';
import { LuoiDuLieu } from '../luoi.js';
import { xacNhan, thongBao } from '../hop-thoai.js';
import { dinhDangNgay, nhanTrangThai } from '../danh-muc.js';
import { $, taoTrangThai, luuVaIn, baoLoi, hienKetQua, ganNutDong } from '../chung-tu.js';

const tt = taoTrangThai({
  TT1: { bat: ['txtMaSV', 'btnTimBanDoc', 'txtSoPhieu', 'btnTimPhieu'], tat: ['txtMaBanSach', 'btnThemSach', 'dgvChiTiet', 'btnIn', 'btnHuy'] },
  TT2: { bat: ['txtMaBanSach', 'btnThemSach', 'dgvChiTiet', 'btnHuy'], tat: ['txtMaSV', 'btnTimBanDoc', 'txtSoPhieu', 'btnTimPhieu', 'btnIn'] },
  TT3: { bat: ['txtMaBanSach', 'btnThemSach', 'dgvChiTiet', 'btnIn', 'btnHuy'], tat: ['txtMaSV', 'btnTimBanDoc', 'txtSoPhieu', 'btnTimPhieu'] }
});

/** @type {{ banDoc: any, dieuKien: any, cheDo: 'moi'|'bosung'|'sua'|null, soPMuon: string|null, dong: any[] }} */
let s = { banDoc: null, dieuKien: null, cheDo: null, soPMuon: null, dong: [] };

const luoi = new LuoiDuLieu($('dgvChiTiet'), {
  khoa: 'maBanSach', duLieuTinh: [], rong: 'Chưa có bản sách nào. Nhập mã bản sách rồi bấm “Thêm sách”.',
  cot: [
    { ten: 'maBanSach', nhan: 'Mã bản sách' },
    { ten: 'tenSach', nhan: 'Tên sách' },
    { ten: 'viTri', nhan: 'Vị trí lưu trữ' },
    { ten: 'ngayMuon', nhan: 'Ngày mượn', dinhDang: dinhDangNgay },
    { ten: 'hanTra', nhan: 'Hạn trả', dinhDang: dinhDangNgay },
    { ten: 'tinhTrangMuon', nhan: 'Tình trạng khi mượn' },
    { ten: 'ghiChuMuon', nhan: 'Ghi chú' },
    { ten: 'daLuu', nhan: 'Trạng thái', dinhDang: (v, d) => d.daTra ? nhanTrangThai('Đã trả', 'qt-nhan-tt--tin')
        : v ? nhanTrangThai(d.doi ? 'Đã lưu – sửa' : 'Đã lưu', 'qt-nhan-tt--ok') : nhanTrangThai('Mới', 'qt-nhan-tt--canh-bao') }
  ],
  khiNhapDup: (d) => suaDong(d)
});

function veLuoi() {
  luoi.tc.duLieuTinh = s.dong;
  luoi.tai();
  $('lblTongSoBan').textContent = String(s.dong.length);
  const moi = s.dong.filter((d) => !d.daLuu);
  $('lblHanTra').textContent = moi.length ? dinhDangNgay(moi[0].hanTra) : s.dong.length ? dinhDangNgay(s.dong[s.dong.length - 1].hanTra) : '–';
  // Trạng thái 3 khi lưới có ít nhất 1 dòng hợp lệ cần lưu (phiếu đã lưu mở để sửa: trạng thái 3 ngay – SPEC btnTimPhieu)
  const coViec = s.cheDo === 'sua' ? s.dong.length > 0 : moi.length > 0 || s.dong.some((d) => d.doi);
  tt.chuyen(coViec ? 'TT3' : 'TT2');
}

function hienBanDoc(bd) {
  $('txtMaBanDoc').value = bd?.maBanDoc ?? '';
  $('txtHoTen').value = bd?.hoTen ?? '';
  $('txtDoiTuong').value = bd?.doiTuong ?? '';
  $('txtLopDonVi').value = bd?.lopDonVi ?? '';
}

function napPhieu(p, cheDo) {
  s.cheDo = cheDo;
  s.soPMuon = p.soPMuon;
  $('txtSoPMuon').value = p.soPMuon;
  $('txtNgayLap').value = dinhDangNgay(p.ngayLapPMuon);
  $('txtNguoiLap').value = p.tenNd;
  s.dong = p.chiTiet.map((d) => ({ ...d, daLuu: true, doi: false }));
}

/** btnHuy / sau khi in: xóa trắng form, trở về trạng thái 1 (không cấp số phiếu). */
function xoaTrang() {
  s = { banDoc: null, dieuKien: null, cheDo: null, soPMuon: null, dong: [] };
  for (const id of ['txtMaSV', 'txtSoPhieu', 'txtMaBanSach', 'txtSoPMuon', 'txtNgayLap']) $(id).value = '';
  $('txtNguoiLap').value = $('txtNguoiLap').defaultValue;
  hienBanDoc(null);
  $('khuDieuKien').hidden = true;
  $('khuDapUng').hidden = true;
  luoi.tc.duLieuTinh = [];
  luoi.tai();
  $('lblTongSoBan').textContent = '0';
  $('lblHanTra').textContent = '–';
  tt.chuyen('TT1');
  $('txtMaSV').focus();
}

// ---------- txtMaSV, btnTimBanDoc ----------
async function timBanDoc() {
  const maSv = $('txtMaSV').value.trim();
  if (!maSv) { thongBao('Vui lòng nhập mã SV/HV/CB.', 'loi'); $('txtMaSV').focus(); return; }
  try {
    const kq = await api.lay('/api/quan-tri/phieu-muon/tim-ban-doc' + chuoiTruyVan({ maSv }));
    s.banDoc = kq.banDoc;
    s.dieuKien = kq.dieuKien;
    hienBanDoc(kq.banDoc);
    const dk = kq.dieuKien;
    if (!dk.duDieuKien) {
      hienKetQua($('khuDieuKien'), { loai: 'loi', tieuDe: `${kq.banDoc.hoTen} chưa đủ điều kiện mượn sách:`, dong: dk.lyDo });
      return; // giữ trạng thái 1
    }
    const dong = [`Đang mượn ${dk.soDangMuon}/${dk.soBanToiDa} bản; số ngày được mượn: ${kq.soNgayMuon} ngày.`];
    if (kq.phieuTrongNgay) {
      napPhieu(kq.phieuTrongNgay, 'bosung');
      dong.push(`Bạn đọc đã có Phiếu mượn sách ${kq.phieuTrongNgay.soPMuon} lập trong ngày – bản sách mượn thêm được bổ sung vào phiếu này, không cấp số mới.`);
    } else s.cheDo = 'moi';
    hienKetQua($('khuDieuKien'), { loai: 'ok', tieuDe: `${kq.banDoc.hoTen} đủ điều kiện mượn.`, dong });
    veLuoi();
    $('txtMaBanSach').focus();
  } catch (e) { baoLoi(e); }
}

// ---------- btnTimPhieu ----------
async function timPhieu() {
  const so = $('txtSoPhieu').value.trim().toUpperCase();
  if (!so) { thongBao('Vui lòng nhập số Phiếu mượn sách cần sửa.', 'loi'); $('txtSoPhieu').focus(); return; }
  try {
    const p = await api.lay('/api/quan-tri/phieu-muon/' + encodeURIComponent(so));
    s.banDoc = p.banDoc;
    hienBanDoc(p.banDoc);
    napPhieu(p, 'sua');
    hienKetQua($('khuDieuKien'), { loai: 'tin', tieuDe: `Đang sửa Phiếu mượn sách ${p.soPMuon} lập ngày ${dinhDangNgay(p.ngayLapPMuon)}.`,
      dong: ['Sửa tình trạng khi mượn, ghi chú; bản sách đã lưu không bỏ khỏi phiếu được.'] });
    veLuoi();
  } catch (e) { baoLoi(e); }
}

// ---------- txtMaBanSach, btnThemSach ----------
async function themSach(maNhap) {
  const ma = (maNhap ?? $('txtMaBanSach').value).trim().toUpperCase();
  if (!ma) { thongBao('Vui lòng nhập mã bản sách.', 'loi'); $('txtMaBanSach').focus(); return; }
  if (s.dong.some((d) => d.maBanSach === ma)) { thongBao(`Bản sách ${ma} đã có trong phiếu.`, 'loi'); return; }
  const dk = s.dieuKien;
  if (dk && dk.soDangMuon + s.dong.filter((d) => !d.daLuu).length >= dk.soBanToiDa) {
    thongBao(`Đã đủ số bản tối đa (${dk.soBanToiDa} bản) đối với ${s.banDoc.doiTuong.toLowerCase()}.`, 'loi');
    return;
  }
  try {
    const kq = await api.lay('/api/quan-tri/phieu-muon/ban-sach' + chuoiTruyVan({ maBanSach: ma, maBanDoc: s.banDoc.maBanDoc }));
    if (!kq.dapUng) {
      // TT019 thông báo không đáp ứng; TT016/TT017: đề xuất bản sách cùng tên trong kho để lấy/đổi
      let them = null;
      if (kq.banThayThe.length) {
        them = document.createElement('div');
        them.className = 'qt-thay-the';
        const nhan = document.createElement('span');
        nhan.textContent = 'Đổi sang bản sách cùng tên còn sẵn:';
        them.append(nhan);
        for (const b of kq.banThayThe) {
          const nut = document.createElement('button');
          nut.type = 'button';
          nut.className = 'qt-nut qt-nut--vien qt-nut--nho';
          nut.textContent = `${b.maBanSach} – ${b.viTri}`;
          nut.addEventListener('click', () => { $('khuDapUng').hidden = true; themSach(b.maBanSach); });
          them.append(nut);
        }
      }
      hienKetQua($('khuDapUng'), { loai: 'loi', tieuDe: 'Không đáp ứng được bản sách.', dong: [kq.lyDo], them });
      return;
    }
    $('khuDapUng').hidden = true;
    const b = kq.banSach;
    s.dong.push({ maBanSach: b.maBanSach, tenSach: b.tenSach, viTri: b.viTri, ngayMuon: kq.ngayMuon, hanTra: kq.hanTra,
      tinhTrangMuon: 'Bình thường', ghiChuMuon: null, daLuu: false, daTra: false, doi: false });
    $('txtMaBanSach').value = '';
    veLuoi();
    luoi.nhanManh(s.dong.length - 1);
    $('txtMaBanSach').focus();
  } catch (e) { baoLoi(e); }
}

// ---------- dgvChiTiet (double click dòng i) ----------
async function suaDong(d) {
  if (d.daTra) { thongBao(`Bản sách ${d.maBanSach} đã trả, không sửa được.`, 'tin'); return; }
  const form = document.createElement('div');
  form.innerHTML = `
    <div class="mb-3"><label class="qt-nhan" for="dlgTinhTrangMuon">Tình trạng khi mượn<span class="bat-buoc" aria-hidden="true">*</span></label>
      <input class="qt-o" id="dlgTinhTrangMuon" maxlength="50" list="dsTinhTrangMuon" />
      <datalist id="dsTinhTrangMuon"><option value="Bình thường"></option><option value="Bìa hơi cũ"></option><option value="Có vết bút"></option><option value="Gáy sách lỏng"></option></datalist></div>
    <div class="mb-2"><label class="qt-nhan" for="dlgGhiChuMuon">Ghi chú</label><input class="qt-o" id="dlgGhiChuMuon" maxlength="200" /></div>
    ${d.daLuu ? '' : '<div class="form-check mt-3"><input class="form-check-input" type="checkbox" id="dlgBoDong" /><label class="form-check-label" for="dlgBoDong">Bỏ dòng này khỏi phiếu (dòng chưa lưu)</label></div>'}`;
  form.querySelector('#dlgTinhTrangMuon').value = d.tinhTrangMuon ?? '';
  form.querySelector('#dlgGhiChuMuon').value = d.ghiChuMuon ?? '';
  if (!(await xacNhan({ tieuDe: `Bản sách ${d.maBanSach} – ${d.tenSach}`, noiDung: form, nutDongY: 'Cập nhật dòng' }))) return;
  if (form.querySelector('#dlgBoDong')?.checked) {
    s.dong = s.dong.filter((x) => x !== d);
  } else {
    const tinhTrang = form.querySelector('#dlgTinhTrangMuon').value.trim();
    if (!tinhTrang) { thongBao('Tình trạng khi mượn không được để trống.', 'loi'); return; }
    const ghiChu = form.querySelector('#dlgGhiChuMuon').value.trim() || null;
    if (d.daLuu && (tinhTrang !== d.tinhTrangMuon || ghiChu !== d.ghiChuMuon)) d.doi = true;
    d.tinhTrangMuon = tinhTrang;
    d.ghiChuMuon = ghiChu;
  }
  veLuoi();
}

// ---------- btnIn ----------
async function inPhieu() {
  if (!s.dong.length) return;
  try {
    await luuVaIn({
      tieuDe: 'Lưu và in Phiếu mượn sách', tenChungTu: 'Phiếu mượn sách',
      luu: () => api.them('/api/quan-tri/phieu-muon', {
        maBanDoc: s.banDoc.maBanDoc, soPMuon: s.cheDo === 'sua' ? s.soPMuon : null,
        chiTiet: s.dong.filter((d) => !d.daTra).map((d) => ({ maBanSach: d.maBanSach, tinhTrangMuon: d.tinhTrangMuon, ghiChuMuon: d.ghiChuMuon }))
      }),
      duongDanIn: (so) => '/quan-tri/in/phieu-muon/' + encodeURIComponent(so)
    });
    xoaTrang();
  } catch (e) { baoLoi(e); }
}

$('btnTimBanDoc').addEventListener('click', timBanDoc);
$('txtMaSV').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); timBanDoc(); } });
$('btnTimPhieu').addEventListener('click', timPhieu);
$('txtSoPhieu').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); timPhieu(); } });
$('btnThemSach').addEventListener('click', () => themSach());
$('txtMaBanSach').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); themSach(); } });
$('btnIn').addEventListener('click', inPhieu);
$('btnHuy').addEventListener('click', xoaTrang);
ganNutDong();

// frmLapPhieuMuon (Load): trạng thái 1
tt.chuyen('TT1');
luoi.tai();
