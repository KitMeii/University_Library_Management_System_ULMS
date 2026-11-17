// Book Preview – nơi DUY NHẤT xem thông tin sách (file 06 mục 1, 6). Mọi điểm vào (cuốn giữa kệ coverflow, phần tử
// [data-mo-sach], yêu thích, thông báo, liên kết sâu /sach/{maDauSach}) đều mở cùng một hộp này.
// Hiệu ứng: bìa bay từ vị trí nguồn ra giữa màn hình → bìa mở quanh gáy (rotateY 0 → −180°) → lật nhanh 3 tờ giấy →
// dừng ở hai trang giấy (--paper). Lật trang: StPageFlip (page-flip 2.0.7) – máy tính 2 trang, điện thoại 1 trang vuốt lật.
// Đóng: ×, Esc, bấm nền, Back trình duyệt (history.pushState) → sách gập lại, bay về đúng vị trí nguồn (đo lại lúc đóng).
// Giảm chuyển động: mờ dần motion.dur.sm (0.25s), không bay, không lật.
import { motion, giamChuyenDong } from '../motion.config.js';
import { api, LoiApi } from '../api.js';
import { veMaVach } from './ma-vach.js';
import { PageFlip } from '/lib/page-flip/2.0.7/page-flip.module.js';

const gsap = window.gsap;
const p = motion.preview;
const NHAC_GIO = 2; // đếm ngược chuyển màu cảnh báo khi < 2 giờ (ThuVien:DatCho:NhacTruocGio)

let hienTai = null;          // phiên preview đang mở
let daDayLichSu = false;     // đã pushState /sach/{ma} cho phiên hiện tại
let dangNhapTrongPreview = false;

const laBanDoc = () => document.body.dataset.banDoc === '1';
const dienThoai = () => window.matchMedia('(max-width: 767.98px)').matches;
const h = (tag, lop, chu) => { const e = document.createElement(tag); if (lop) e.className = lop; if (chu !== undefined) e.textContent = chu; return e; };

// ───────────────────────────── Bìa sách ─────────────────────────────
function doSang(hexMau) {
  const m = /^#?([0-9a-f]{6})$/i.exec(hexMau || '');
  if (!m) return 0;
  const [r, g, b] = [0, 2, 4].map((i) => parseInt(m[1].slice(i, i + 2), 16) / 255)
    .map((v) => (v <= 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4)));
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

/** Bìa: ảnh thật nếu có, ngược lại bìa chữ trên nền màu gáy (giống kệ coverflow). */
function taoBia(s, lon) {
  const khung = h('div', 'bp-bia');
  const src = lon ? (s.anhBiaLon || s.anhBiaNho) : (s.anhBiaNho || s.anhBiaLon);
  if (src) {
    const img = h('img');
    img.src = src; img.alt = ''; img.decoding = 'async';
    khung.append(img);
  } else {
    const mau = s.mauGay || '#176B54';
    khung.classList.add('bp-bia--chu', doSang(mau) > 0.45 ? 'bp-bia--sang' : 'bp-bia--toi');
    khung.style.setProperty('--mau-bia', mau);
    khung.append(h('span', 'bp-bia__nhan', 'THƯ VIỆN'), h('span', 'bp-bia__ten', s.tenSach), h('span', 'bp-bia__gach'),
      h('span', 'bp-bia__tac-gia', s.tacGia || ''));
  }
  return khung;
}

// ───────────────────────────── Khung DOM ─────────────────────────────
function taoKhung() {
  const goc = h('div', 'bp');
  goc.setAttribute('role', 'dialog');
  goc.setAttribute('aria-modal', 'true');
  goc.setAttribute('aria-labelledby', 'bpTieuDe');
  goc.innerHTML = `
    <div class="bp__nen" data-dong="1"></div>
    <button type="button" class="bp__dong" id="btnDongSach" aria-label="Đóng sách"><i class="bi bi-x-lg" aria-hidden="true"></i></button>
    <div class="bp__san" data-dong="1">
      <div class="bp__sach">
        <div class="bp__trang-goc"><div class="bp__flip"></div></div>
        <div class="bp__to-giay" aria-hidden="true"><span></span><span></span><span></span></div>
        <div class="bp__bia-bay" aria-hidden="true"><div class="bp__bia-mat"><div class="bp__bia-ngoai"></div><div class="bp__bia-trong"></div></div></div>
      </div>
    </div>
    <div class="bp__ghim"><button type="button" class="bp-nut bp-nut--chinh" id="btnDatChoGhim"><i class="bi bi-bookmark-plus" aria-hidden="true"></i> Đặt chỗ</button></div>
    <div class="bp__thong-bao visually-hidden" aria-live="polite"></div>`;
  return goc;
}

