// Kệ sách nổi bật – hiệu ứng coverflow gáy sách theo ảnh tham khảo (file 05, file 13 mục 3.1).
// Trạng thái chỉ có MỘT số thực `pos` (chỉ số cuốn ở giữa, lẻ trong lúc chuyển động); layout(pos) tính transform cho mọi cuốn;
// GSAP tween `pos` (không tween từng cuốn) → chuyển nhiều bước liền mạch. Mỗi cuốn là khối hộp CSS 3D (bìa, 2 gáy, mặt trên).
// Mọi thông số ở motion.config.js (motion.coverflow). Tương tác đi qua state machine (file 10 mục 2).
import { motion, giamChuyenDong } from '../motion.config.js';
import { MayTrangThai } from '../state-machine.js';

const cf = motion.coverflow;
const $ = (id) => document.getElementById(id);
const goc = $('ke-sach');
const sanKhau = $('cfSanKhau');
const track = $('cfKe');
const duLieuBanDau = JSON.parse($('duLieuKe').textContent);

const RAD = Math.PI / 180;
const clamp = (v, a, b) => Math.min(b, Math.max(a, v));
const lerp = (a, b, t) => a + (b - a) * t;
const coChuot = () => window.matchMedia('(hover: hover) and (pointer: fine)').matches;

let danhSachKe = duLieuBanDau.danhSach;
let ke = duLieuBanDau.ke;
let sach = ke.sach;
const st = { pos: Math.floor((sach.length - 1) / 2) };
let sachLi = [];          // [{ li, nut, dat, mat }]
let W = 200, H = 300, T = 32, soMoiBen = 11, X = [0], PERSPECTIVE = 1800;
let chiSoGiua = -1;
let tween = null;

// ---------------- Hình học (file 05 mục 4) ----------------
let gocGan = cf.rotNear; // điện thoại: cf.rotNearMobile
const gocXoay = (a) => (a <= 1 ? lerp(0, gocGan, a) : lerp(gocGan, cf.rotFar, clamp(a - 1, 0, 1)));
const tiLe = (a) => Math.max(cf.scaleMin, 1 - cf.scaleK * Math.pow(a, cf.scalePow));
const chieuSau = (a) => -(cf.arcRadius * W) * (1 - Math.cos(Math.min(a, cf.arcMaxBooks) * cf.arcStepDeg * RAD)) - a * cf.depthPerBook;

function doKichThuoc() {
  const rong = goc.clientWidth;
  const dt = rong < 768;
  gocGan = dt ? cf.rotNearMobile : cf.rotNear;
  W = dt ? Math.min(rong * cf.centerVwMobile, 220) : clamp(rong * 0.135, 150, 230);
  const caoSan = sanKhau.clientHeight || W * 1.7;
  W = Math.min(W, (caoSan * 0.86) / 1.5); // vừa chiều cao sân khấu
  H = W * 1.5;
  T = W * cf.spineRatio;
  soMoiBen = rong >= 1440 ? 18 : rong >= 1280 ? 15 : rong >= 1024 ? 11 : rong >= 768 ? 7 : 4;
  PERSPECTIVE = parseFloat(getComputedStyle(sanKhau).perspective) || 1800;
  // Biến kích thước đặt ở sân khấu: cuốn sách và bóng sàn cùng dùng
  sanKhau.style.setProperty('--w', W + 'px');
  sanKhau.style.setProperty('--h', H + 'px');
  sanKhau.style.setProperty('--t', T + 'px');
  // X[k]: vị trí ngang (trên màn hình) của cuốn cách tâm k – cộng dồn bề rộng hiển thị từ tâm ra
  X = [0];
  let x = 0, nuaTruoc = W / 2;
  for (let k = 1; k <= soMoiBen + 2; k++) {
    const r = gocXoay(k) * RAD, s = tiLe(k);
    // hai cuốn ±1 còn thấy một phần bìa; xa hơn chỉ lộ gáy
    const phanBia = k <= 1 ? 1 : cf.frontVisible;
    const rongK = (W * Math.cos(r) * phanBia + T * Math.sin(r)) * s;
    x += nuaTruoc + cf.gap * s + rongK / 2;
    X[k] = x;
    nuaTruoc = rongK / 2;
  }
}

/** Vị trí ngang cho khoảng cách d (lẻ) – nội suy phần lẻ để không giật; bù phối cảnh để khớp vị trí trên màn hình. */
function viTriX(d, z) {
  const a = Math.abs(d);
  const i0 = Math.min(Math.floor(a), X.length - 2);
  const xa = lerp(X[i0], X[i0 + 1], a - i0);
  return Math.sign(d) * xa * ((PERSPECTIVE - z) / PERSPECTIVE);
}

