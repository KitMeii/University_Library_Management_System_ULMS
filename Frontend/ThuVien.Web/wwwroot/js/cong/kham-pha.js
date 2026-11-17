// Khu 5 – Khám phá sách (file 04 mục 1, 4; file 10 mục 3) + thanh tìm kiếm Hero, khu 4 Danh mục thể loại, khu 7 Thư viện bằng số.
// - Đổi bộ lọc/tìm: KHÔNG đổi route, không tải lại trang; danh sách cũ giữ nguyên trong lúc tải; yêu cầu mới nhất thắng
//   (AbortController + requestId). Lưới đổi bằng FLIP tự viết (GSAP core): thẻ rời đi mờ + thu 0.96, thẻ còn lại dịch chỗ, thẻ mới vào stagger.
// - Thẻ sách (Book Card – file 04 mục 4): bấm → Book Preview (data-mo-sach, book-preview.js); nút tim → yêu thích.
import { motion, giamChuyenDong } from '../motion.config.js';
import { api, chuoiTruyVan, LoiApi } from '../api.js';

const $ = (id) => document.getElementById(id);
const goc = $('kham-pha');
const luoi = $('kpLuoi');
const ketQua = $('kpKetQua');
const nutXemThem = $('btnXemThem');
const form = $('frmKhamPha');
const gsap = window.gsap;
const CO_TRANG = 12;
const laBanDoc = () => document.body.dataset.banDoc === '1';

let yeuThich = new Set();
let lanGoi = 0;
let huy = null;
let trang = 1;
let tongSo = 0;

// ───────────── Bộ lọc ─────────────
function thamSo() {
  const p = { q: $('txtTimSach').value.trim(), theLoai: $('cboTheLoai').value, sapXep: $('cboSapXep').value,
    conSach: $('chkConSach').checked ? 'true' : '' };
  const nam = $('cboNam')?.value;
  if (nam) { const [tu, den] = nam.split('-'); p.namTu = tu; p.namDen = den; }
  if ($('txtNamTu')) { p.namTu = $('txtNamTu').value; p.namDen = $('txtNamDen').value; }
  return p;
}

function moTaKetQua(n, p) {
  const phan = [];
  if (p.q) phan.push(`“${p.q}”`);
  const tl = $('cboTheLoai');
  if (p.theLoai) phan.push(tl.options[tl.selectedIndex].text.replace(/\s*\(\d+\)$/, ''));
  if (p.conSach) phan.push('còn có thể mượn');
  return n === 0
    ? `Không tìm thấy sách phù hợp${phan.length ? ' với ' + phan.join(', ') : ''}. Thử từ khóa khác hoặc bỏ bớt bộ lọc.`
    : `${n.toLocaleString('vi-VN')} đầu sách${phan.length ? ' – ' + phan.join(', ') : ''}`;
}

// ───────────── Thẻ sách ─────────────
function doSang(hex) {
  const m = /^#?([0-9a-f]{6})$/i.exec(hex || '');
  if (!m) return 0;
  const [r, g, b] = [0, 2, 4].map((i) => parseInt(m[1].slice(i, i + 2), 16) / 255)
    .map((v) => (v <= 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4)));
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

function taoThe(s) {
  const the = document.createElement('article');
  the.className = 'the-sach';
  the.dataset.ma = s.maDauSach;
  the.dataset.moSach = s.maDauSach;
  const mo = document.createElement('button');
  mo.type = 'button';
  mo.className = 'the-sach__mo';
  mo.dataset.moSach = s.maDauSach;
  // Tên đọc là chữ ẩn trong nút (không dùng aria-label: chữ trên bìa chữ phải nằm trong tên – axe label-content-name-mismatch)
  const tenDoc = document.createElement('span');
  tenDoc.className = 'sr-only';
  tenDoc.textContent = `Mở sách ${s.tenSach}${s.tacGia ? ' – ' + s.tacGia : ''}`;
  mo.append(tenDoc);
  const bia = document.createElement('span');
  bia.className = 'the-sach__bia';
  bia.setAttribute('aria-hidden', 'true');
  if (s.anhBiaNho || s.anhBiaLon) {
    const img = document.createElement('img');
    img.src = s.anhBiaNho || s.anhBiaLon; img.alt = ''; img.loading = 'lazy'; img.decoding = 'async';
    img.width = 240; img.height = 360;
    bia.append(img);
  } else {
    const mau = s.mauGay || '#176B54';
    bia.classList.add('the-sach__bia--chu', doSang(mau) > 0.45 ? 'sang' : 'toi');
    bia.style.setProperty('--mau-bia', mau); // CSSOM – CSP cho phép
    const ten = document.createElement('span'); ten.className = 'the-sach__bia-ten'; ten.textContent = s.tenSach;
    const tg = document.createElement('span'); tg.className = 'the-sach__bia-tg'; tg.textContent = s.tacGia || '';
    bia.append(ten, tg);
  }
  mo.append(bia);
  const tt = document.createElement('div');
  tt.className = 'the-sach__tt';
  const h = document.createElement('h3'); h.className = 'the-sach__ten'; h.textContent = s.tenSach;
  const p = document.createElement('p'); p.className = 'the-sach__phu';
  p.textContent = [s.tacGia, s.namXb].filter(Boolean).join(' · ');
  const nhan = document.createElement('span');
  nhan.className = 'the-sach__nhan ' + (s.soBanCoTheMuon > 0 ? 'con' : 'het');
  nhan.textContent = s.soBanCoTheMuon > 0 ? `Còn ${s.soBanCoTheMuon} bản có thể mượn` : 'Đang được mượn hết';
  tt.append(h, p, nhan);
  const tim = document.createElement('button');
  tim.type = 'button';
  tim.className = 'the-sach__tim';
  datTim(tim, s, yeuThich.has(s.maDauSach));
  tim.addEventListener('click', (e) => { e.preventDefault(); e.stopPropagation(); doiYeuThich(tim, s); });
  the.append(mo, tt, tim);
  return the;
}