function kichThuoc() {
  const W = window.innerWidth, H = window.innerHeight;
  if (dienThoai()) {
    const pw = Math.min(W - 32, 460);
    return { pw, ph: Math.max(380, H - 148), haiTrang: false };
  }
  let ph = Math.min(H * 0.82, 680);
  let pw = ph * 0.72;
  const toiDa = (W - 140) / 2;
  if (pw > toiDa) { pw = toiDa; ph = pw / 0.72; }
  return { pw: Math.round(pw), ph: Math.round(ph), haiTrang: true };
}

// ───────────────────────────── Nội dung hai trang ─────────────────────────────
function trangTrai(s) {
  const t = h('div', 'bp-trang bp-trang--trai');
  const noi = h('div', 'bp-trang__noi');
  noi.append(taoBia(s, true));
  const ten = h('h2', 'bp-ten', s.tenSach); ten.id = 'bpTieuDe';
  noi.append(ten);
  if (s.tacGia) noi.append(h('p', 'bp-tac-gia', s.tacGia));
  const dl = h('dl', 'bp-tt');
  for (const [k, v] of [['Nhà xuất bản', s.nhaXuatBan], ['Năm xuất bản', s.namXb], ['ISBN', s.isbn], ['Thể loại', s.tenTheLoai]]) {
    if (v === null || v === undefined || v === '') continue;
    dl.append(h('dt', null, k), h('dd', null, String(v)));
  }
  noi.append(dl);
  if (s.tuKhoa?.length) {
    const tk = h('div', 'bp-tu-khoa');
    tk.setAttribute('aria-label', 'Từ khóa');
    // Tìm theo từ khóa: trang Khám phá sách ở Pha 7 – hiện là nhãn
    for (const k of s.tuKhoa) tk.append(h('span', 'bp-chip', k));
    noi.append(tk);
  }
  const goiY = h('p', 'bp-goi-y-lat');
  goiY.innerHTML = 'Vuốt sang trái để xem giới thiệu và đặt chỗ <i class="bi bi-arrow-right" aria-hidden="true"></i>';
  noi.append(goiY);
  t.append(noi, h('span', 'bp-so-trang', '1'));
  return t;
}

function trangPhai(s) {
  const t = h('div', 'bp-trang bp-trang--phai');
  const noi = h('div', 'bp-trang__noi');
  noi.append(h('h3', 'bp-muc', 'Giới thiệu'));
  const gt = h('div', 'bp-gioi-thieu', s.gioiThieu || 'Chưa có phần giới thiệu cho đầu sách này.');
  gt.tabIndex = 0;
  gt.setAttribute('aria-label', 'Giới thiệu sách');
  noi.append(gt);

  const tt = h('div', 'bp-tinh-trang');
  tt.id = 'bpTinhTrang';
  noi.append(tt);
  const hd = h('div', 'bp-hanh-dong');
  hd.id = 'bpHanhDong';
  noi.append(hd);
  noi.append(h('p', 'bp-quy-dinh', 'Giữ sách 24 giờ – nhận tại quầy mượn trả'));
  t.append(noi, h('span', 'bp-so-trang', '2'));
  // StPageFlip bắt chạm/nhấn trên cả trang để lật: chặn ở vùng cuộn và khối hành động (ô nhập, nút) để gõ/cuộn bình thường
  for (const vung of [gt, hd])
    for (const ev of ['mousedown', 'touchstart', 'pointerdown']) vung.addEventListener(ev, (e) => e.stopPropagation(), { passive: true });
  return t;
}

function veTinhTrang(phien) {
  const s = phien.sach;
  const el = phien.goc.querySelector('#bpTinhTrang');
  el.replaceChildren();
  const so = s.soBanCoTheMuon ?? 0;
  const dong = h('p', 'bp-tinh-trang__so ' + (so > 0 ? 'con' : 'het'));
  dong.append(h('i', 'bi ' + (so > 0 ? 'bi-check-circle-fill' : 'bi-hourglass-split')));
  dong.firstChild.setAttribute('aria-hidden', 'true');
  dong.append(document.createTextNode(so > 0 ? ` Còn ${so} bản có thể mượn` : ' Hiện đã được mượn hoặc giữ chỗ hết'));
  el.append(dong);
  if (s.khuVuc?.length) {
    const ul = h('ul', 'bp-khu-vuc');
    for (const k of s.khuVuc) {
      const li = h('li');
      li.append(h('span', 'bp-khu-vuc__loai', k.loaiKhuVuc), document.createTextNode(` ${k.viTri} · ${k.soBan} bản`));
      ul.append(li);
    }
    el.append(ul);
  }
}

