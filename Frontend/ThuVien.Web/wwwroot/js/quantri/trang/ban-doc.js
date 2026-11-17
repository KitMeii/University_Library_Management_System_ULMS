// SPEC 6.1.3 Danh mục Bạn đọc (frmDanhMucBanDoc) – 01.01.01 Lưu thông tin Thẻ Bạn đọc.
//  (Load): btnKiemTraDK → False
//  txtMaSV, btnTimKiem: thấy → hiển thị Bạn đọc; btnKiemTraDK, btnSua, btnXoa → True
//                        không thấy → thông báo, mở chế độ thêm mới; btnLuu, btnHuy → True; btnKiemTraDK, btnSua, btnXoa → False
//  btnKiemTraDK: hạn thẻ, sách đang mượn/quá hạn, vi phạm chưa thanh toán → hiển thị kết quả điều kiện mượn
import { api, LoiApi } from '../../api.js';
import { FormDanhMuc, nhanTrangThai, dinhDangNgay, dinhDangTien } from '../danh-muc.js';
import { LuoiDuLieu } from '../luoi.js';
import { thongBao } from '../hop-thoai.js';
import { datNgay } from '../../chon-ngay.js';

const $ = (id) => document.getElementById(id);
const coQuyenThem = !!$('btnThem');

const form = new FormDanhMuc({
  api: '/api/quan-tri/ban-doc',
  khoa: 'maBanDoc',
  doiTuong: 'Bạn đọc',
  moTa: (d) => `${d.maBanDoc} – ${d.hoTen}`,
  oTim: ['txtMaSV', 'btnTimKiem'],
  nutThem: ['btnKiemTraDK'],
  ghiChuSauThem: 'Đã tạo tài khoản cổng bạn đọc, chờ kích hoạt.',
  truong: [
    { id: 'txtMaBanDoc', ten: 'maBanDoc', khoa: true },
    { id: 'txtHoTen', ten: 'hoTen' },
    { id: 'cboDoiTuong', ten: 'doiTuong', macDinh: () => 'Sinh viên' },
    { id: 'txtMaSvHvCb', ten: 'maSvHvCb' },
    { id: 'txtLopDonVi', ten: 'lopDonVi' },
    { id: 'dtpNgaySinh', ten: 'ngaySinh' },
    { id: 'txtSoTheBD', ten: 'soTheBd' },
    { id: 'txtDienThoaiEmail', ten: 'dienThoaiEmail' },
    { id: 'dtpHsdTuNgay', ten: 'hsdTuNgay' },
    { id: 'dtpHsdDenNgay', ten: 'hsdDenNgay' },
    { id: 'txtAnh', ten: 'anh' },
    { id: 'fileAnh', ten: 'fileAnh', khongGui: true }
  ],
  luoi: {
    id: 'dgvBanDoc', sapXep: 'MaBanDoc', coTrang: 10,
    cot: [
      { ten: 'maBanDoc', nhan: 'Mã bạn đọc', sapXep: 'MaBanDoc' },
      { ten: 'hoTen', nhan: 'Họ và tên', sapXep: 'HoTen' },
      { ten: 'doiTuong', nhan: 'Đối tượng', sapXep: 'DoiTuong' },
      { ten: 'maSvHvCb', nhan: 'Mã SV/HV/CB', sapXep: 'MaSvHvCb' },
      { ten: 'lopDonVi', nhan: 'Lớp/Đơn vị', sapXep: 'LopDonVi' },
      { ten: 'hsdDenNgay', nhan: 'Hạn thẻ', sapXep: 'HsdDenNgay',
        dinhDang: (v, d) => nhanTrangThai(dinhDangNgay(v) + (d.theConHan ? '' : ' – hết hạn'), d.theConHan ? 'qt-nhan-tt--ok' : 'qt-nhan-tt--loi') }
    ]
  },
  khiChon: (d) => { hienAnh(d.anh); anKetQua(); napTaiKhoanCong(d?.maBanDoc); },
  khiThem: async () => {
    hienAnh(null);
    anKetQua();
    // Gợi ý mã bạn đọc kế tiếp, hạn thẻ mặc định 4 năm
    try { $('txtMaBanDoc').value = (await api.lay('/api/quan-tri/ban-doc/ma-ke-tiep')).maBanDoc; } catch { /* để trống */ }
    const hom = new Date();
    const iso = (d) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    datNgay($('dtpHsdTuNgay'), iso(hom));
    datNgay($('dtpHsdDenNgay'), iso(new Date(hom.getFullYear() + 4, hom.getMonth(), hom.getDate())));
  },
  khiVeTaiForm: () => { hienAnh(null); },
  khiTimKiem: () => timTheoMaSv()
});

