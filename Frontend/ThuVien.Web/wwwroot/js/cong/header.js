// Header cổng (file 04 mục 2 + THAM_KHAO_MTA mục 3.1, 3.2, 3.9):
// - bóng nhẹ khi scrollY > 8, thanh tiến độ cuộn 3px (scaleX qua quickSetter – file 10 mục 3);
// - thu gọn khi cuộn xuống (máy tính): cả header dịch lên bằng chiều cao dải tiện ích (transform, motion.dur.sm, ease.out),
//   cuộn lên hoặc về đầu trang thì hiện lại; điện thoại không thu gọn (dải tiện ích chứa nút menu);
// - menu thả xuống “Danh mục”, “Hướng dẫn mượn sách”: rê chuột/focus mở (CSS), nút mũi tên Enter/Space/↓, Esc đóng;
//   danh sách thể loại nạp một lần từ /api/cong/the-loai;
// - nút lên đầu trang: hiện khi cuộn quá 1 khung nhìn, ẩn khi Book Preview mở (CSS);
// - mục đang xem ở trang chủ được đánh dấu trên menu.
import { motion, giamChuyenDong } from '../motion.config.js';

const header = document.querySelector('.cong-header');
const thanh = document.querySelector('.cong-tien-do-cuon');
const tienIch = document.querySelector('.cong-header__tien-ich');
const nutLenDau = document.getElementById('btnLenDau');
const mayTinh = window.matchMedia('(min-width: 992px)');
const gsap = window.gsap;
const mucCon = [...document.querySelectorAll('.cong-nav__muc--con')];

if (header) {
  const datScale = thanh && gsap ? gsap.quickSetter(thanh, 'scaleX') : null;
  let dangCho = false;
  let yTruoc = window.scrollY;
  let thuGon = false;

  const datThuGon = (co) => {
    if (co === thuGon) return;
    thuGon = co;
    header.classList.toggle('thu-gon', co);
    if (co) dongTatCa();
    const dich = co ? -(tienIch?.offsetHeight ?? 0) : 0;
    if (gsap && !giamChuyenDong()) gsap.to(header, { y: dich, duration: motion.dur.sm, ease: motion.ease.out, overwrite: true });
    else if (gsap) gsap.set(header, { y: dich });
  };

  const capNhat = () => {
    dangCho = false;
    const y = window.scrollY;
    header.classList.toggle('da-cuon', y > 8);
    if (datScale) {
      const max = document.documentElement.scrollHeight - window.innerHeight;
      datScale(max > 0 ? Math.min(1, y / max) : 0);
    }
    // Thu gọn: chỉ máy tính, quá 120px và đang cuộn xuống; về gần đầu trang hoặc cuộn lên ≥ 6px thì mở lại
    if (!mayTinh.matches || y < 120) datThuGon(false);
    else if (y - yTruoc > 4) datThuGon(true);
    else if (yTruoc - y > 6) datThuGon(false);
    yTruoc = y;
    if (nutLenDau) nutLenDau.hidden = y < window.innerHeight;
  };
  window.addEventListener('scroll', () => {
    if (!dangCho) { dangCho = true; requestAnimationFrame(capNhat); }
  }, { passive: true });
  mayTinh.addEventListener('change', () => datThuGon(false));
  // Focus bằng bàn phím vào header khi đang thu gọn → hiện lại đầy đủ
  header.addEventListener('focusin', () => datThuGon(false));
  capNhat();
}

// ───────────── Menu thả xuống ─────────────
function dong(li, traFocus = false) {
  li.removeAttribute('data-mo');
  li.setAttribute('data-dong', '');
  const nut = li.querySelector('.cong-nav__mo');
  nut.setAttribute('aria-expanded', 'false');
  if (traFocus) nut.focus();
}
function dongTatCa() { mucCon.forEach((li) => { if (li.hasAttribute('data-mo') || li.matches(':focus-within')) dong(li); }); }
function mo(li, focusDau = false) {
  mucCon.filter((x) => x !== li).forEach((x) => dong(x));
  li.removeAttribute('data-dong');
  li.setAttribute('data-mo', '');
  li.querySelector('.cong-nav__mo').setAttribute('aria-expanded', 'true');
  napTheLoai();
  if (focusDau) requestAnimationFrame(() => li.querySelector('.cong-nav__con a')?.focus());
}
mucCon.forEach((li) => {
  const nut = li.querySelector('.cong-nav__mo');
  nut.addEventListener('click', () => (li.hasAttribute('data-mo') ? dong(li) : mo(li)));
  nut.addEventListener('keydown', (e) => { if (e.key === 'ArrowDown') { e.preventDefault(); mo(li, true); } });
  li.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') { e.preventDefault(); dong(li, true); return; }
    if ((e.key === 'ArrowDown' || e.key === 'ArrowUp') && e.target.closest('.cong-nav__con')) {
      e.preventDefault();
      const ds = [...li.querySelectorAll('.cong-nav__con a')];
      const i = ds.indexOf(e.target);
      ds[(i + (e.key === 'ArrowDown' ? 1 : -1) + ds.length) % ds.length]?.focus();
    }
  });
  li.addEventListener('pointerenter', () => { li.removeAttribute('data-dong'); nut.setAttribute('aria-expanded', 'true'); napTheLoai(); });
  li.addEventListener('pointerleave', () => { if (!li.hasAttribute('data-mo')) nut.setAttribute('aria-expanded', 'false'); });
  li.addEventListener('focusin', napTheLoai);
  li.addEventListener('focusout', (e) => {
    if (li.contains(e.relatedTarget)) return;
    li.removeAttribute('data-mo'); li.removeAttribute('data-dong'); nut.setAttribute('aria-expanded', 'false');
  });
});
document.addEventListener('click', (e) => {
  if (!e.target.closest('.cong-nav__muc--con')) mucCon.forEach((li) => { if (li.hasAttribute('data-mo')) dong(li); });
});
// Điện thoại: mở nhóm thu gọn “Danh mục” thì nạp thể loại
document.querySelectorAll('.cong-offcanvas__nhom').forEach((d) => d.addEventListener('toggle', () => { if (d.open) napTheLoai(); }));