function datTim(nut, s, co) {
  nut.setAttribute('aria-pressed', co ? 'true' : 'false');
  nut.setAttribute('aria-label', (co ? 'Bỏ yêu thích ' : 'Yêu thích ') + s.tenSach);
  nut.innerHTML = `<i class="bi ${co ? 'bi-heart-fill' : 'bi-heart'}" aria-hidden="true"></i>`;
}

async function doiYeuThich(nut, s) {
  if (!laBanDoc()) { location.href = '/dang-nhap?returnUrl=' + encodeURIComponent(location.pathname + location.search + '#kham-pha'); return; }
  const co = !yeuThich.has(s.maDauSach);
  try {
    const url = '/api/cong/yeu-thich/' + encodeURIComponent(s.maDauSach);
    const kq = co ? await api.sua(url) : await api.xoa(url);
    if (kq.yeuThich) yeuThich.add(s.maDauSach); else yeuThich.delete(s.maDauSach);
    datTim(nut, s, kq.yeuThich);
    document.dispatchEvent(new CustomEvent('ulms:yeu-thich-doi', { detail: { so: yeuThich.size } }));
    if (kq.yeuThich && gsap && !giamChuyenDong()) gsap.fromTo(nut.firstChild, { scale: 0.4 }, { scale: 1, duration: motion.dur.md, ease: motion.ease.back });
  } catch (e) { if (e instanceof LoiApi) alert(e.message); }
}

// ───────────── Tải và vẽ lưới ─────────────
async function tai({ them = false } = {}) {
  const p = thamSo();
  const id = ++lanGoi;
  huy?.abort();
  huy = new AbortController();
  luoi.setAttribute('aria-busy', 'true');
  goc.classList.add('kp--dang-tai'); // danh sách cũ giữ nguyên, chỉ mờ nhẹ
  const t = them ? trang + 1 : 1;
  try {
    const kq = await api.lay('/api/cong/sach' + chuoiTruyVan({ ...p, trang: t, coTrang: CO_TRANG }), { signal: huy.signal });
    if (id !== lanGoi) return; // đã có yêu cầu mới hơn
    trang = t;
    tongSo = kq.tongSo;
    if (them) themThe(kq.sach); else await doiLuoi(kq.sach);
    ketQua.textContent = moTaKetQua(kq.tongSo, p);
    nutXemThem.hidden = trang * CO_TRANG >= tongSo;
  } catch (e) {
    if (e.name === 'AbortError' || id !== lanGoi) return;
    ketQua.textContent = e.message || 'Không tải được danh sách sách.';
  } finally {
    if (id === lanGoi) { goc.classList.remove('kp--dang-tai'); luoi.setAttribute('aria-busy', 'false'); }
  }
}

/**
 * Thay lưới bằng hiệu ứng FLIP tự viết với GSAP core (không dùng plugin Flip – CSP chặt, GIA_DINH D78 đổi 17/11/2026):
 * giữ phần tử của sách còn lại, đo vị trí trước (First) – thay DOM – đo sau (Last) – dịch ngược (Invert) – chạy về 0 (Play).
 * Chỉ dùng transform/opacity (CSSOM, hợp CSP).
 */
async function doiLuoi(ds) {
  const cu = new Map([...luoi.children].map((c) => [c.dataset.ma, c]));
  const moi = ds.map((s) => cu.get(s.maDauSach) ?? taoThe(s));
  const roi = [...cu.values()].filter((c) => !moi.includes(c));
  if (!gsap || giamChuyenDong()) { luoi.replaceChildren(...moi); return; }
  if (roi.length) await gsap.to(roi, { opacity: 0, scale: 0.96, duration: motion.dur.xs, ease: motion.ease.out }).then();
  const giu = moi.filter((c) => cu.has(c.dataset.ma));
  const truoc = new Map(giu.map((c) => [c, c.getBoundingClientRect()]));
  luoi.replaceChildren(...moi);
  for (const c of giu) {
    const a = truoc.get(c), b = c.getBoundingClientRect();
    const dx = a.left - b.left, dy = a.top - b.top;
    if (dx || dy) gsap.fromTo(c, { x: dx, y: dy }, { x: 0, y: 0, duration: motion.dur.md, ease: motion.ease.inOut, clearProps: 'transform' });
  }
  const vao = moi.filter((c) => !cu.has(c.dataset.ma));
  if (vao.length) gsap.fromTo(vao, { opacity: 0, y: 16 }, { opacity: 1, y: 0, duration: motion.dur.md, ease: motion.ease.out,
    stagger: motion.stagger.tight, clearProps: 'transform,opacity' });
}