// ---------- Ảnh thẻ ----------
function hienAnh(duongDan) {
  $('txtAnh').value = duongDan || '';
  $('imgAnh').hidden = !duongDan;
  if (duongDan) $('imgAnh').src = duongDan;
  $('fileAnh').value = '';
  $('tenTepAnh').textContent = duongDan ? 'Đã có ảnh' : 'Chưa chọn ảnh';
}
$('fileAnh').addEventListener('change', async () => {
  const tep = $('fileAnh').files[0];
  if (!tep) return;
  const fd = new FormData();
  fd.append('tep', tep);
  const res = await fetch('/quan-tri/tai-len/anh-ban-doc', {
    method: 'POST', body: fd, credentials: 'same-origin',
    headers: { 'RequestVerificationToken': document.querySelector('meta[name="csrf-token"]').content }
  });
  const kq = await res.json().catch(() => null);
  if (!res.ok) { thongBao(kq?.thongBao || 'Không tải được ảnh.', 'loi'); $('fileAnh').value = ''; return; }
  hienAnh(kq.duongDan);
});

// ---------- txtMaSV, btnTimKiem ----------
const luoiDangMuon = new LuoiDuLieu($('dgvSachDangMuon'), {
  khoa: 'maBanSach', duLieuTinh: [], rong: 'Bạn đọc không có sách đang mượn.',
  cot: [
    { ten: 'soPMuon', nhan: 'Số phiếu mượn' },
    { ten: 'maBanSach', nhan: 'Mã bản sách' },
    { ten: 'tenSach', nhan: 'Tên sách' },
    { ten: 'ngayMuon', nhan: 'Ngày mượn', dinhDang: dinhDangNgay },
    { ten: 'hanTra', nhan: 'Hạn trả', dinhDang: dinhDangNgay },
    { ten: 'soNgayQuaHan', nhan: 'Quá hạn', dinhDang: (v) => v > 0 ? nhanTrangThai(v + ' ngày', 'qt-nhan-tt--loi') : nhanTrangThai('Trong hạn', 'qt-nhan-tt--ok') }
  ]
});

async function hienSachDangMuon(ds) {
  luoiDangMuon.tc.duLieuTinh = ds;
  $('khuSachDangMuon').hidden = false;
  await luoiDangMuon.tai();
}

async function timTheoMaSv() {
  const ma = $('txtMaSV').value.trim();
  if (!ma) { thongBao('Vui lòng nhập mã SV/HV/CB.', 'tin'); $('txtMaSV').focus(); return; }
  try {
    const kq = await api.lay('/api/quan-tri/ban-doc/tim?maSv=' + encodeURIComponent(ma));
    form.khiChonDong(kq.banDoc);
    await hienSachDangMuon(kq.sachDangMuon);
  } catch (e) {
    if (!(e instanceof LoiApi)) throw e;
    if (e.maHttp !== 404) { thongBao(e.message, 'loi'); return; }
    $('khuSachDangMuon').hidden = true;
    if (coQuyenThem) {
      thongBao(e.message + ' Mở chế độ thêm Bạn đọc mới.', 'tin');
      await form.them();
      $('txtMaSvHvCb').value = ma;
      $('txtHoTen').focus();
    } else thongBao(e.message, 'tin');
  }
}