// ───────────────────────────── Khối hành động (file 06 mục 6) ─────────────────────────────
function veHanhDong(phien, trangThai, thamSo = {}) {
  phien.trangThai = trangThai;
  const el = phien.goc.querySelector('#bpHanhDong');
  dungDemNguoc(phien);
  el.replaceChildren();
  const ghim = phien.goc.querySelector('.bp__ghim');
  ghim.hidden = !(trangThai === 'mac-dinh' || trangThai === 'loi');

  if (trangThai === 've') return veVe(phien, el, thamSo);
  if (trangThai === 'dang-nhap') return veDangNhap(phien, el, thamSo);

  if (trangThai === 'loi' && thamSo.loi) {
    const box = h('div', 'bp-ly-do');
    box.setAttribute('role', 'alert');
    box.id = 'bpLyDo';
    box.append(h('p', 'bp-ly-do__chinh', thamSo.loi.message));
    if (thamSo.loi.truong?.huongXuLy) box.append(h('p', 'bp-ly-do__huong', thamSo.loi.truong.huongXuLy));
    el.append(box);
  }
  const nut = h('div', 'bp-nut-hang');
  const dat = h('button', 'bp-nut bp-nut--chinh');
  dat.type = 'button'; dat.id = 'btnDatCho';
  dat.innerHTML = '<span class="bp-nut__xoay" aria-hidden="true"></span><i class="bi bi-bookmark-plus" aria-hidden="true"></i> <span>Đặt chỗ</span>';
  dat.addEventListener('click', () => datCho(phien));
  const yt = h('button', 'bp-nut bp-nut--phu bp-nut--tron');
  yt.type = 'button'; yt.id = 'btnYeuThich';
  yt.setAttribute('aria-pressed', phien.yeuThich ? 'true' : 'false');
  yt.setAttribute('aria-label', phien.yeuThich ? 'Bỏ yêu thích' : 'Yêu thích');
  yt.innerHTML = `<i class="bi ${phien.yeuThich ? 'bi-heart-fill' : 'bi-heart'}" aria-hidden="true"></i>`;
  yt.addEventListener('click', () => doiYeuThich(phien));
  const cs = h('button', 'bp-nut bp-nut--phu bp-nut--tron');
  cs.type = 'button'; cs.id = 'btnChiaSe';
  cs.setAttribute('aria-label', 'Chia sẻ liên kết');
  cs.innerHTML = '<i class="bi bi-share" aria-hidden="true"></i>';
  cs.addEventListener('click', () => chiaSe(phien));
  nut.append(dat, yt, cs);
  el.append(nut);
}

function veDangNhap(phien, el, { tiepTheo }) {
  const f = h('form', 'bp-dang-nhap');
  f.id = 'frmDangNhapNhanh';
  f.noValidate = true;
  f.innerHTML = `
    <p class="bp-dang-nhap__tieu-de">Đăng nhập để ${tiepTheo === 'yeu-thich' ? 'lưu sách yêu thích' : 'đặt chỗ'}</p>
    <div class="bp-loi-dn" role="alert" hidden></div>
    <label for="bpMaSv">Mã SV/HV/CB</label>
    <input class="bp-o" id="bpMaSv" name="maSvHvCb" autocomplete="username" maxlength="15" required />
    <label for="bpMatKhau">Mật khẩu</label>
    <input class="bp-o" id="bpMatKhau" name="matKhau" type="password" autocomplete="current-password" required />
    <div class="bp-nut-hang">
      <button type="submit" class="bp-nut bp-nut--chinh" id="btnDangNhapNhanh"><span class="bp-nut__xoay" aria-hidden="true"></span><span>Đăng nhập${tiepTheo === 'yeu-thich' ? '' : ' và đặt chỗ'}</span></button>
      <button type="button" class="bp-nut bp-nut--phu" id="btnHuyDangNhap">Để sau</button>
    </div>
    <p class="bp-dang-nhap__phu">Chưa có mật khẩu? <a href="/kich-hoat">Kích hoạt tài khoản</a></p>`;
  el.append(f);
  const loi = f.querySelector('.bp-loi-dn');
  f.querySelector('#btnHuyDangNhap').addEventListener('click', () => veHanhDong(phien, 'mac-dinh'));
  f.addEventListener('submit', async (e) => {
    e.preventDefault();
    const nut = f.querySelector('#btnDangNhapNhanh');
    const maSvHvCb = f.maSvHvCb.value.trim(), matKhau = f.matKhau.value;
    if (!maSvHvCb || !matKhau) { loi.textContent = 'Vui lòng nhập mã SV/HV/CB và mật khẩu.'; loi.hidden = false; return; }
    dangXuLy(nut, true);
    try {
      const res = await fetch('/dang-nhap/nhanh', {
        method: 'POST', credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json',
          'RequestVerificationToken': document.querySelector('meta[name="csrf-token"]')?.content || '' },
        body: JSON.stringify({ maSvHvCb, matKhau })
      });
      const d = await res.json().catch(() => null);
      if (!res.ok) throw new LoiApi(res.status, d?.ma || 'LOI', d?.thongBao || 'Đăng nhập không thành công.');
      // Token AntiForgery mới gắn với danh tính bạn đọc – các lệnh ghi sau đó dùng token này
      document.querySelector('meta[name="csrf-token"]')?.setAttribute('content', d.csrf);
      document.body.dataset.banDoc = '1';
      dangNhapTrongPreview = true;
      thongBaoSr(phien, `Đã đăng nhập: ${d.hoTen}`);
      await napCuaToi(phien);
      if (phien.datCho) { veHanhDong(phien, 've', { moi: false }); return; }
      if (tiepTheo === 'yeu-thich') { veHanhDong(phien, 'mac-dinh'); await doiYeuThich(phien); }
      else await datCho(phien);
    } catch (err) {
      loi.textContent = err.message; loi.hidden = false;
      dangXuLy(nut, false);
    }
  });
  requestAnimationFrame(() => f.maSvHvCb.focus({ preventScroll: true }));
}