function layout(pos) {
  for (let i = 0; i < sachLi.length; i++) {
    const b = sachLi[i];
    const d = i - pos, a = Math.abs(d);
    const hien = a <= soMoiBen + 0.6;
    if (!hien) { if (b.an !== true) { b.li.style.visibility = 'hidden'; b.an = true; } continue; }
    if (b.an !== false) { b.li.style.visibility = 'visible'; b.an = false; }
    const z = chieuSau(a);
    b.dat({
      x: viTriX(d, z), z, rotationY: (d >= 0 ? 1 : -1) * gocXoay(a), scale: tiLe(a),
      opacity: clamp(soMoiBen + 0.6 - a, 0, 1), zIndex: 1000 - Math.round(a * 10)
    });
    const toi = Math.round(clamp(a * cf.shadePerBook, 0, cf.shadeMax) * 100) / 100;
    if (toi !== b.toiCu) { b.toi.forEach((f) => f(toi)); b.toiCu = toi; }
    // Từ cuốn thứ 3 trở ra bìa trước gần vuông góc và bị cuốn kế bên che → ẩn để bớt lớp vẽ
    const anBia = a > 2.5;
    if (anBia !== b.biaAn) { b.mat.style.visibility = anBia ? 'hidden' : ''; b.biaAn = anBia; }
  }
  const giua = clamp(Math.round(pos), 0, sach.length - 1);
  if (giua !== chiSoGiua) doiCuonGiua(giua);
  capNhatTruc(pos);
}

// ---------------- Dựng DOM ----------------
// Màu gáy do thủ thư nhập tự do → chọn màu chữ đạt tương phản AA 4.5:1 (WCAG): chữ giấy (--book-paper) hoặc chữ rừng đậm
// (--midnight); không cái nào đạt thì làm tối màu gáy dần tới khi chữ giấy đạt.
const GIAY = [0xF6, 0xF1, 0xE6], RUNG = [0x0B, 0x2A, 0x25];
const rgb = (hex) => { const m = /^#?([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})$/i.exec(hex || ''); return m ? [m[1], m[2], m[3]].map((h) => parseInt(h, 16)) : [0x12, 0x3F, 0x35]; };
const doSangTuongDoi = (c) => {
  const [r, g, b] = c.map((v) => { v /= 255; return v <= 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4); });
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
};
const tuongPhan = (a, b) => { const [x, y] = [doSangTuongDoi(a), doSangTuongDoi(b)].sort((m, n) => n - m); return (x + 0.05) / (y + 0.05); };
const hex = (c) => '#' + c.map((v) => Math.round(v).toString(16).padStart(2, '0')).join('');
function mauDoc(mau) {
  let nen = rgb(mau);
  if (tuongPhan(nen, GIAY) >= 4.5) return { nen: hex(nen), sang: false };
  if (tuongPhan(nen, RUNG) >= 4.5) return { nen: hex(nen), sang: true };
  for (let i = 0; i < 12 && tuongPhan(nen, GIAY) < 4.5; i++) nen = nen.map((v, k) => v * 0.9 + RUNG[k] * 0.1);
  return { nen: hex(nen), sang: false };
}

function biaChu(s) {
  const b = document.createElement('span');
  b.className = 'bia-chu';
  b.innerHTML = '<span class="bia-chu__nhan">THƯ VIỆN</span><span class="bia-chu__ten"></span><span class="bia-chu__gach"></span><span class="bia-chu__tac-gia"></span><span class="bia-chu__dau"><i class="bi bi-book-half"></i></span>';
  b.querySelector('.bia-chu__ten').textContent = s.tenSach;
  b.querySelector('.bia-chu__tac-gia').textContent = s.tacGia || '';
  return b;
}

function gay(s, ben) {
  const g = document.createElement('span');
  g.className = 'book__spine book__spine--' + ben;
  g.setAttribute('aria-hidden', 'true');
  g.innerHTML = '<span class="book__spine-dai"></span><span class="book__spine-ten"></span><span class="book__spine-dau"><i class="bi bi-book-half"></i></span>';
  g.querySelector('.book__spine-ten').textContent = s.tenSach;
  return g;
}

