// Khung dashboard: thu/mở nhóm menu, thu gọn sidebar (desktop), hiệu ứng vào trang (file 10 mục 4).
import { motion, giamChuyenDong } from '../motion.config.js';
import { ganTatCa } from '../chon-ngay.js';

// DatePicker dd/MM/yyyy cho mọi ô ngày (chạy trước script của trang)
ganTatCa();

const khung = document.querySelector('.qt-khung');

// Nhóm menu thu gọn được (lưu lựa chọn ở localStorage – chỉ là tùy chọn giao diện, không phải dữ liệu nhạy cảm)
document.querySelectorAll('.qt-nav__tieu-de').forEach((nut) => {
  const ds = document.getElementById(nut.getAttribute('aria-controls'));
  const khoa = 'qt-nhom:' + nut.dataset.nhom;
  let dong = false;
  try { dong = localStorage.getItem(khoa) === '0'; } catch { /* trình duyệt chặn lưu trữ */ }
  if (dong && !ds.querySelector('[aria-current="page"]')) {
    nut.setAttribute('aria-expanded', 'false');
    ds.hidden = true;
  }
  nut.addEventListener('click', () => {
    const mo = nut.getAttribute('aria-expanded') !== 'true';
    nut.setAttribute('aria-expanded', String(mo));
    ds.hidden = !mo;
    try { localStorage.setItem(khoa, mo ? '1' : '0'); } catch { /* bỏ qua */ }
  });
});

// Thu gọn sidebar (≥ 992px)
const nutThuGon = document.getElementById('btnThuGonMenu');
if (nutThuGon && khung) {
  try { if (localStorage.getItem('qt-thu-gon') === '1') khung.classList.add('thu-gon'); } catch { /* bỏ qua */ }
  nutThuGon.setAttribute('aria-pressed', String(khung.classList.contains('thu-gon')));
  nutThuGon.addEventListener('click', () => {
    const thuGon = khung.classList.toggle('thu-gon');
    nutThuGon.setAttribute('aria-pressed', String(thuGon));
    try { localStorage.setItem('qt-thu-gon', thuGon ? '1' : '0'); } catch { /* bỏ qua */ }
  });
}

// Vào trang: nội dung trượt lên 12px + mờ → rõ; thẻ stagger
const noiDung = document.querySelector('.qt-noi-dung');
if (noiDung && window.gsap) {
  if (giamChuyenDong()) {
    gsap.from(noiDung, { opacity: 0, duration: motion.reduced.fade, clearProps: 'opacity' });
  } else {
    const tl = gsap.timeline({ defaults: { ease: motion.ease.out } });
    tl.from(noiDung, { y: 12, opacity: 0, duration: motion.dashboard.pageIn, clearProps: 'transform,opacity' });
    const the = noiDung.querySelectorAll('.qt-the');
    if (the.length) {
      tl.from(the, { y: 12, opacity: 0, duration: motion.dashboard.pageIn, stagger: motion.dashboard.cardStagger,
        clearProps: 'transform,opacity' }, '<0.05');
    }
  }
}
