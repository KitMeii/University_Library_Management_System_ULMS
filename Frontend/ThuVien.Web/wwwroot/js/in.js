// Trang in chứng từ (SPEC 6.4): nút In / Đóng; mở ở tab riêng với ?tuIn=1 thì tự mở hộp thoại in.
(function () {
  var trongKhung = window.self !== window.top;
  document.getElementById('btnInLai').addEventListener('click', function () { window.print(); });
  document.getElementById('btnDongIn').addEventListener('click', function () {
    if (trongKhung) return;
    if (window.opener) window.close(); else history.length > 1 ? history.back() : (location.href = '/quan-tri');
  });
  if (!trongKhung && new URLSearchParams(location.search).get('tuIn') === '1') {
    window.addEventListener('load', function () { document.fonts.ready.then(function () { window.print(); }); });
  }
})();
