// Nguồn DUY NHẤT của mọi thông số chuyển động (file 10 mục 1). Không viết số thời lượng/easing ở chỗ khác.
export const motion = {
  dur: { xs: 0.15, sm: 0.25, md: 0.45, lg: 0.7, xl: 1.1 },
  ease: { out: 'power2.out', outStrong: 'power3.out', inOut: 'power2.inOut', expo: 'expo.out', back: 'back.out(1.6)' },
  stagger: { tight: 0.03, normal: 0.06, loose: 0.1 },
  hero: { in: 1.1, out: 1.1, interval: 4.5, resumeAfterDot: 6, xPercent: 7, scaleFrom: 1.05, ease: 'power2.inOut', reducedFade: 0.3 },
  coverflow: { step: 0.7, perDistance: 0.08, maxJump: 1.1, ease: 'power3.out', wheelLock: 0.25, hoverLift: 10, shelfSwitch: 0.6,
               arcRadius: 1.6, arcStepDeg: 9, cursorSize: 58, cursorLag: 0.18, tiltMax: 6, swipeMaxJump: 5,
               // Hình học kệ (file 05 mục 4): góc xoay theo khoảng cách tới tâm, thu nhỏ dần, cung lùi sâu, gáy tối dần
               rotNear: 58, rotFar: 82, scaleMin: 0.42, scaleK: 0.075, scalePow: 0.85, depthPerBook: 12, arcMaxBooks: 10,
               // frontVisible: phần bìa (đã xoay) tính vào khoảng cách từ cuốn thứ 2 trở ra – phần còn lại bị cuốn kế bên che
               // Điện thoại (file 05 mục 5b): cuốn giữa 42vw, hai cuốn ±1 xoay gắt hơn để mỗi bên còn chỗ cho các gáy
               gap: 5, spineRatio: 0.16, frontVisible: 0.2, rotNearMobile: 72, centerVwMobile: 0.42, shadePerBook: 0.06, shadeMax: 0.45, dragPerBook: 0.6, inertia: 0.25,
               shelfShiftPercent: 12, titleStagger: 0.012 },
  glass: { hoverLift: 5, cardStagger: 0.06 },
  scroll: { lenisLerp: 0.1, lenisOnTouch: false },
  preview: { flip: 0.55, open: 1.6, pageTurn: 0.22, close: 0.9 },
  dashboard: { pageIn: 0.45, cardStagger: 0.05, countUp: 1.2, rowStagger: 0.02, toast: 0.35, modal: 0.3 },
  // ECharts (file 09 mục 2): vào 900 ms cubicOut, cột/điểm trễ dần 30 ms, cập nhật dữ liệu 600 ms – đơn vị giây như các mục khác
  chart: { in: 0.9, itemDelay: 0.03, update: 0.6, ease: 'cubicOut' },
  reduced: { fade: 0.2 }
};

/** true khi người dùng bật “giảm chuyển động” → dùng motion.reduced. */
export const giamChuyenDong = () => window.matchMedia('(prefers-reduced-motion: reduce)').matches;
