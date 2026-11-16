// Hộp thoại xác nhận (modal) và thông báo (toast) – không dùng alert/confirm mặc định (SPEC mục 0, 5).
import { motion, giamChuyenDong } from '../motion.config.js';

let dem = 0;

/**
 * Hỏi xác nhận trước khi Xóa/Khóa/Phục hồi/Lưu quyền (SPEC mục 5).
 * @returns {Promise<boolean>}
 */
export function xacNhan({ tieuDe, noiDung, nutDongY = 'Đồng ý', nutHuy = 'Hủy', nguyHiem = false }) {
  return new Promise((resolve) => {
    const id = 'hopXacNhan' + (++dem);
    const el = document.createElement('div');
    el.className = 'modal fade';
    el.id = id;
    el.tabIndex = -1;
    el.setAttribute('aria-labelledby', id + 'TieuDe');
    el.innerHTML = `
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content qt-modal">
          <div class="modal-header">
            <h2 class="modal-title fs-5" id="${id}TieuDe"></h2>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button>
          </div>
          <div class="modal-body"></div>
          <div class="modal-footer">
            <button type="button" class="qt-nut qt-nut--phu" data-bs-dismiss="modal" data-nut="huy"></button>
            <button type="button" class="qt-nut ${nguyHiem ? 'qt-nut--nguy-hiem' : 'qt-nut--chinh'}" data-nut="dong-y"></button>
          </div>
        </div>
      </div>`;
    el.querySelector('.modal-title').textContent = tieuDe;
    const than = el.querySelector('.modal-body');
    if (noiDung instanceof Node) than.append(noiDung); else than.textContent = noiDung;
    el.querySelector('[data-nut="huy"]').textContent = nutHuy;
    el.querySelector('[data-nut="dong-y"]').textContent = nutDongY;
    document.body.append(el);

    let ketQua = false;
    const modal = new bootstrap.Modal(el);
    el.querySelector('[data-nut="dong-y"]').addEventListener('click', () => { ketQua = true; modal.hide(); });
    el.addEventListener('shown.bs.modal', () => el.querySelector('[data-nut="huy"]').focus());
    el.addEventListener('hidden.bs.modal', () => { el.remove(); resolve(ketQua); });
    modal.show();
  });
}

function vungToast() {
  let v = document.getElementById('vungThongBao');
  if (!v) {
    v = document.createElement('div');
    v.id = 'vungThongBao';
    v.className = 'qt-vung-toast';
    v.setAttribute('aria-live', 'polite');
    v.setAttribute('aria-atomic', 'false');
    document.body.append(v);
  }
  return v;
}

/**
 * Thông báo kết quả sau mỗi thao tác (SPEC mục 5): trượt từ góc phải trên (file 10 mục 4).
 * @param {'ok'|'loi'|'tin'} loai
 */
export function thongBao(noiDung, loai = 'ok') {
  const icon = { ok: 'bi-check-circle-fill', loi: 'bi-exclamation-octagon-fill', tin: 'bi-info-circle-fill' }[loai];
  const t = document.createElement('div');
  t.className = `qt-toast qt-toast--${loai}`;
  t.setAttribute('role', loai === 'loi' ? 'alert' : 'status');
  t.innerHTML = `<i class="bi ${icon}" aria-hidden="true"></i><span></span>
    <button type="button" class="qt-toast__dong" aria-label="Đóng thông báo"><i class="bi bi-x-lg" aria-hidden="true"></i></button>`;
  t.querySelector('span').textContent = noiDung;
  vungToast().append(t);

  const dong = () => {
    if (!t.isConnected) return;
    if (window.gsap && !giamChuyenDong()) {
      gsap.to(t, { x: 40, opacity: 0, duration: motion.dashboard.toast, ease: motion.ease.inOut, onComplete: () => t.remove() });
    } else t.remove();
  };
  t.querySelector('.qt-toast__dong').addEventListener('click', dong);
  if (window.gsap) {
    if (giamChuyenDong()) gsap.from(t, { opacity: 0, duration: motion.reduced.fade });
    else gsap.from(t, { x: 48, opacity: 0, duration: motion.dashboard.toast, ease: motion.ease.out });
    gsap.delayedCall(loai === 'loi' ? 8 : 4.5, dong);
  }
  return t;
}