function themThe(ds) {
  const vao = ds.map(taoThe);
  luoi.append(...vao);
  if (gsap && !giamChuyenDong()) gsap.fromTo(vao, { opacity: 0, y: 16 }, { opacity: 1, y: 0, duration: motion.dur.md,
    ease: motion.ease.out, stagger: motion.stagger.normal, clearProps: 'transform,opacity' });
  vao[0]?.querySelector('.the-sach__mo')?.focus({ preventScroll: true });
}

/** Cuộn tới khu 5 (Lenis nếu đang bật) – trừ chiều cao header. */
function cuonToiKhamPha() {
  if (!goc || goc.classList.contains('kham-pha--toan-trang')) return;
  const header = document.querySelector('.cong-header')?.offsetHeight ?? 0;
  const dich = goc.getBoundingClientRect().top + window.scrollY - header - 8;
  if (window.__lenis) window.__lenis.scrollTo(dich);
  else window.scrollTo({ top: dich, behavior: giamChuyenDong() ? 'auto' : 'smooth' });
}

// ───────────── Gắn sự kiện ─────────────
if (goc) {
  form.addEventListener('submit', (e) => { e.preventDefault(); tai(); });
  for (const id of ['cboTheLoai', 'cboSapXep', 'chkConSach', 'cboNam']) $(id)?.addEventListener('change', () => tai());
  for (const id of ['txtNamTu', 'txtNamDen']) $(id)?.addEventListener('change', () => tai());
  // Gõ tìm: chờ người dùng dừng gõ 350ms
  let hen;
  $('txtTimSach').addEventListener('input', () => { clearTimeout(hen); hen = setTimeout(() => tai(), 350); });
  nutXemThem.addEventListener('click', () => tai({ them: true }));

  // Thanh tìm kiếm Hero + gợi ý từ khóa: tìm ngay tại khu 5 (không đổi route)
  $('frmTimHero')?.addEventListener('submit', (e) => {
    e.preventDefault();
    $('txtTimSach').value = $('txtTimHero').value.trim();
    tai();
    cuonToiKhamPha();
  });
  document.querySelectorAll('#goiYTuKhoa [data-tu-khoa]').forEach((a) => a.addEventListener('click', (e) => {
    e.preventDefault();
    $('txtTimHero').value = $('txtTimSach').value = a.dataset.tuKhoa;
    tai();
    cuonToiKhamPha();
  }));
  // Khu 4: bấm ô thể loại → lọc + cuộn tới khu 5
  document.querySelectorAll('.danh-muc__o[data-the-loai]').forEach((a) => a.addEventListener('click', (e) => {
    e.preventDefault();
    $('cboTheLoai').value = a.dataset.theLoai;
    // Chọn thể loại là bắt đầu duyệt mới: bỏ từ khóa đang tìm
    $('txtTimSach').value = '';
    if ($('txtTimHero')) $('txtTimHero').value = '';
    tai();
    cuonToiKhamPha();
  }));

  // Menu thả xuống “Danh mục” ở header (header.js) → lọc tại chỗ như bấm ô thể loại
  document.addEventListener('ulms:loc-the-loai', (e) => {
    $('cboTheLoai').value = e.detail;
    $('txtTimSach').value = '';
    if ($('txtTimHero')) $('txtTimHero').value = '';
    tai();
    cuonToiKhamPha();
  });

  (async () => {
    if (laBanDoc()) {
      try { yeuThich = new Set((await api.lay('/api/cong/yeu-thich')).map((x) => x.maDauSach)); } catch { /* không chặn */ }
    }
    await tai();
  })();
}

// ───────────── Khu 7: Thư viện bằng số – đếm lên khi cuộn tới (một lần) ─────────────
const so = document.querySelectorAll('[data-dem-len]');
if (so.length && gsap && window.ScrollTrigger && !giamChuyenDong()) {
  so.forEach((el) => {
    const cuoi = Number(el.dataset.demLen) || 0;
    const o = { v: 0 };
    el.textContent = '0';
    gsap.to(o, { v: cuoi, duration: motion.dashboard.countUp, ease: motion.ease.out,
      onUpdate: () => { el.textContent = Math.round(o.v).toLocaleString('vi-VN'); },
      scrollTrigger: { trigger: el, start: 'top 90%', once: true } });
  });
}
