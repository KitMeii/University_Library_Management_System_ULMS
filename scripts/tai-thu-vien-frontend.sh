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
for w in 400 500 600 700; do for s in vietnamese latin latin-ext; do
  tai $J/@fontsource/be-vietnam-pro@5.2.8/files/be-vietnam-pro-$s-$w-normal.woff2 fonts/be-vietnam-pro/be-vietnam-pro-$s-$w-normal.woff2
done; done