function veVe(phien, el, { moi }) {
  const d = phien.datCho;
  const ve = h('div', 'bp-ve');
  ve.id = 'veGiuCho';
  ve.innerHTML = `
    <div class="bp-ve__dau"><span class="bp-ve__nhan">Vé giữ chỗ</span><span class="bp-ve__ma"></span></div>
    <div class="bp-ve__vach"></div>
    <dl class="bp-ve__tt">
      <dt>Hạn giữ</dt><dd class="bp-ve__han"></dd>
      <dt>Còn lại</dt><dd class="bp-ve__dem" aria-live="off"></dd>
      <dt>Nhận tại</dt><dd class="bp-ve__quay"></dd>
      <dt>Bản sách</dt><dd class="bp-ve__ban"></dd>
    </dl>
    <span class="bp-ve__tem" aria-hidden="true">ĐÃ GIỮ CHỖ</span>`;
  ve.querySelector('.bp-ve__ma').textContent = d.maDatCho;
  ve.querySelector('.bp-ve__vach').append(veMaVach(d.maDatCho, { nhan: 'Mã vạch đặt chỗ ' + d.maDatCho }));
  ve.querySelector('.bp-ve__han').textContent = ngayGio(d.tgHetHan);
  ve.querySelector('.bp-ve__quay').textContent = d.quayNhan;
  ve.querySelector('.bp-ve__ban').textContent = `${d.maBanSach} · ${d.viTri}`;
  el.append(ve);
  const huy = h('button', 'bp-nut bp-nut--phu bp-nut--rong');
  huy.type = 'button'; huy.id = 'btnHuyDatCho';
  huy.innerHTML = '<i class="bi bi-x-circle" aria-hidden="true"></i> Hủy đặt chỗ';
  huy.addEventListener('click', () => huyDatCho(phien, huy));
  el.append(huy);
  batDemNguoc(phien, ve.querySelector('.bp-ve__dem'), new Date(d.tgHetHan));
  if (moi && !giamChuyenDong()) dongDau(ve);
}

/** Tem “ĐÃ GIỮ CHỖ” đóng xuống + tia nhỏ (file 06 mục 6). */
function dongDau(ve) {
  const tem = ve.querySelector('.bp-ve__tem');
  gsap.fromTo(tem, { scale: 2.4, opacity: 0, rotate: -26 }, { scale: 1, opacity: 1, rotate: -12, duration: motion.dur.md, ease: motion.ease.back });
  for (let i = 0; i < 10; i++) {
    const tia = h('span', 'bp-ve__tia');
    ve.append(tia);
    const goc = (i / 10) * Math.PI * 2;
    gsap.fromTo(tia, { x: 0, y: 0, opacity: 1, scale: 1 },
      { x: Math.cos(goc) * 46, y: Math.sin(goc) * 30, opacity: 0, scale: 0.4, duration: motion.dur.lg, delay: motion.dur.sm,
        ease: motion.ease.out, onComplete: () => tia.remove() });
  }
}

// Đếm ngược: đổi số kiểu lật mỗi phút, chuyển màu cảnh báo khi < 2 giờ (file 10)
function batDemNguoc(phien, el, hetHan) {
  let phutTruoc = null;
  const cap = () => {
    const con = hetHan - Date.now();
    if (con <= 0) { el.textContent = 'Đã hết hạn'; el.classList.add('canh-bao'); dungDemNguoc(phien); return; }
    const phut = Math.floor(con / 60000);
    if (phut === phutTruoc) return;
    phutTruoc = phut;
    el.textContent = `${Math.floor(phut / 60)} giờ ${String(phut % 60).padStart(2, '0')} phút`;
    el.classList.toggle('canh-bao', con < NHAC_GIO * 3600000);
    if (!giamChuyenDong()) gsap.fromTo(el, { rotateX: -80, opacity: 0.3 }, { rotateX: 0, opacity: 1, duration: motion.dur.sm, ease: motion.ease.out });
  };
  cap();
  phien.demNguoc = setInterval(cap, 1000);
}
function dungDemNguoc(phien) { if (phien.demNguoc) { clearInterval(phien.demNguoc); phien.demNguoc = null; } }

