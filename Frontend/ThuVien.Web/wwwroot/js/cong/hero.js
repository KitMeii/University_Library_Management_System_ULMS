// Hero (file 04 mục 2): ảnh chuyển lần lượt – vào opacity 0→1, xPercent 7→0, scale 1.05→1; ra xPercent → −7 (1.1s power2.inOut);
// tự chuyển mỗi 4.5s bằng gsap.delayedCall; nút Tạm dừng/Phát; dừng khi trỏ chuột/focus; bấm chấm → nghỉ 6s rồi chạy lại;
// ảnh sau chỉ chuyển khi decode() xong; giảm chuyển động → chỉ mờ chéo 0.3s.
import { motion, giamChuyenDong } from '../motion.config.js';

const khung = document.getElementById('heroKhung');
const anh = khung ? [...khung.querySelectorAll('.hero__slide')] : [];
if (khung && anh.length > 1 && window.gsap) {
  const cham = [...khung.querySelectorAll('.hero__cham-nut')];
  const nutDung = document.getElementById('btnHeroDung');
  const h = motion.hero;
  let hien = 0;
  let dangChuyen = false;
  let nguoiDung = false;   // người dùng bấm Tạm dừng
  let tamNghi = false;     // đang trỏ chuột/focus trong khung
  let hen = null;

  gsap.set(anh.slice(1), { opacity: 0 });

  const napAnh = (img) => {
    if (img.dataset.src && !img.getAttribute('src')) img.src = img.dataset.src;
    return img.decode ? img.decode().catch(() => {}) : Promise.resolve();
  };

  function henGio(giay = h.interval) {
    hen?.kill();
    if (nguoiDung || tamNghi) return;
    // Book Preview đang mở: không chuyển ảnh (không chạy cùng lúc với chuyển động sách), hẹn lại lần sau
    hen = gsap.delayedCall(giay, () => (document.documentElement.classList.contains('bp-dang-mo') ? henGio() : chuyenToi((hien + 1) % anh.length)));
  }

  async function chuyenToi(i) {
    if (i === hien || dangChuyen) return;
    dangChuyen = true;
    const cu = anh[hien], moi = anh[i];
    await napAnh(moi);
    // tải trước ảnh kế tiếp để lần chuyển sau không chờ
    napAnh(anh[(i + 1) % anh.length]);
    if (giamChuyenDong()) {
      await Promise.all([
        gsap.to(cu, { opacity: 0, duration: h.reducedFade }),
        gsap.fromTo(moi, { opacity: 0, xPercent: 0, scale: 1 }, { opacity: 1, duration: h.reducedFade })
      ]);
    } else {
      await Promise.all([
        gsap.to(cu, { opacity: 0, xPercent: -h.xPercent, duration: h.out, ease: h.ease }),
        gsap.fromTo(moi, { opacity: 0, xPercent: h.xPercent, scale: h.scaleFrom }, { opacity: 1, xPercent: 0, scale: 1, duration: h.in, ease: h.ease })
      ]);
    }
    cu.classList.remove('dang-hien');
    moi.classList.add('dang-hien');
    hien = i;
    cham.forEach((c, k) => { if (k === i) c.setAttribute('aria-current', 'true'); else c.removeAttribute('aria-current'); });
    dangChuyen = false;
    henGio();
  }

  cham.forEach((c, k) => c.addEventListener('click', () => { chuyenToi(k); henGio(h.resumeAfterDot); }));
  // Nút ← → (THAM_KHAO_MTA mục 3.3): dùng lại đúng hiệu ứng chuyển ảnh ở trên; là <button> nên bấm được bằng Enter/Space
  document.getElementById('btnHeroTruoc')?.addEventListener('click', () => { chuyenToi((hien - 1 + anh.length) % anh.length); henGio(h.resumeAfterDot); });
  document.getElementById('btnHeroSau')?.addEventListener('click', () => { chuyenToi((hien + 1) % anh.length); henGio(h.resumeAfterDot); });
  // Phím ←/→ khi focus đang trong khung Hero (trừ ô nhập)
  khung.addEventListener('keydown', (e) => {
    if ((e.key !== 'ArrowLeft' && e.key !== 'ArrowRight') || e.target.closest('input')) return;
    e.preventDefault();
    chuyenToi((hien + (e.key === 'ArrowRight' ? 1 : -1) + anh.length) % anh.length);
    henGio(h.resumeAfterDot);
  });

  nutDung?.addEventListener('click', () => {
    nguoiDung = !nguoiDung;
    nutDung.setAttribute('aria-pressed', String(nguoiDung));
    nutDung.setAttribute('aria-label', nguoiDung ? 'Phát trình chiếu' : 'Tạm dừng trình chiếu');
    nutDung.querySelector('i').className = 'bi ' + (nguoiDung ? 'bi-play-fill' : 'bi-pause-fill');
    if (nguoiDung) hen?.kill(); else henGio();
  });

  const nghi = (v) => { tamNghi = v; if (v) hen?.kill(); else henGio(); };
  khung.addEventListener('pointerenter', () => nghi(true));
  khung.addEventListener('pointerleave', () => nghi(khung.contains(document.activeElement)));
  khung.addEventListener('focusin', () => nghi(true));
  khung.addEventListener('focusout', (e) => { if (!khung.contains(e.relatedTarget)) nghi(false); });
  // Tab ẩn: không chạy
  document.addEventListener('visibilitychange', () => { if (document.hidden) hen?.kill(); else henGio(); });

  napAnh(anh[1]);
  henGio();
}

// Gợi ý “Cuộn xuống”: ẩn khi đã cuộn quá 40px (CSS chạy nhấp nhô, tắt khi giảm chuyển động)
const cuon = document.getElementById('lnkCuonXuong');
if (cuon) {
  const capNhat = () => cuon.classList.toggle('an', window.scrollY > 40);
  window.addEventListener('scroll', capNhat, { passive: true });
  capNhat();
}
