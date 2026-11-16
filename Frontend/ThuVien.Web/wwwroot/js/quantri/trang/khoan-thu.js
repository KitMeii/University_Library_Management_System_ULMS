// SPEC 6.1.2 Danh mục khoản thu (frmDanhMucKhoanThu).
import { FormDanhMuc, dinhDangTien } from '../danh-muc.js';

const form = new FormDanhMuc({
  api: '/api/quan-tri/khoan-thu',
  khoa: 'maKhoanThu',
  doiTuong: 'khoản thu',
  moTa: (d) => `${d.maKhoanThu} – ${d.noiDungTt}`,
  truong: [
    { id: 'txtMaKhoanThu', ten: 'maKhoanThu', khoa: true },
    { id: 'txtNoiDungTT', ten: 'noiDungTt' },
    { id: 'txtDonGia', ten: 'donGia', loai: 'so' }
  ],
  luoi: {
    id: 'dgvKhoanThu', sapXep: 'MaKhoanThu',
    cot: [
      { ten: 'maKhoanThu', nhan: 'Mã khoản thu', sapXep: 'MaKhoanThu' },
      { ten: 'noiDungTt', nhan: 'Nội dung thanh toán', sapXep: 'NoiDungTt' },
      { ten: 'donGia', nhan: 'Đơn giá', sapXep: 'DonGia', canPhai: true, dinhDang: dinhDangTien }
    ]
  }
});
// Hiển thị đơn giá có dấu chấm nghìn khi đổ lên ô
const doVaoGoc = form.doVao.bind(form);
form.doVao = (d) => { doVaoGoc(d); const o = document.getElementById('txtDonGia'); if (d && o.value) o.value = Number(d.donGia).toLocaleString('vi-VN'); };
form.batDau();