const ngayGio = (s) => {
  const d = new Date(s);
  const p2 = (n) => String(n).padStart(2, '0');
  return `${p2(d.getHours())}:${p2(d.getMinutes())} ${p2(d.getDate())}/${p2(d.getMonth() + 1)}/${d.getFullYear()}`;
};

function dangXuLy(nut, co) {
  nut.disabled = co;
  nut.classList.toggle('dang-xu-ly', co);
  nut.setAttribute('aria-busy', co ? 'true' : 'false');
}
function thongBaoSr(phien, chu) { phien.goc.querySelector('.bp__thong-bao').textContent = chu; }

// ───────────────────────────── Nghiệp vụ ─────────────────────────────
async function napCuaToi(phien) {
  if (!laBanDoc()) { phien.datCho = null; phien.yeuThich = false; return; }
  try {
    const d = await api.lay('/api/cong/dat-cho/dau-sach/' + encodeURIComponent(phien.ma));
    phien.datCho = d.datCho;
    phien.yeuThich = d.yeuThich;
    phien.khongDat = d.khongDat;
  } catch { phien.datCho = null; }
}

async function napSach(phien) {
  phien.sach = await api.lay('/api/cong/sach/' + encodeURIComponent(phien.ma));
}

async function datCho(phien) {
  if (!laBanDoc()) { veHanhDong(phien, 'dang-nhap', { tiepTheo: 'dat-cho' }); lenTrangHanhDong(phien); return; }
  const nut = phien.goc.querySelector('#btnDatCho');
  const ghim = phien.goc.querySelector('#btnDatChoGhim');
  if (nut) dangXuLy(nut, true);
  dangXuLy(ghim, true);
  try {
    const kq = await api.them('/api/cong/dat-cho', { maDauSach: phien.ma });
    phien.datCho = kq.datCho;
    try { await napSach(phien); } catch { phien.sach.soBanCoTheMuon = Math.max(0, (phien.sach.soBanCoTheMuon ?? 1) - 1); }
    veTinhTrang(phien);
    veHanhDong(phien, 've', { moi: true });
    thongBaoSr(phien, kq.thongBao);
    lenTrangHanhDong(phien);
    document.dispatchEvent(new CustomEvent('ulms:thong-bao-moi'));
  } catch (err) {
    if (!(err instanceof LoiApi)) throw err;
    if (err.ma === 'VUA_DUOC_DAT' || err.ma === 'HET_BAN_CO_THE_DAT') { try { await napSach(phien); veTinhTrang(phien); } catch { /* giữ số cũ */ } }
    veHanhDong(phien, 'loi', { loi: err });
    lenTrangHanhDong(phien);
  } finally {
    dangXuLy(ghim, false);
  }
}

async function huyDatCho(phien, nut) {
  if (!window.confirm(`Hủy đặt chỗ ${phien.datCho.maDatCho}? Bản sách sẽ được trả lại kệ cho bạn đọc khác.`)) return;
  dangXuLy(nut, true);
  try {
    await api.xoa('/api/cong/dat-cho/' + encodeURIComponent(phien.datCho.maDatCho));
    phien.datCho = null;
    try { await napSach(phien); } catch { /* giữ số cũ */ }
    veTinhTrang(phien);
    veHanhDong(phien, 'mac-dinh');
    thongBaoSr(phien, 'Đã hủy đặt chỗ.');
    phien.goc.querySelector('#btnDatCho')?.focus({ preventScroll: true });
  } catch (err) {
    dangXuLy(nut, false);
    window.alert(err.message);
  }
}

async function doiYeuThich(phien) {
  if (!laBanDoc()) { veHanhDong(phien, 'dang-nhap', { tiepTheo: 'yeu-thich' }); lenTrangHanhDong(phien); return; }
  const moi = !phien.yeuThich;
  try {
    const url = '/api/cong/yeu-thich/' + encodeURIComponent(phien.ma);
    const kq = moi ? await api.sua(url) : await api.xoa(url);
    phien.yeuThich = kq.yeuThich;
    const nut = phien.goc.querySelector('#btnYeuThich');
    if (nut) {
      nut.setAttribute('aria-pressed', phien.yeuThich ? 'true' : 'false');
      nut.setAttribute('aria-label', phien.yeuThich ? 'Bỏ yêu thích' : 'Yêu thích');
      nut.innerHTML = `<i class="bi ${phien.yeuThich ? 'bi-heart-fill' : 'bi-heart'}" aria-hidden="true"></i>`;
      if (phien.yeuThich && !giamChuyenDong()) gsap.fromTo(nut.firstChild, { scale: 0.4 }, { scale: 1, duration: motion.dur.md, ease: motion.ease.back });
    }
    thongBaoSr(phien, phien.yeuThich ? 'Đã thêm vào yêu thích.' : 'Đã bỏ khỏi yêu thích.');
  } catch (err) { window.alert(err.message); }
}

