// Nguồn DUY NHẤT của mọi thông số chuyển động (file 10 mục 1). Không viết số thời lượng/easing ở chỗ khác.
export const motion = {
  dur: { xs: 0.15, sm: 0.25, md: 0.45, lg: 0.7, xl: 1.1 },
  ease: { out: 'power2.out', outStrong: 'power3.out', inOut: 'power2.inOut', expo: 'expo.out', back: 'back.out(1.6)' },
  stagger: { tight: 0.03, normal: 0.06, loose: 0.1 },
  hero: { in: 1.1, out: 1.1, interval: 4.5, resumeAfterDot: 6, xPercent: 7, scaleFrom: 1.05 },
  coverflow: { step: 0.7, perDistance: 0.08, maxJump: 1.1, ease: 'power3.out', wheelLock: 0.25, hoverLift: 10, shelfSwitch: 0.6,
               arcRadius: 1.6, arcStepDeg: 9, cursorSize: 58, cursorLag: 0.18, tiltMax: 6, swipeMaxJump: 5 },
  glass: { hoverLift: 5, cardStagger: 0.06 },
  scroll: { lenisLerp: 0.1, lenisOnTouch: false },
  preview: { flip: 0.55, open: 1.6, pageTurn: 0.22, close: 0.9 },
  dashboard: { pageIn: 0.45, cardStagger: 0.05, countUp: 1.2, rowStagger: 0.02, toast: 0.35, modal: 0.3 },
  reduced: { fade: 0.2 }
};

/** true khi người dùng bật “giảm chuyển động” → dùng motion.reduced. */
export const giamChuyenDong = () => window.matchMedia('(prefers-reduced-motion: reduce)').matches;
