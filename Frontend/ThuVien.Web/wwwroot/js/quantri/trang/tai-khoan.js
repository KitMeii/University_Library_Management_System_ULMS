// SPEC 6.3.2 Quản lý tài khoản người dùng (frmTaiKhoan, 04.00.01).
//  (Load): btnThem, btnTimKiem → True; btnSua, btnKhoa, btnDatLaiMK, btnLuu, btnHuy → False
//  dgvNguoiDung click: btnKhoa hiển thị “Khóa”/“Mở khóa” theo tinhtrang; btnSua, btnKhoa, btnDatLaiMK → True
//  btnThem: tự sinh ma_nd, tinhtrang mặc định “Hoạt động”; btnSua: mở ô trừ ma_nd, tendangnhap
//  btnKhoa, btnDatLaiMK: hỏi xác nhận → ghi nhật ký → về trạng thái Load
import { api } from '../../api.js';
import { FormDanhMuc, nhanTrangThai } from '../danh-muc.js';
import { thongBao, xacNhan } from '../hop-thoai.js';

const $ = (id) => document.getElementById(id);

const nhom = await api.lay('/api/quan-tri/nhom-nguoi-dung');
$('cboNhom').replaceChildren(...nhom.map((n) => new Option(`${n.maNhom} – ${n.tenNhom}`, n.maNhom)));

const form = new FormDanhMuc({
  api: '/api/quan-tri/tai-khoan',
  khoa: 'maNd',
  doiTuong: 'tài khoản',
  moTa: (d) => `${d.maNd} – ${d.tenDangNhap}`,
  nutThem: ['btnKhoa', 'btnDatLaiMK'],
  truong: [
    { id: 'txtMaND', ten: 'maNd', khoa: true, luonChiDoc: true },
    { id: 'txtTenND', ten: 'tenNd' },
    { id: 'txtTenDangNhap', ten: 'tenDangNhap', khoa: true },
    { id: 'txtSDT', ten: 'sdt' },
    { id: 'txtChucVu', ten: 'chucVu' },
    { id: 'cboNhom', ten: 'maNhom', macDinh: () => nhom[0]?.maNhom ?? '' },
    { id: 'cboTinhTrang', ten: 'tinhTrang', macDinh: () => 'Hoạt động' }
  ],
  luoi: {
    id: 'dgvNguoiDung', sapXep: 'MaNd',
    cot: [
      { ten: 'maNd', nhan: 'Mã người dùng', sapXep: 'MaNd' },
      { ten: 'tenNd', nhan: 'Tên người dùng', sapXep: 'TenNd' },
      { ten: 'tenDangNhap', nhan: 'Tên đăng nhập', sapXep: 'TenDangNhap' },
      { ten: 'chucVu', nhan: 'Chức vụ' },
      { ten: 'maNhom', nhan: 'Nhóm', sapXep: 'MaNhom', dinhDang: (v, d) => `${v} – ${d.tenNhom}` },
      { ten: 'tinhTrang', nhan: 'Tình trạng', sapXep: 'TinhTrang',
        dinhDang: (v) => nhanTrangThai(v, v === 'Khóa' ? 'qt-nhan-tt--loi' : 'qt-nhan-tt--ok') }
    ]
  },
  khiChon: (d) => {
    const khoa = d.tinhTrang === 'Khóa';
    $('btnKhoa').querySelector('span').textContent = khoa ? 'Mở khóa' : 'Khóa';
    $('btnKhoa').querySelector('.bi').className = 'bi ' + (khoa ? 'bi-unlock' : 'bi-lock');
  },
  khiThem: async () => {
    try { $('txtMaND').value = (await api.lay('/api/quan-tri/tai-khoan/ma-ke-tiep')).maNd; } catch { /* để trống */ }
  }
});

async function lamMoi(ma) {
  form.veTaiForm();
  await form.luoi.tai();
  const i = form.luoi.dong.findIndex((d) => d.maNd === ma);
  if (i >= 0) form.luoi.nhanManh(i);
}

$('btnKhoa').addEventListener('click', async () => {
  const d = form.chon;
  if (!d) return;
  const khoa = d.tinhTrang !== 'Khóa';
  const ok = await xacNhan({
    tieuDe: khoa ? 'Khóa tài khoản' : 'Mở khóa tài khoản', nguyHiem: khoa, nutDongY: khoa ? 'Khóa' : 'Mở khóa',
    noiDung: khoa ? `Khóa tài khoản ${d.tenDangNhap} (${d.tenNd})? Tài khoản bị khóa sẽ không đăng nhập được.`
                  : `Mở khóa tài khoản ${d.tenDangNhap} (${d.tenNd})?`
  });
  if (!ok) return;
  try {
    await api.them(`/api/quan-tri/tai-khoan/${encodeURIComponent(d.maNd)}/khoa`, {});
    thongBao(`Đã ${khoa ? 'khóa' : 'mở khóa'} tài khoản ${d.tenDangNhap}.`);
    await lamMoi(d.maNd);
  } catch (e) { thongBao(e.message, 'loi'); }
});

$('btnDatLaiMK').addEventListener('click', async () => {
  const d = form.chon;
  if (!d) return;
  const ok = await xacNhan({ tieuDe: 'Đặt lại mật khẩu', nutDongY: 'Đặt lại',
    noiDung: `Đặt lại mật khẩu của tài khoản ${d.tenDangNhap} (${d.tenNd}) về mật khẩu mặc định?` });
  if (!ok) return;
  try {
    await api.them(`/api/quan-tri/tai-khoan/${encodeURIComponent(d.maNd)}/dat-lai-mat-khau`, {});
    thongBao(`Đã đặt lại mật khẩu mặc định cho tài khoản ${d.tenDangNhap}.`);
    await lamMoi(d.maNd);
  } catch (e) { thongBao(e.message, 'loi'); }
});

await form.batDau();