async function chiaSe(phien) {
  const url = location.origin + '/sach/' + encodeURIComponent(phien.ma);
  try {
    if (navigator.share && dienThoai()) { await navigator.share({ title: phien.sach.tenSach, url }); return; }
    await navigator.clipboard.writeText(url);
    thongBaoSr(phien, 'Đã sao chép liên kết sách.');
    const nut = phien.goc.querySelector('#btnChiaSe');
    nut?.classList.add('da-chep');
    setTimeout(() => nut?.classList.remove('da-chep'), 1600);
  } catch { window.prompt('Sao chép liên kết sách:', url); }
}

/** Điện thoại: khối hành động ở trang 2 → lật tới trang 2. */
function lenTrangHanhDong(phien) {
  if (phien.flip && !phien.kt.haiTrang && phien.flip.getCurrentPageIndex() !== 1) phien.flip.flip(1);
}

// ───────────────────────────── Mở / đóng ─────────────────────────────
/**
 * Mở Book Preview.
 * @param {string} maDauSach
 * @param {{nguon?: Element, duLieuNhanh?: object, lichSu?: boolean}} [tc] nguon = phần tử bìa để bay ra/bay về
 * @returns {Promise<void>} xong khi đóng
 */
export function moBookPreview(maDauSach, tc = {}) {
  if (hienTai) return hienTai.xong;
  const goc = taoKhung();
  const phien = {
    ma: String(maDauSach).toUpperCase(), goc, nguon: tc.nguon || null, sach: tc.duLieuNhanh ? { ...tc.duLieuNhanh } : null,
    focusTruoc: document.activeElement, flip: null, kt: kichThuoc(), trangThai: 'mac-dinh', datCho: null, yeuThich: false
  };
  let giai;
  phien.xong = new Promise((r) => { giai = r; });
  phien.giai = giai;
  hienTai = phien;

  if (tc.lichSu !== false && location.pathname !== '/sach/' + phien.ma) {
    history.pushState({ bp: phien.ma }, '', '/sach/' + encodeURIComponent(phien.ma));
    daDayLichSu = true;
  } else daDayLichSu = false;

  document.body.append(goc);
  document.documentElement.classList.add('bp-dang-mo');
  window.__lenis?.stop?.();
  ganSuKien(phien);
  batDau(phien);
  return phien.xong;
}

async function batDau(phien) {
  const { goc, kt } = phien;
  const sachEl = goc.querySelector('.bp__sach');
  sachEl.classList.toggle('bp__sach--hai-trang', kt.haiTrang);
  sachEl.style.setProperty('--pw', kt.pw + 'px');
  sachEl.style.setProperty('--ph', kt.ph + 'px');
  const biaNgoai = goc.querySelector('.bp__bia-ngoai');
  if (phien.sach) biaNgoai.append(taoBia(phien.sach, false));

  // Nạp dữ liệu song song với hiệu ứng bay
  const napDuLieu = Promise.all([napSach(phien), napCuaToi(phien)]);
  const hieuUng = moHieuUng(phien);
  try {
    await napDuLieu;
  } catch (err) {
    await hieuUng;
    hienLoiNap(phien, err);
    return;
  }
  if (!biaNgoai.firstChild) biaNgoai.append(taoBia(phien.sach, false));
  dungTrang(phien);
  await hieuUng;
  ketThucMo(phien);
}

function dungTrang(phien) {
  const flipEl = phien.goc.querySelector('.bp__flip');
  flipEl.replaceChildren(trangTrai(phien.sach), trangPhai(phien.sach));
  veTinhTrang(phien);
  veHanhDong(phien, phien.datCho ? 've' : 'mac-dinh', { moi: false });
  document.title = phien.sach.tenSach + ' – ' + (document.title.split(' – ').pop() || '');
}

function hienLoiNap(phien, err) {
  const flipEl = phien.goc.querySelector('.bp__flip');
  const t = h('div', 'bp-trang bp-trang--loi');
  const tt = h('div', 'bp-trang__noi');
  tt.append(h('h2', 'bp-ten', err?.maHttp === 404 ? 'Không tìm thấy sách' : 'Không mở được sách'), h('p', null, err?.message || 'Vui lòng thử lại.'));
  tt.querySelector('h2').id = 'bpTieuDe';
  t.append(tt);
  flipEl.replaceChildren(t);
  phien.goc.querySelector('.bp__ghim').hidden = true;
  phien.goc.querySelector('.bp__trang-goc').classList.add('hien');
  gsap.set(phien.goc.querySelector('.bp__bia-bay'), { autoAlpha: 0 });
  phien.goc.querySelector('#btnDongSach').focus({ preventScroll: true });
}