// ---------- btnKiemTraDK ----------
function anKetQua() { $('khuDieuKien').hidden = true; $('khuDieuKien').replaceChildren(); }
$('btnKiemTraDK').addEventListener('click', async () => {
  const bd = form.chon;
  if (!bd) return;
  try {
    const kq = await api.lay(`/api/quan-tri/ban-doc/${encodeURIComponent(bd.maBanDoc)}/dieu-kien-muon`);
    const k = $('khuDieuKien');
    const div = document.createElement('div');
    div.className = 'qt-ket-qua ' + (kq.duDieuKien ? 'qt-thong-bao--ok' : 'qt-thong-bao--loi');
    div.setAttribute('role', 'status');
    const tieuDe = document.createElement('strong');
    tieuDe.innerHTML = `<i class="bi ${kq.duDieuKien ? 'bi-check-circle-fill' : 'bi-x-octagon-fill'}" aria-hidden="true"></i> `;
    tieuDe.append(kq.duDieuKien ? 'Đủ điều kiện mượn' : 'Không đủ điều kiện mượn');
    const tt = document.createElement('div');
    tt.className = 'small mt-1';
    tt.textContent = `Thẻ ${kq.theConHan ? 'còn hạn' : 'hết hạn'} · Đang mượn ${kq.soDangMuon}/${kq.soBanToiDa} bản` +
      (kq.viPhamChuaThanhToan.length ? ` · Vi phạm chưa thanh toán: ${kq.viPhamChuaThanhToan.map((v) => v.soBienBan + ' (' + dinhDangTien(v.tongTien) + ')').join(', ')}` : '');
    div.append(tieuDe, tt);
    if (kq.lyDo.length) {
      const ul = document.createElement('ul');
      kq.lyDo.forEach((l) => { const li = document.createElement('li'); li.textContent = l; ul.append(li); });
      div.append(ul);
    }
    k.replaceChildren(div);
    k.hidden = false;
    await hienSachDangMuon(kq.sachDangMuon);
  } catch (e) { thongBao(e.message, 'loi'); }
});

// ---------- Mở rộng (file 07 mục 1): khóa/mở khóa tài khoản cổng bạn đọc – chỉ có khi trang render khối này ----------
const khuTk = $('khuTaiKhoanCong');
let tkHienTai = null;
const ngayGioTk = (v) => { if (!v) return 'chưa đăng nhập'; const [d, t] = String(v).split('T'); return `${dinhDangNgay(d)} ${t.slice(0, 5)}`; };
function veTaiKhoanCong(tk) {
  tkHienTai = tk;
  const lop = tk.tinhTrang === 'Hoạt động' ? 'qt-nhan-tt--ok' : tk.tinhTrang === 'Khóa' ? 'qt-nhan-tt--loi' : 'qt-nhan-tt--canh-bao';
  $('lblTinhTrangTaiKhoan').replaceChildren(nhanTrangThai(tk.tinhTrang, lop));
  $('lblDangNhapCuoi').textContent = ngayGioTk(tk.tgDangNhapCuoi);
  const khoa = tk.tinhTrang !== 'Khóa';
  $('btnKhoaTaiKhoanCong').querySelector('span').textContent = khoa ? 'Khóa tài khoản cổng' : 'Mở khóa tài khoản cổng';
  $('btnKhoaTaiKhoanCong').querySelector('i').className = 'bi ' + (khoa ? 'bi-lock' : 'bi-unlock');
  khuTk.hidden = false;
}
async function napTaiKhoanCong(ma) {
  if (!khuTk) return;
  if (!ma) { khuTk.hidden = true; return; }
  try { veTaiKhoanCong(await api.lay(`/api/quan-tri/ban-doc/${encodeURIComponent(ma)}/tai-khoan-cong`)); }
  catch { khuTk.hidden = true; }
}
khuTk && $('btnKhoaTaiKhoanCong').addEventListener('click', async () => {
  if (!tkHienTai) return;
  const khoa = tkHienTai.tinhTrang !== 'Khóa';
  try {
    veTaiKhoanCong(await api.sua(`/api/quan-tri/ban-doc/${encodeURIComponent(tkHienTai.maBanDoc)}/tai-khoan-cong`, { khoa }));
    thongBao(khoa ? 'Đã khóa tài khoản cổng bạn đọc.' : 'Đã mở khóa tài khoản cổng bạn đọc.', 'ok');
  } catch (e) { thongBao(e.message, 'loi'); }
});

await form.batDau();
$('txtMaSV').focus();
