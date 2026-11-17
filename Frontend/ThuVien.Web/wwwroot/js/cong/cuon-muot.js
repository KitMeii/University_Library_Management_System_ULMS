// Cuộn mượt cổng bạn đọc (file 10 mục 3, file 13): Lenis trên máy tính, đồng bộ ScrollTrigger.update, gsap.ticker.lagSmoothing(0);
// điện thoại/cảm ứng giữ cuộn gốc; giảm chuyển động → tắt. Liên kết neo (#ke-sach…) cuộn qua Lenis, chừa chiều cao header.
// Xuất hiện khi cuộn: tiêu đề khu vực [data-xuat-hien] trượt lên 24px + mờ → rõ, chạy một lần.
import { motion, giamChuyenDong } from '../motion.config.js';

const camUng = window.matchMedia('(hover: none), (pointer: coarse)').matches;
const header = () => document.querySelector('.cong-header')?.offsetHeight ?? 0;

/** @type {any} */
export let lenis = null;

if (window.gsap && window.ScrollTrigger) gsap.registerPlugin(ScrollTrigger);

if (window.Lenis && !giamChuyenDong() && (motion.scroll.lenisOnTouch || !camUng)) {
  lenis = new window.Lenis({ lerp: motion.scroll.lenisLerp, smoothWheel: true, syncTouch: false });
  window.__lenis = lenis;
  if (window.gsap) {
    if (window.ScrollTrigger) lenis.on('scroll', ScrollTrigger.update);
    gsap.ticker.add((t) => lenis.raf(t * 1000));
    gsap.ticker.lagSmoothing(0);
  } else {
    const vong = (t) => { lenis.raf(t); requestAnimationFrame(vong); };
    requestAnimationFrame(vong);
  }
}

/** Cuộn tới phần tử (qua Lenis nếu có), chừa chiều cao header dính. */
export function cuonToi(el) {
  if (!el) return;
  // Vị trí tính bằng số (Lenis tự cộng thêm scroll-margin-top nếu truyền phần tử → lệch hai lần)
  const dich = el.getBoundingClientRect().top + window.scrollY - header() - 8;
  if (lenis) lenis.scrollTo(dich);
  else window.scrollTo({ top: dich, behavior: giamChuyenDong() ? 'auto' : 'smooth' });
}

// Liên kết neo cùng trang (/#ke-sach khi đang ở “/”)
document.addEventListener('click', (e) => {
  const a = e.target.closest('a[href^="/#"], a[href^="#"]');
  if (!a || e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey) return;
  const href = a.getAttribute('href');
  if (href.startsWith('/#') && location.pathname !== '/') return;
  const id = href.slice(href.indexOf('#') + 1);
  const dich = id && document.getElementById(id);
  if (!dich) return;
  e.preventDefault();
  cuonToi(dich);
  history.replaceState(null, '', '#' + id);
  dich.setAttribute('tabindex', dich.getAttribute('tabindex') ?? '-1');
  dich.focus({ preventScroll: true });
});
// Mở trang kèm neo: cuộn tới sau khi bố cục ổn định
if (location.hash.length > 1) {
  const dich = document.getElementById(decodeURIComponent(location.hash.slice(1)));
  if (dich) window.addEventListener('load', () => cuonToi(dich), { once: true });
}

// Xuất hiện khi cuộn (ScrollTrigger, một lần)
if (window.gsap && window.ScrollTrigger && !giamChuyenDong()) {
  gsap.utils.toArray('[data-xuat-hien]').forEach((el) => {
    gsap.from(el, { y: 24, opacity: 0, duration: motion.dur.lg, ease: motion.ease.out, clearProps: 'transform,opacity',
      scrollTrigger: { trigger: el, start: 'top 88%', once: true } });
  });
}