/** Chuỗi chuyển động mở sách (file 06 mục 1, motion.preview). */
function moHieuUng(phien) {
  const { goc, kt } = phien;
  const nen = goc.querySelector('.bp__nen');
  const bay = goc.querySelector('.bp__bia-bay');
  const mat = goc.querySelector('.bp__bia-mat');
  const trang = goc.querySelector('.bp__trang-goc');
  const to = goc.querySelectorAll('.bp__to-giay span');
  const dong = goc.querySelector('#btnDongSach');

  if (giamChuyenDong()) {
    gsap.set([bay, ...to], { autoAlpha: 0 });
    gsap.set(trang, { autoAlpha: 1 });
    trang.classList.add('hien');
    return gsap.fromTo([nen, goc.querySelector('.bp__sach'), dong], { opacity: 0 }, { opacity: 1, duration: motion.dur.sm }).then();
  }

  // Bìa đặt ở trang phải (máy tính) hoặc trang duy nhất (điện thoại); bay từ đúng vị trí nguồn
  const dich = bay.getBoundingClientRect();
  const r = phien.nguon?.isConnected ? phien.nguon.getBoundingClientRect() : null;
  const tu = r && r.width > 4 ? { x: r.left - dich.left, y: r.top - dich.top, scaleX: r.width / dich.width, scaleY: r.height / dich.height }
    : { x: 0, y: 40, scaleX: 0.6, scaleY: 0.6 };
  if (r) phien.nguon.classList.add('bp-nguon-an');

  const tl = gsap.timeline();
  tl.fromTo(nen, { opacity: 0 }, { opacity: 1, duration: p.flip, ease: motion.ease.out }, 0)
    .fromTo(dong, { opacity: 0 }, { opacity: 1, duration: motion.dur.sm }, p.flip * 0.6)
    .fromTo(bay, { ...tu, transformOrigin: '0 0' }, { x: 0, y: 0, scaleX: 1, scaleY: 1, duration: p.flip, ease: motion.ease.outStrong }, 0)
    .add(() => { trang.classList.add('hien'); gsap.set(trang, { autoAlpha: 1 }); gsap.set(to, { autoAlpha: 1 }); })
    // Bìa trước quay quanh gáy 0 → −180°
    .to(mat, { rotateY: -180, duration: p.open - p.flip - p.pageTurn * 2, ease: motion.ease.inOut })
    .to(bay, { autoAlpha: 0, duration: motion.dur.sm }, kt.haiTrang ? '>-0.05' : '<+0.25');
  // Lật nhanh 3 tờ giấy rồi dừng ở hai trang giấy
  to.forEach((t, i) => tl.to(t, { rotateY: -180, duration: p.pageTurn, ease: motion.ease.inOut }, `<${i === 0 ? '-0.1' : '+0.08'}`));
  tl.to(to, { autoAlpha: 0, duration: motion.dur.xs });
  return tl.then();
}

function ketThucMo(phien) {
  const { goc, kt } = phien;
  goc.classList.add('bp--da-mo');
  // StPageFlip: máy tính 2 trang (landscape), điện thoại 1 trang vuốt lật (portrait)
  const flipEl = goc.querySelector('.bp__flip');
  const trang = [...flipEl.querySelectorAll('.bp-trang')];
  if (trang.length >= 2) {
    try {
      phien.flip = new PageFlip(flipEl, {
        width: kt.pw, height: kt.ph, size: 'fixed', usePortrait: !kt.haiTrang, showCover: false, drawShadow: true,
        maxShadowOpacity: 0.35, flippingTime: Math.round(p.pageTurn * 2600), mobileScrollSupport: true,
        useMouseEvents: !kt.haiTrang, startZIndex: 2, autoSize: false
      });
      phien.flip.loadFromHTML(trang);
      if (!kt.haiTrang && phien.datCho) phien.flip.turnToPage(1);
    } catch (err) { console.warn('StPageFlip không khởi tạo được – hiển thị trang tĩnh', err); }
  }
  goc.querySelector('#btnDongSach').focus({ preventScroll: true });
}

function ganSuKien(phien) {
  const { goc } = phien;
  goc.addEventListener('click', (e) => {
    if (e.target.closest('#btnDongSach') || e.target.dataset.dong === '1') dongBookPreview();
  });
  goc.querySelector('#btnDatChoGhim').addEventListener('click', () => datCho(phien));
  goc.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') { e.preventDefault(); dongBookPreview(); return; }
    if (e.key !== 'Tab') return;
    // Giữ focus trong preview
    const ds = [...goc.querySelectorAll('button, a[href], input, [tabindex="0"]')].filter((x) => !x.disabled && x.offsetParent !== null);
    if (!ds.length) return;
    const dau = ds[0], cuoi = ds[ds.length - 1];
    if (e.shiftKey && document.activeElement === dau) { e.preventDefault(); cuoi.focus(); }
    else if (!e.shiftKey && document.activeElement === cuoi) { e.preventDefault(); dau.focus(); }
  });
}

