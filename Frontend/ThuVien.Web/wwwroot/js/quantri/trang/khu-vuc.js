// SPEC 6.1.4 Danh mục khu vực lưu trữ (frmDanhMucKhuVuc).
import { FormDanhMuc, nhanTrangThai } from '../danh-muc.js';

new FormDanhMuc({
  api: '/api/quan-tri/khu-vuc',
  khoa: 'maKhuVuc',
  doiTuong: 'khu vực',
  moTa: (d) => `${d.maKhuVuc} – ${d.viTri}`,
  truong: [
    { id: 'txtMaKhuVuc', ten: 'maKhuVuc', khoa: true },
    { id: 'txtViTri', ten: 'viTri' },
    { id: 'cboLoaiKhuVuc', ten: 'loaiKhuVuc', macDinh: () => 'Kệ mở' }
  ],
  luoi: {
    id: 'dgvKhuVuc', sapXep: 'MaKhuVuc',
    cot: [
      { ten: 'maKhuVuc', nhan: 'Mã khu vực', sapXep: 'MaKhuVuc' },
      { ten: 'viTri', nhan: 'Vị trí', sapXep: 'ViTri' },
      { ten: 'loaiKhuVuc', nhan: 'Loại khu vực', sapXep: 'LoaiKhuVuc',
        dinhDang: (v) => nhanTrangThai(v, v === 'Kệ mở' ? 'qt-nhan-tt--ok' : 'qt-nhan-tt--tin') },
      { ten: 'soBanSach', nhan: 'Số bản sách', canPhai: true }
    ]
  }
}).batDau();