function taoCuon(s, i) {
  const li = document.createElement('li');
  li.className = 'book';
  li.dataset.index = i;
  const mau = mauDoc(s.mauGay);
  li.style.setProperty('--mau', mau.nen); // CSSOM – không phải style nội tuyến (CSP)
  if (mau.sang) li.classList.add('book--sang');
  const nut = document.createElement('button');
  nut.type = 'button';
  nut.className = 'book__body';
  nut.tabIndex = -1;
  // Tên đọc cho trình đọc màn hình là chữ ẩn trong nút; các mặt sách (chữ trang trí trên bìa, gáy) ẩn khỏi trình đọc
  const ten = document.createElement('span');
  ten.className = 'sr-only';
  ten.textContent = `${s.tenSach}${s.tacGia ? ' – ' + s.tacGia : ''}`;
  const mat = document.createElement('span');
  mat.className = 'book__front';
  mat.setAttribute('aria-hidden', 'true');
  if (s.anhBiaNho) {
    const img = document.createElement('img');
    img.src = s.anhBiaNho; img.alt = ''; img.decoding = 'async'; img.draggable = false;
    img.width = 240; img.height = 360;
    mat.append(img);
  } else mat.append(biaChu(s));
  const gayTrai = gay(s, 'trai'), gayPhai = gay(s, 'phai');
  // Lớp tối dần về hai mép: phần tử thật, ghi opacity trực tiếp (không dùng biến CSS kế thừa → không tính lại style cả cây)
  const toi = [mat, gayTrai, gayPhai].map((m) => {
    const t = document.createElement('span');
    t.className = 'book__shade';
    m.append(t);
    return gsap.quickSetter(t, 'opacity');
  });
  nut.append(ten, mat, gayTrai, gayPhai);
  li.append(nut);
  gsap.set(li, { xPercent: -50, yPercent: -50 });
  return { li, nut, mat, s, dat: gsap.quickSetter(li, 'css'), toi, toiCu: -1, biaAn: false, an: null };
}

function dungKe() {
  track.replaceChildren();
  sachLi = sach.map((s, i) => taoCuon(s, i));
  track.append(...sachLi.map((b) => b.li));
  chiSoGiua = -1;
  veTruc();
}

// ---------------- Chú thích cuốn giữa ----------------
const tieuDe = $('cfTieuDe'), tacGia = $('cfTacGia'), meta = $('cfMeta'), docTen = $('cfDocTen');
let animChu = null;

function chuMeta(s) {
  const phan = [];
  if (ke.truc === 'hang') phan.push(`HẠNG ${s.hang}`, `${s.soLuotMuon} LƯỢT MƯỢN / 90 NGÀY`);
  else phan.push(s.namXb ? String(s.namXb) : 'CHƯA RÕ NĂM', (s.tenTheLoai || 'CHƯA PHÂN LOẠI').toUpperCase());
  phan.push(s.soBanCoTheMuon > 0 ? `CÒN ${s.soBanCoTheMuon} BẢN CÓ THỂ MƯỢN` : 'ĐANG ĐƯỢC MƯỢN HẾT');
  return phan.join(' — ');
}

function doiCuonGiua(i) {
  const cu = chiSoGiua;
  chiSoGiua = i;
  const s = sach[i];
  sachLi.forEach((b, k) => {
    const giua = k === i;
    b.nut.tabIndex = giua ? 0 : -1;
    if (giua) b.li.setAttribute('aria-current', 'true'); else b.li.removeAttribute('aria-current');
  });
  napBiaLon(i);
  docTen.textContent = `Đang chọn: ${s.tenSach}${s.tacGia ? ', ' + s.tacGia : ''}`;
  $('cfTruoc').disabled = i <= 0;
  $('cfSau').disabled = i >= sach.length - 1;
  // Tên sách: chữ cũ trượt lên + mờ, chữ mới trượt vào theo ký tự (stagger 0.012s); tác giả, meta mờ chéo
  animChu?.kill();
  if (cu < 0 || giamChuyenDong() || !window.gsap) {
    tieuDe.textContent = s.tenSach; tacGia.textContent = s.tacGia || ''; meta.textContent = chuMeta(s);
    return;
  }
  const kyTu = [...s.tenSach].map((c) => { const sp = document.createElement('span'); sp.className = 'ky-tu'; sp.textContent = c; return sp; });
  animChu = gsap.timeline()
    .to(tieuDe, { yPercent: -40, opacity: 0, duration: motion.dur.xs, ease: motion.ease.inOut })
    .add(() => { tieuDe.replaceChildren(...kyTu); tieuDe.setAttribute('aria-label', s.tenSach); gsap.set(tieuDe, { yPercent: 0, opacity: 1 }); })
    .from(kyTu, { yPercent: 70, opacity: 0, duration: motion.dur.sm, ease: motion.ease.out, stagger: cf.titleStagger })
    .to([tacGia, meta], { opacity: 0, duration: motion.dur.xs }, 0)
    .add(() => { tacGia.textContent = s.tacGia || ''; meta.textContent = chuMeta(s); }, motion.dur.xs)
    .to([tacGia, meta], { opacity: 1, duration: motion.dur.sm }, motion.dur.xs);
}