/** Đóng: nếu đã pushState → history.back() (popstate sẽ đóng thật) để nút Back và nút × cùng một đường. */
export function dongBookPreview() {
  if (!hienTai || hienTai.dangDong) return;
  if (daDayLichSu) { daDayLichSu = false; history.back(); return; }
  if (location.pathname.startsWith('/sach/')) history.replaceState(null, '', '/');
  dongThat();
}

async function dongThat() {
  const phien = hienTai;
  if (!phien || phien.dangDong) return;
  phien.dangDong = true;
  dungDemNguoc(phien);
  const { goc } = phien;
  const nen = goc.querySelector('.bp__nen');
  const bay = goc.querySelector('.bp__bia-bay');
  const mat = goc.querySelector('.bp__bia-mat');
  const trang = goc.querySelector('.bp__trang-goc');
  const r = phien.nguon?.isConnected ? phien.nguon.getBoundingClientRect() : null;
  const trongMan = r && r.width > 4 && r.bottom > 0 && r.top < window.innerHeight;

  if (giamChuyenDong() || !trongMan) {
    await gsap.to(goc, { opacity: 0, duration: giamChuyenDong() ? motion.dur.sm : p.close * 0.5 }).then();
  } else {
    // Gập sách lại rồi bay về đúng vị trí nguồn (đo lại vị trí lúc đóng)
    const dich = bay.getBoundingClientRect();
    const dx = r.left - dich.left - (gsap.getProperty(bay, 'x') || 0);
    const dy = r.top - dich.top - (gsap.getProperty(bay, 'y') || 0);
    const tl = gsap.timeline();
    tl.to([trang, goc.querySelector('.bp__ghim'), goc.querySelector('#btnDongSach')], { autoAlpha: 0, duration: p.close * 0.25 }, 0)
      .set(bay, { autoAlpha: 1 }, 0)
      .fromTo(mat, { rotateY: -180 }, { rotateY: 0, duration: p.close * 0.35, ease: motion.ease.inOut }, 0)
      .to(bay, { x: dx, y: dy, scaleX: r.width / dich.width, scaleY: r.height / dich.height, transformOrigin: '0 0',
        duration: p.close * 0.5, ease: motion.ease.inOut }, '>-0.05')
      .to(nen, { opacity: 0, duration: p.close * 0.5 }, '<');
    await tl.then();
  }
  try { phien.flip?.destroy?.(); } catch { /* đã gỡ */ }
  phien.nguon?.classList.remove('bp-nguon-an');
  goc.remove();
  document.documentElement.classList.remove('bp-dang-mo');
  window.__lenis?.start?.();
  if (document.title.includes(' – ')) document.title = document.title.split(' – ').pop();
  hienTai = null;
  const ve = phien.focusTruoc;
  if (ve?.isConnected) ve.focus({ preventScroll: true });
  phien.giai();
  // Đăng nhập ngay trong preview: nạp lại trang để header hiện tài khoản (vẫn ở đúng vị trí cuộn)
  if (dangNhapTrongPreview) { dangNhapTrongPreview = false; location.reload(); }
}

// Nút Back của trình duyệt
window.addEventListener('popstate', (e) => {
  if (hienTai) { daDayLichSu = false; dongThat(); }
  else if (e.state?.bp) moBookPreview(e.state.bp, { lichSu: false });
});

// Mọi điểm vào khác: phần tử có data-mo-sach (thẻ sách, kết quả, yêu thích, thông báo…)
document.addEventListener('click', (e) => {
  const el = e.target.closest('[data-mo-sach]');
  if (!el || e.defaultPrevented || e.ctrlKey || e.metaKey || e.shiftKey || e.button !== 0) return;
  e.preventDefault();
  const nguon = el.querySelector('img, .bp-bia, .book__front') || el;
  moBookPreview(el.dataset.moSach, { nguon });
});

// Liên kết sâu /sach/{maDauSach}: trang chủ render kèm data-mo-sach trên <body>
const sau = document.body.dataset.moSachKhiTai;
if (sau) {
  history.replaceState({ bp: sau }, '', location.pathname);
  // Đợi kệ sách dựng xong để bay ra từ cuốn tương ứng nếu có
  requestAnimationFrame(() => setTimeout(() => {
    const nguon = document.querySelector(`[data-ma-dau-sach="${CSS.escape(sau)}"] .book__front`);
    moBookPreview(sau, { nguon, lichSu: false });
  }, 350));
}
