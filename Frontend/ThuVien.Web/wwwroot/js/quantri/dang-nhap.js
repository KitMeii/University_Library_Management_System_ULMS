// SPEC 6.3.1 frmDangNhap – trạng thái nút:
//  (Load) xóa trắng, con trỏ ở txtTenDangNhap; btnDangNhap Enable = False; btnThoat Enable = True
//  (txtTenDangNhap, txtMatKhau) nhập đủ cả hai → btnDangNhap Enable = True, thiếu một ô → False
//  btnThoat: hỏi xác nhận rồi đóng (web: về trang chủ cổng – GIA_DINH giả định Pha 0)
import { motion, giamChuyenDong } from '../motion.config.js';

const form = document.getElementById('frmDangNhap');
const txtTen = document.getElementById('txtTenDangNhap');
const txtMk = document.getElementById('txtMatKhau');
const btnDangNhap = document.getElementById('btnDangNhap');
const btnThoat = document.getElementById('btnThoat');
const nutHienMk = document.getElementById('btnHienMatKhau');

function capNhatTrangThai() {
  const du = txtTen.value.trim() !== '' && txtMk.value !== '';
  btnDangNhap.disabled = !du;
  btnDangNhap.setAttribute('aria-disabled', String(!du));
}

txtTen.addEventListener('input', capNhatTrangThai);
txtMk.addEventListener('input', capNhatTrangThai);
capNhatTrangThai();
(txtTen.value ? txtMk : txtTen).focus();

form.addEventListener('submit', (e) => {
  if (btnDangNhap.disabled) { e.preventDefault(); return; }
  btnDangNhap.disabled = true;
  btnDangNhap.setAttribute('aria-busy', 'true');
  btnDangNhap.querySelector('.nhan').textContent = 'Đang đăng nhập…';
  btnDangNhap.querySelector('.spinner-border')?.removeAttribute('hidden');
});

nutHienMk?.addEventListener('click', () => {
  const hien = txtMk.type === 'password';
  txtMk.type = hien ? 'text' : 'password';
  nutHienMk.setAttribute('aria-pressed', String(hien));
  nutHienMk.setAttribute('aria-label', hien ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
  nutHienMk.querySelector('.bi').className = hien ? 'bi bi-eye-slash' : 'bi bi-eye';
});

// btnThoat → hộp thoại xác nhận (không dùng alert/confirm mặc định – SPEC mục 0)
const modalThoat = document.getElementById('modalThoat');
btnThoat.addEventListener('click', () => bootstrap.Modal.getOrCreateInstance(modalThoat).show());

// Lỗi đăng nhập: rung ngang 6px 0.3s (chỉ trục x – file 10 mục 4)
const loi = document.getElementById('thongBaoLoi');
if (loi && window.gsap && !giamChuyenDong()) {
  gsap.fromTo(form, { x: 0 }, { x: 6, duration: motion.dur.xs / 3, repeat: 5, yoyo: true, ease: 'none', clearProps: 'transform' });
}