/** Bìa lớn chỉ cho cuốn giữa; tải trước 3 cuốn kế bên (file 05 mục 7). */
function napBiaLon(i) {
  for (let k = Math.max(0, i - 3); k <= Math.min(sach.length - 1, i + 3); k++) {
    const b = sachLi[k];
    if (!b.s.anhBiaLon || b.daLon) continue;
    const img = b.mat.querySelector('img');
    const lon = new Image();
    lon.src = b.s.anhBiaLon;
    lon.decode?.().then(() => { img.src = b.s.anhBiaLon; b.daLon = true; }).catch(() => {});
  }
}

// ---------------- Thanh thời gian ----------------
const truc = $('cfTruc'), vach = $('cfVach'), cham = $('cfCham');
let giaTri = [], gtMin = 0, gtMax = 1;

function veTruc() {
  giaTri = sach.map((s, i) => (ke.truc === 'hang' ? s.hang : s.namXb ?? null));
  // năm thiếu → lấy theo vị trí để chấm vẫn chạy đều
  const coSo = giaTri.filter((v) => v != null);
  gtMin = coSo.length ? Math.min(...coSo) : 0;
  gtMax = coSo.length ? Math.max(...coSo) : 1;
  if (gtMax === gtMin) gtMax = gtMin + 1;
  giaTri = giaTri.map((v, i) => v ?? lerp(gtMin, gtMax, i / Math.max(1, sach.length - 1)));
  vach.replaceChildren();
  const buoc = ke.truc === 'hang' ? 1 : 1;
  const nhan = ke.truc === 'hang' ? (v) => v === 1 || v % 5 === 0 : (v) => v % 5 === 0;
  const dau = Math.ceil(gtMin), cuoi = Math.floor(gtMax);
  for (let v = dau; v <= cuoi; v += buoc) {
    const t = document.createElement('span');
    t.className = 'coverflow__tick' + (nhan(v) ? ' coverflow__tick--nhan' : '');
    t.style.left = ((v - gtMin) / (gtMax - gtMin)) * 100 + '%';
    if (nhan(v)) { const l = document.createElement('span'); l.textContent = ke.truc === 'hang' ? '#' + v : v; t.append(l); }
    vach.append(t);
  }
  truc.setAttribute('aria-valuemin', '1');
  truc.setAttribute('aria-valuemax', String(sach.length));
  truc.setAttribute('aria-label', ke.truc === 'hang' ? 'Thanh thứ hạng mượn nhiều' : 'Thanh thời gian theo năm xuất bản');
}

const datCham = window.gsap ? gsap.quickSetter(cham, 'left', '%') : (v) => { cham.style.left = v + '%'; };
function capNhatTruc(pos) {
  const i0 = clamp(Math.floor(pos), 0, sach.length - 1), i1 = clamp(i0 + 1, 0, sach.length - 1);
  const v = lerp(giaTri[i0], giaTri[i1], clamp(pos - i0, 0, 1));
  datCham(((v - gtMin) / (gtMax - gtMin)) * 100);
  const g = clamp(Math.round(pos), 0, sach.length - 1);
  truc.setAttribute('aria-valuenow', String(g + 1));
  const s = sach[g];
  truc.setAttribute('aria-valuetext', `${ke.truc === 'hang' ? 'Hạng ' + s.hang : (s.namXb ?? 'Chưa rõ năm')} – ${s.tenSach}`);
}

/** Cuốn có giá trị (năm/hạng) gần nhất với vị trí bấm trên thanh. */
function cuonTheoTruc(clientX) {
  const r = truc.getBoundingClientRect();
  const v = lerp(gtMin, gtMax, clamp((clientX - r.left) / r.width, 0, 1));
  let tot = 0, kc = Infinity;
  giaTri.forEach((g, i) => { const k = Math.abs(g - v); if (k < kc) { kc = k; tot = i; } });
  return tot;
}

// ---------------- Chuyển động: một tween `pos` ----------------
function thoiLuong(tu, den) {
  const kc = Math.abs(den - tu);
  if (kc <= 1.01) return cf.step;
  return Math.min(0.35 + cf.perDistance * kc, cf.maxJump);
}

function tweenToi(dich, thoiGian) {
  dich = clamp(dich, 0, sach.length - 1);
  tween?.kill();
  if (giamChuyenDong()) {
    // Giảm chuyển động: không xoay 3D khi chuyển – đổi cuốn bằng mờ chéo
    return new Promise((xong) => {
      tween = gsap.timeline({ onComplete: xong })
        .to(track, { opacity: 0, duration: motion.reduced.fade / 2 })
        .add(() => { st.pos = dich; layout(st.pos); })
        .to(track, { opacity: 1, duration: motion.reduced.fade / 2 });
    });
  }
  return new Promise((xong) => {
    tween = gsap.to(st, { pos: dich, duration: thoiGian ?? thoiLuong(st.pos, dich), ease: cf.ease, onUpdate: () => layout(st.pos), onComplete: xong });
  });
}

