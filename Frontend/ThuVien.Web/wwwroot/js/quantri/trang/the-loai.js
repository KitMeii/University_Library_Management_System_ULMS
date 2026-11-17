// Quản lý thể loại (X01 – mở rộng, cổng bạn đọc). Cùng mẫu nút/ô với danh mục SPEC 6.1.x (FormDanhMuc).
import { FormDanhMuc } from '../danh-muc.js';

const $ = (id) => document.getElementById(id);

function bieuTuong(v) {
  const i = document.createElement('i');
  i.className = 'bi ' + (/^bi-[a-z0-9-]+$/.test(v || '') ? v : 'bi-bookmark');
  i.setAttribute('aria-hidden', 'true');
  return i;
}
const xemBieuTuong = () => { $('iBieuTuong').className = bieuTuong($('txtBieuTuong').value.trim()).className; };
$('txtBieuTuong').addEventListener('input', xemBieuTuong);

new FormDanhMuc({
  api: '/api/quan-tri/the-loai',
  khoa: 'maTheLoai',
  doiTuong: 'thể loại',
  moTa: (d) => `${d.maTheLoai} – ${d.tenTheLoai}`,
  truong: [
    { id: 'txtMaTheLoai', ten: 'maTheLoai', khoa: true },
    { id: 'txtTenTheLoai', ten: 'tenTheLoai' },
    { id: 'txtBieuTuong', ten: 'bieuTuong', macDinh: () => 'bi-book' },
    { id: 'txtThuTuHienThi', ten: 'thuTuHienThi', loai: 'so', macDinh: () => 0 },
    { id: 'txtMoTa', ten: 'moTa' }
  ],
  luoi: {
    id: 'dgvTheLoai', sapXep: 'ThuTuHienThi',
    cot: [
      { ten: 'bieuTuong', nhan: '', dinhDang: (v) => bieuTuong(v) },
      { ten: 'maTheLoai', nhan: 'Mã thể loại', sapXep: 'MaTheLoai' },
      { ten: 'tenTheLoai', nhan: 'Tên thể loại', sapXep: 'TenTheLoai' },
      { ten: 'moTa', nhan: 'Mô tả' },
      { ten: 'thuTuHienThi', nhan: 'Thứ tự', sapXep: 'ThuTuHienThi', canPhai: true },
      { ten: 'soDauSach', nhan: 'Số đầu sách', canPhai: true }
    ]
  },
  khiChon: () => xemBieuTuong(),
  khiThem: () => xemBieuTuong(),
  khiVeTaiForm: () => xemBieuTuong()
}).batDau();
