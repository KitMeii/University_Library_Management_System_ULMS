// Header cổng (file 04 mục 2): bóng nhẹ khi scrollY > 8, thanh tiến độ cuộn 3px (scaleX qua quickSetter – file 10 mục 3),
// mục đang xem ở trang chủ (Kệ sách nổi bật, Thông tin thư viện) được đánh dấu trên menu.
const header = document.querySelector('.cong-header');
const thanh = document.querySelector('.cong-tien-do-cuon');
if (header) {
  const datScale = thanh && window.gsap ? gsap.quickSetter(thanh, 'scaleX') : null;
  let dangCho = false;
  const capNhat = () => {
    dangCho = false;
    const y = window.scrollY;
    header.classList.toggle('da-cuon', y > 8);
    if (datScale) {
      const max = document.documentElement.scrollHeight - window.innerHeight;
      datScale(max > 0 ? Math.min(1, y / max) : 0);
    }
  };
  window.addEventListener('scroll', () => {
    if (!dangCho) { dangCho = true; requestAnimationFrame(capNhat); }
  }, { passive: true });
  capNhat();
}

// Theo dõi khu vực đang xem (chỉ ở trang chủ): aria-current="location" cho mục neo, “page” cho Trang chủ khi ở đầu trang
if (location.pathname === '/' && 'IntersectionObserver' in window) {
  const muc = [...document.querySelectorAll('a[data-muc]')];
  const khu = ['ke-sach', 'thong-tin'].map((id) => document.getElementById(id)).filter(Boolean);
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
