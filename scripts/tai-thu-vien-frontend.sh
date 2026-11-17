#!/bin/sh
# Tải thư viện frontend (phiên bản ghim theo file 01) vào Frontend/ThuVien.Web/wwwroot/lib và font tự host.
# Không gọi CDN khi chạy thật: các tệp được commit vào repo và đóng gói vào image.
set -e
cd "$(dirname "$0")/../Frontend/ThuVien.Web/wwwroot"
J=https://cdn.jsdelivr.net/npm
tai() { mkdir -p "$(dirname "$2")"; curl -fsSL "$1" -o "$2"; echo "  $2"; }
tai $J/bootstrap@5.3.3/dist/css/bootstrap.min.css lib/bootstrap/5.3.3/bootstrap.min.css
tai $J/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js lib/bootstrap/5.3.3/bootstrap.bundle.min.js
tai $J/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css lib/bootstrap-icons/1.11.3/bootstrap-icons.min.css
tai $J/bootstrap-icons@1.11.3/font/fonts/bootstrap-icons.woff2 lib/bootstrap-icons/1.11.3/fonts/bootstrap-icons.woff2
tai $J/bootstrap-icons@1.11.3/font/fonts/bootstrap-icons.woff lib/bootstrap-icons/1.11.3/fonts/bootstrap-icons.woff
for f in gsap Flip ScrollTrigger Observer Draggable; do tai $J/gsap@3.13.0/dist/$f.min.js lib/gsap/3.13.0/$f.min.js; done
# Pha 4 – biểu đồ thống kê (file 09)
tai $J/echarts@5.6.0/dist/echarts.min.js lib/echarts/5.6.0/echarts.min.js
# Pha 5 – cuộn mượt cổng bạn đọc (file 10, 13)
tai $J/lenis@1.3.27/dist/lenis.min.js lib/lenis/1.3.27/lenis.min.js
tai $J/lenis@1.3.27/dist/lenis.css lib/lenis/1.3.27/lenis.css
for w in 400 500 600 700; do for s in vietnamese latin latin-ext; do
  tai $J/@fontsource/be-vietnam-pro@5.2.8/files/be-vietnam-pro-$s-$w-normal.woff2 fonts/be-vietnam-pro/be-vietnam-pro-$s-$w-normal.woff2
done; done
# Pha 6 – lật trang Book Preview (file 13: StPageFlip, gói npm page-flip 2.0.7, MIT)
tai $J/page-flip@2.0.7/dist/js/page-flip.module.js lib/page-flip/2.0.7/page-flip.module.js
# KHÔNG sửa thư viện: thẻ <style> nó tự chèn được CSP cho phép bằng băm SHA-256 (HeaderBaoMatMiddleware.BamStylePageFlip – D69).
# Ghim đúng 2.0.7: tệp tải về phải có sha256 = b718faafca6856bff51629baedeff601e2cc59482e951105bdbd6e46978cca38
echo "b718faafca6856bff51629baedeff601e2cc59482e951105bdbd6e46978cca38  lib/page-flip/2.0.7/page-flip.module.js" | sha256sum -c -