// ---------------- State machine (file 05 mục 5: KE_SAN_SANG ⇄ KE_DANG_XOAY, KE_DOI_KE, MO_SACH) ----------------
const buocDen = { den: 'KE_DANG_XOAY', khi: (d) => Number.isFinite(d?.dich), lam: (d) => tweenToi(d.dich, d.thoiGian), xong: 'KE_SAN_SANG' };
const buocDoiKe = { den: 'KE_DOI_KE', lam: (d) => doiKe(d.ma), xong: 'KE_SAN_SANG' };
const may = new MayTrangThai({
  ten: 'coverflow',
  batDau: 'KE_SAN_SANG',
  ranh: ['KE_SAN_SANG'],
  bang: {
    KE_SAN_SANG: { den: buocDen, doiKe: buocDoiKe, mo: { den: 'MO_SACH', lam: (d) => moSach(d.chiSo), xong: 'KE_SAN_SANG' } },
    // Lệnh đến khi đang xoay: cập nhật đích của tween hiện tại (không xếp hàng nhiều lệnh)
    KE_DANG_XOAY: { den: buocDen, doiKe: buocDoiKe },
    KE_DOI_KE: {},
    MO_SACH: {}
  },
  khiDoi: (tu, den) => { goc.dataset.trangThai = den; }
});
goc.dataset.trangThai = may.hienTai;

const denCuon = (i, thoiGian) => may.chuyen('den', { dich: i, thoiGian });
/** ±n cuốn tính từ ĐÍCH của tween đang chạy – bấm liên tục nhiều lần cộng dồn, không kẹt. */
function buoc(n) {
  const tu = may.hienTai === 'KE_DANG_XOAY' && tween?.isActive() && Number.isFinite(tween.vars?.pos) ? tween.vars.pos : st.pos;
  denCuon(clamp(Math.round(tu) + n, 0, sach.length - 1), cf.step);
}

// ---------------- Đổi kệ ----------------
const tenKe = $('tenKeHienTai');
let yeuCauKe = null;

async function doiKe(ma) {
  if (ma === ke.ma) return;
  tween?.kill();
  yeuCauKe?.abort();
  const dk = new AbortController();
  yeuCauKe = dk;
  const raKhoi = giamChuyenDong()
    ? gsap.to(track, { opacity: 0, duration: motion.reduced.fade })
    : gsap.to(track, { yPercent: -cf.shelfShiftPercent, opacity: 0, duration: cf.shelfSwitch / 2, ease: motion.ease.inOut });
  let moi;
  try {
    const r = await fetch('/api/cong/ke-sach?ke=' + encodeURIComponent(ma), { signal: dk.signal, headers: { Accept: 'application/json' } });
    if (!r.ok) throw new Error('Không tải được kệ sách.');
    moi = await r.json();
    await raKhoi;
  } catch (e) {
    if (e.name === 'AbortError') return;
    gsap.to(track, { yPercent: 0, opacity: 1, duration: motion.dur.sm });
    docTen.textContent = 'Không tải được kệ sách, vui lòng thử lại.';
    return;
  }
  if (dk !== yeuCauKe || !moi.sach?.length) { gsap.to(track, { yPercent: 0, opacity: 1, duration: motion.dur.sm }); return; }
  ke = moi;
  sach = moi.sach;
  st.pos = Math.floor((sach.length - 1) / 2);
  dungKe();
  layout(st.pos);
  danhDauKe();
  // Tên kệ đổi bằng hiệu ứng lật chữ
  if (!giamChuyenDong()) {
    gsap.timeline()
      .to(tenKe, { rotationX: 90, opacity: 0, duration: motion.dur.xs, transformOrigin: '50% 50% -8px' })
      .add(() => { tenKe.textContent = ke.ten; })
      .to(tenKe, { rotationX: 0, opacity: 1, duration: motion.dur.sm, ease: motion.ease.out });
    await gsap.fromTo(track, { yPercent: cf.shelfShiftPercent, opacity: 0 }, { yPercent: 0, opacity: 1, duration: cf.shelfSwitch / 2, ease: motion.ease.out });
  } else {
    tenKe.textContent = ke.ten;
    await gsap.to(track, { opacity: 1, duration: motion.reduced.fade });
  }
}