let daNapTheLoai = false;
async function napTheLoai() {
  if (daNapTheLoai) return;
  daNapTheLoai = true;
  try {
    const ds = await (await fetch('/api/cong/the-loai', { headers: { Accept: 'application/json' } })).json();
    document.querySelectorAll('ul[data-ds-the-loai]').forEach((ul) => {
      const laMayTinh = ul.classList.contains('cong-nav__ds');
      for (const t of ds) {
        const li = document.createElement('li');
        const a = document.createElement('a');
        a.href = '/kham-pha?theLoai=' + encodeURIComponent(t.maTheLoai);
        a.dataset.locTheLoai = t.maTheLoai;
        if (laMayTinh) {
          const i = document.createElement('i');
          i.className = 'bi ' + (t.bieuTuong || 'bi-bookmark');
          i.setAttribute('aria-hidden', 'true');
          const so = document.createElement('span');
          so.className = 'cong-nav__so';
          so.textContent = t.soDauSach;
          a.append(i, document.createTextNode(t.tenTheLoai), so);
        } else a.textContent = `${t.tenTheLoai} (${t.soDauSach})`;
        li.append(a);
        ul.append(li);
      }
    });
  } catch { daNapTheLoai = false; }
}

// Trang có khu Khám phá sách (trang chủ): chọn thể loại trong menu → lọc tại chỗ, không đổi route (kham-pha.js lắng nghe)
document.addEventListener('click', (e) => {
  const a = e.target.closest('a[data-loc-the-loai]');
  if (!a || !document.getElementById('kpLuoi') || e.ctrlKey || e.metaKey || e.shiftKey) return;
  e.preventDefault();
  mucCon.forEach((li) => dong(li));
  document.dispatchEvent(new CustomEvent('ulms:loc-the-loai', { detail: a.dataset.locTheLoai }));
  const oc = document.getElementById('congMenu');
  if (oc?.classList.contains('show')) window.bootstrap?.Offcanvas.getInstance(oc)?.hide();
});

// ───────────── Nút lên đầu trang ─────────────
nutLenDau?.addEventListener('click', () => {
  if (window.__lenis) window.__lenis.scrollTo(0);
  else window.scrollTo({ top: 0, behavior: giamChuyenDong() ? 'auto' : 'smooth' });
  document.getElementById('noiDung')?.focus({ preventScroll: true });
});

// ───────────── Mục đang xem (chỉ trang chủ) ─────────────
if (location.pathname === '/' && 'IntersectionObserver' in window) {
  const muc = [...document.querySelectorAll('a[data-muc]')];
  const khu = ['ke-sach', 'danh-muc', 'kham-pha', 'huong-dan', 'thong-tin'].map((id) => document.getElementById(id)).filter(Boolean);
  const danh = (id) => muc.forEach((a) => {
    const dich = a.dataset.muc;
    if (id ? dich === '/#' + id : dich === '/') a.setAttribute('aria-current', id ? 'location' : 'page');
    else a.removeAttribute('aria-current');
  });
  const dangThay = new Map();
  const io = new IntersectionObserver((ds) => {
    ds.forEach((d) => dangThay.set(d.target.id, d.isIntersecting ? d.intersectionRatio : 0));
    const tot = [...dangThay.entries()].filter(([, r]) => r > 0.25).sort((a, b) => b[1] - a[1])[0];
    danh(tot ? tot[0] : null);
  }, { threshold: [0, 0.25, 0.5, 0.75] });
  khu.forEach((k) => io.observe(k));
}
