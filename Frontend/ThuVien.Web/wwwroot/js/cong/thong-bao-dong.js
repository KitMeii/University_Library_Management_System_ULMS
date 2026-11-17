// Một dòng thông báo – dùng chung cho chuông header (thong-bao.js) và trang /tai-khoan/thong-bao (tai-khoan.js).

const BIEU_TUONG = {
  'Đặt chỗ thành công': 'bi-bookmark-check', 'Sắp hết hạn giữ': 'bi-hourglass-split', 'Hết hạn giữ': 'bi-bookmark-x',
  'Đã nhận sách': 'bi-bag-check', 'Sắp đến hạn trả': 'bi-calendar-event', 'Quá hạn trả': 'bi-alarm', 'Biên bản vi phạm': 'bi-receipt', 'Đặt chỗ bị hủy': 'bi-x-circle'
};

export function thoiGianTuongDoi(s) {
  const phut = Math.round((Date.now() - new Date(s)) / 60000);
  if (phut < 1) return 'vừa xong';
  if (phut < 60) return `${phut} phút trước`;
  const gio = Math.round(phut / 60);
  if (gio < 24) return `${gio} giờ trước`;
  const ngay = Math.round(gio / 24);
  if (ngay < 30) return `${ngay} ngày trước`;
  return new Date(s).toLocaleDateString('vi-VN');
}

/** Một dòng thông báo (dùng chung với trang /tai-khoan/thong-bao). */
export function dongThongBao(t, khiBam) {
  const li = document.createElement('li');
  li.className = 'tb-dong' + (t.daDoc ? '' : ' tb-dong--moi');
  const a = document.createElement(t.lienKet ? 'a' : 'div');
  a.className = 'tb-dong__noi';
  if (t.lienKet) a.href = t.lienKet;
  const i = document.createElement('i');
  i.className = 'bi ' + (BIEU_TUONG[t.loai] || 'bi-bell');
  i.setAttribute('aria-hidden', 'true');
  const than = document.createElement('span');
  than.className = 'tb-dong__than';
  const loai = document.createElement('strong'); loai.textContent = t.loai;
  const nd = document.createElement('span'); nd.className = 'tb-dong__nd'; nd.textContent = t.noiDung;
  const tg = document.createElement('time'); tg.dateTime = t.tgTao; tg.textContent = thoiGianTuongDoi(t.tgTao);
  than.append(loai, nd, tg);
  a.append(i, than);
  if (!t.daDoc) {
    const cham = document.createElement('span'); cham.className = 'tb-dong__cham'; cham.setAttribute('aria-label', 'Chưa đọc');
    a.append(cham);
  }
  a.addEventListener('click', () => khiBam?.(t));
  li.append(a);
  return li;
}