function danhDauKe() {
  document.querySelectorAll('.coverflow__chip').forEach((c) => c.setAttribute('aria-pressed', String(c.dataset.ke === ke.ma)));
  const i = danhSachKe.findIndex((k) => k.ma === ke.ma);
  $('cfKeTruoc').setAttribute('aria-label', 'Kệ trước: ' + danhSachKe[(i - 1 + danhSachKe.length) % danhSachKe.length].ten);
  $('cfKeSau').setAttribute('aria-label', 'Kệ sau: ' + danhSachKe[(i + 1) % danhSachKe.length].ten);
  // Điện thoại: đưa chip kệ đang chọn vào giữa hàng chip (chỉ cuộn ngang hàng chip, không cuộn trang)
  const hang = document.querySelector('.coverflow__chips');
  const chip = hang?.querySelector('[aria-pressed="true"]');
  if (hang && chip && hang.scrollWidth > hang.clientWidth)
    hang.scrollTo({ left: chip.offsetLeft - (hang.clientWidth - chip.offsetWidth) / 2, behavior: giamChuyenDong() ? 'auto' : 'smooth' });
}

function keLienKe(huong) {
  const i = danhSachKe.findIndex((k) => k.ma === ke.ma);
  return danhSachKe[(i + huong + danhSachKe.length) % danhSachKe.length].ma;
}

// ---------------- Mở sách (Book Preview ở Pha 6 – Pha 5: hộp “Xem nhanh”) ----------------
function moSach(i) {
  const s = sach[i];
  goc.dispatchEvent(new CustomEvent('coverflow:mo-sach', { detail: { sach: s, phanTu: sachLi[i].mat } }));
  return new Promise((xong) => {
    const el = document.createElement('div');
    el.className = 'modal fade cong-xem-nhanh';
    el.tabIndex = -1;
    el.setAttribute('aria-labelledby', 'xemNhanhTieuDe');
    el.innerHTML = `
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header"><h2 class="modal-title" id="xemNhanhTieuDe"></h2>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button></div>
          <div class="modal-body cong-xem-nhanh__than">
            <div class="cong-xem-nhanh__bia"></div>
            <dl class="cong-xem-nhanh__tt"></dl>
          </div>
        </div>
      </div>`;
    el.querySelector('h2').textContent = s.tenSach;
    const bia = el.querySelector('.cong-xem-nhanh__bia');
    const mau = mauDoc(s.mauGay);
    bia.style.setProperty('--mau', mau.nen);
    if (s.anhBiaLon) { const img = document.createElement('img'); img.src = s.anhBiaLon; img.alt = 'Bìa sách ' + s.tenSach; bia.append(img); }
    else { bia.append(biaChu(s)); if (mau.sang) bia.classList.add('book--sang'); }
    const dl = el.querySelector('dl');
    const dong = [['Tác giả', s.tacGia || '—'], ['Năm xuất bản', s.namXb ?? '—'], ['Thể loại', s.tenTheLoai || '—'],
      ['Còn có thể mượn', s.soBanCoTheMuon > 0 ? `${s.soBanCoTheMuon} bản` : 'Đang được mượn hết'], ['Lượt mượn 90 ngày', s.soLuotMuon]];
    for (const [k, v] of dong) {
      const dt = document.createElement('dt'); dt.textContent = k;
      const dd = document.createElement('dd'); dd.textContent = v;
      dl.append(dt, dd);
    }
    document.body.append(el);
    const modal = new bootstrap.Modal(el);
    el.addEventListener('hidden.bs.modal', () => { el.remove(); sachLi[i]?.nut.focus({ preventScroll: true }); xong(); });
    modal.show();
  });
}

// ---------------- Tương tác (file 05 mục 5) ----------------
$('cfTruoc').addEventListener('click', () => buoc(-1));
$('cfSau').addEventListener('click', () => buoc(1));
$('cfKeTruoc').addEventListener('click', () => may.chuyen('doiKe', { ma: keLienKe(-1) }));
$('cfKeSau').addEventListener('click', () => may.chuyen('doiKe', { ma: keLienKe(1) }));
document.querySelectorAll('.coverflow__chip').forEach((c) => c.addEventListener('click', () => may.chuyen('doiKe', { ma: c.dataset.ke })));

// Phím ←/→ khi khu vực có focus (trừ khi đang ở thanh thời gian – có phím riêng)
goc.addEventListener('keydown', (e) => {
  if (e.target.closest('.coverflow__chips')) return;
  if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
    e.preventDefault();
    buoc(e.key === 'ArrowLeft' ? -1 : 1);
    if (e.target.classList.contains('book__body')) requestAnimationFrame(() => sachLi[chiSoGiua]?.nut.focus({ preventScroll: true }));
  } else if (e.key === 'Home' && e.target === truc) { e.preventDefault(); denCuon(0); }
  else if (e.key === 'End' && e.target === truc) { e.preventDefault(); denCuon(sach.length - 1); }
});

