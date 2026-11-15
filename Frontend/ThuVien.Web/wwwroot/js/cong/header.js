// Header cổng (file 04 mục 2): bóng nhẹ khi scrollY > 8, thanh tiến độ cuộn 3px (scaleX qua quickSetter – file 10 mục 3).
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