// Cuộn chuột / vuốt touchpad NGANG: ±1 mỗi nấc, khóa 250ms; cuộn dọc vẫn cuộn trang
let khoaCuon = 0;
sanKhau.addEventListener('wheel', (e) => {
  if (Math.abs(e.deltaX) <= Math.abs(e.deltaY) || Math.abs(e.deltaX) < 4) return;
  e.preventDefault();
  const bay = performance.now();
  if (bay < khoaCuon) return;
  khoaCuon = bay + cf.wheelLock * 1000;
  buoc(e.deltaX > 0 ? 1 : -1);
}, { passive: false });

// Kéo / vuốt (Pointer Events, touch-action: pan-y): pos theo tay (1 cuốn = 0.6W px), thả ra có quán tính rồi bám cuốn gần nhất
let keo = null;
sanKhau.addEventListener('pointerdown', (e) => {
  if (e.button !== 0 || may.hienTai === 'KE_DOI_KE' || may.hienTai === 'MO_SACH') return;
  keo = { x0: e.clientX, pos0: st.pos, dangKeo: false, mau: [{ t: performance.now(), p: st.pos }], id: e.pointerId };
});
sanKhau.addEventListener('pointermove', (e) => {
  if (!keo || e.pointerId !== keo.id) return;
  const dx = e.clientX - keo.x0;
  if (!keo.dangKeo) {
    if (Math.abs(dx) < 6) return;
    keo.dangKeo = true;
    tween?.kill();
    try { sanKhau.setPointerCapture(e.pointerId); } catch { /* con trỏ không còn hoạt động */ }
    goc.classList.add('dang-keo');
  }
  st.pos = clamp(keo.pos0 - dx / (cf.dragPerBook * W), -0.4, sach.length - 0.6);
  keo.mau.push({ t: performance.now(), p: st.pos });
  if (keo.mau.length > 6) keo.mau.shift();
  veKhungSau();
});
// Gộp nhiều pointermove trong cùng một khung hình thành MỘT lần layout
let henVe = 0;
function veKhungSau() {
  if (henVe) return;
  henVe = requestAnimationFrame(() => { henVe = 0; layout(st.pos); });
}
function thaKeo(e) {
  if (!keo || e.pointerId !== keo.id) return;
  const k = keo;
  keo = null;
  goc.classList.remove('dang-keo');
  if (!k.dangKeo) {
    // Bấm (không kéo): gáy → xoay về giữa; cuốn giữa → mở
    const li = e.target.closest?.('.book');
    if (!li) return;
    const i = Number(li.dataset.index);
    if (i === Math.round(st.pos) && may.hienTai === 'KE_SAN_SANG') may.chuyen('mo', { chiSo: i });
    else denCuon(i);
    return;
  }
  const dau = k.mau[0], cuoi = k.mau[k.mau.length - 1];
  const v = (cuoi.p - dau.p) / Math.max(16, cuoi.t - dau.t) * 1000; // cuốn/giây
  let dich = Math.round(st.pos + v * cf.inertia);
  dich = clamp(dich, Math.round(k.pos0) - cf.swipeMaxJump, Math.round(k.pos0) + cf.swipeMaxJump);
  denCuon(clamp(dich, 0, sach.length - 1), clamp(0.3 + Math.abs(dich - st.pos) * cf.perDistance, 0.3, cf.maxJump));
}
sanKhau.addEventListener('pointerup', thaKeo);
sanKhau.addEventListener('pointercancel', thaKeo);
// Enter/Space trên cuốn giữa → mở; trên gáy (focus bằng chuột) → về giữa
track.addEventListener('keydown', (e) => {
  if (e.key !== 'Enter' && e.key !== ' ') return;
  const li = e.target.closest('.book');
  if (!li) return;
  e.preventDefault();
  const i = Number(li.dataset.index);
  if (i === chiSoGiua) may.chuyen('mo', { chiSo: i }); else denCuon(i);
});
// Ngăn “click” mặc định của nút (đã xử lý ở pointerup); vẫn cho bàn phím (detail = 0)
track.addEventListener('click', (e) => { if (e.detail > 0) e.preventDefault(); });

// Thanh thời gian: bấm vạch / kéo chấm → nhảy tới cuốn có năm (hạng) gần nhất
let keoTruc = false;
truc.addEventListener('pointerdown', (e) => {
  keoTruc = true;
  try { truc.setPointerCapture(e.pointerId); } catch { /* bỏ qua */ }
  denCuon(cuonTheoTruc(e.clientX));
});
truc.addEventListener('pointermove', (e) => { if (keoTruc) { const i = cuonTheoTruc(e.clientX); if (i !== Math.round(tween?.vars?.pos ?? st.pos)) denCuon(i, 0.35); } });
truc.addEventListener('pointerup', () => { keoTruc = false; });
truc.addEventListener('pointercancel', () => { keoTruc = false; });

// Hover gáy: nâng y −10px; cuốn giữa (chỉ máy có chuột): con trỏ tròn “Mở” đi theo chuột + bìa nghiêng theo chuột ±6°
const conTro = $('cfConTro');
let diChuyenConTro = null, nghieng = null, hoverLi = null;
if (coChuot() && !giamChuyenDong()) {
  gsap.set(conTro, { xPercent: -50, yPercent: -50 });
  diChuyenConTro = { x: gsap.quickTo(conTro, 'x', { duration: cf.cursorLag, ease: motion.ease.out }), y: gsap.quickTo(conTro, 'y', { duration: cf.cursorLag, ease: motion.ease.out }) };
  track.addEventListener('pointerover', (e) => {
    const li = e.target.closest('.book');
    if (li === hoverLi) return;
    if (hoverLi && Number(hoverLi.dataset.index) !== chiSoGiua) gsap.to(hoverLi.firstChild, { y: 0, duration: motion.dur.sm, ease: motion.ease.out });
    hoverLi = li;
    if (li && Number(li.dataset.index) !== chiSoGiua && !keo) gsap.to(li.firstChild, { y: -cf.hoverLift, duration: motion.dur.sm, ease: motion.ease.out });
  });
  track.addEventListener('pointerleave', () => {
    if (hoverLi) gsap.to(hoverLi.firstChild, { y: 0, rotationX: 0, rotationY: 0, duration: motion.dur.md, ease: motion.ease.out });
    hoverLi = null;
    goc.classList.remove('tren-cuon-giua');
  });
  sanKhau.addEventListener('pointermove', (e) => {
    const r = goc.getBoundingClientRect();
    diChuyenConTro.x(e.clientX - r.left);
    diChuyenConTro.y(e.clientY - r.top);
    const b = sachLi[chiSoGiua];
    const tren = !!b && !keo && e.target.closest('.book') === b.li;
    goc.classList.toggle('tren-cuon-giua', tren);
    if (tren) {
      const rb = b.nut.getBoundingClientRect();
      const px = clamp((e.clientX - rb.left) / rb.width, 0, 1) - 0.5, py = clamp((e.clientY - rb.top) / rb.height, 0, 1) - 0.5;
      nghieng ??= { x: gsap.quickTo(b.nut, 'rotationX', { duration: motion.dur.sm }), y: gsap.quickTo(b.nut, 'rotationY', { duration: motion.dur.sm }), nut: b.nut };
      if (nghieng.nut !== b.nut) { gsap.to(nghieng.nut, { rotationX: 0, rotationY: 0, duration: motion.dur.md }); nghieng = { x: gsap.quickTo(b.nut, 'rotationX', { duration: motion.dur.sm }), y: gsap.quickTo(b.nut, 'rotationY', { duration: motion.dur.sm }), nut: b.nut }; }
      nghieng.x(-py * 2 * cf.tiltMax);
      nghieng.y(px * 2 * cf.tiltMax);
    } else if (nghieng) {
      gsap.to(nghieng.nut, { rotationX: 0, rotationY: 0, duration: motion.dur.md, ease: motion.ease.out });
      nghieng = null;
    }
  });
}

// Kích thước: đo MỘT lần khi đổi cỡ (ResizeObserver, debounce)
let henDo = null;
new ResizeObserver(() => {
  clearTimeout(henDo);
  henDo = setTimeout(() => { doKichThuoc(); layout(st.pos); }, 120);
}).observe(goc);

// ---------------- Khởi động ----------------
goc.classList.add('cf-3d');
doKichThuoc();
dungKe();
layout(st.pos);
danhDauKe();
// Vào trang: các cuốn trượt từ tâm ra (chạy một lần khi khu vực vào khung nhìn)
if (window.gsap && !giamChuyenDong()) {
  const tam = Math.round(st.pos);
  const io = new IntersectionObserver((ds) => {
    if (!ds[0].isIntersecting) return;
    io.disconnect();
    gsap.from(sachLi.map((b) => b.nut), { opacity: 0, y: 24, duration: motion.dur.lg, ease: motion.ease.out,
      stagger: { each: cf.titleStagger * 2, from: tam }, clearProps: 'opacity,transform' });
  }, { threshold: 0.25 });
  io.observe(goc);
}
