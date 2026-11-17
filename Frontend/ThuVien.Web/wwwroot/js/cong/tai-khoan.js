// Trang cá nhân bạn đọc (file 07 mục 3) – kính mờ trên nền pastel. Chỉ ĐỌC dữ liệu gốc; bạn đọc không tự gia hạn/trả/thanh toán.
// Thẻ vào trang lần lượt (stagger motion.glass.cardStagger), hover nâng 5px (CSS), nhãn loại màu pastel, thanh tiến độ mảnh.
import { motion, giamChuyenDong } from '../motion.config.js';
import { api } from '../api.js';
import { veMaVach } from './ma-vach.js';
import { dongThongBao } from './thong-bao-dong.js';

const goc = document.getElementById('tkNoiDung');
const than = document.getElementById('tkThan');
const trang = goc.dataset.trang;
const NHAC_GIO = 2;

// ───────────── Tiện ích ─────────────
const h = (tag, lop, chu) => { const e = document.createElement(tag); if (lop) e.className = lop; if (chu !== undefined && chu !== null) e.textContent = chu; return e; };
const p2 = (n) => String(n).padStart(2, '0');
const ngay = (s) => { if (!s) return '—'; const [y, m, d] = String(s).slice(0, 10).split('-'); return `${d}/${m}/${y}`; };
const ngayGio = (s) => { const d = new Date(s); return `${p2(d.getHours())}:${p2(d.getMinutes())} ${p2(d.getDate())}/${p2(d.getMonth() + 1)}/${d.getFullYear()}`; };
const tien = (v) => Number(v).toLocaleString('vi-VN') + ' đ';

function the(lop, nhan, mauNhan) {
  const c = h('article', 'glass-card tk-the ' + (lop || ''));
  if (nhan) c.append(h('span', 'tk-nhan tk-nhan--' + (mauNhan || 'xanh'), nhan));
  return c;
}
function tienDo(phanTram, canhBao, nhan) {
  const t = h('div', 'tk-tien-do' + (canhBao ? ' tk-tien-do--canh-bao' : ''));
  t.setAttribute('role', 'progressbar');
  t.setAttribute('aria-label', nhan || 'Tiến độ');
  t.setAttribute('aria-valuemin', '0'); t.setAttribute('aria-valuemax', '100');
  const v = Math.max(0, Math.min(100, Math.round(phanTram)));
  t.setAttribute('aria-valuenow', String(v));
  const b = h('span'); b.style.setProperty('--gia-tri', v + '%'); // CSSOM – CSP cho phép
  t.append(b);
  return t;
}
function rong(chu, lienKet) {
  const c = the('tk-the--rong');
  c.append(h('i', 'bi bi-inbox tk-rong__icon'), h('p', null, chu));
  c.firstChild.setAttribute('aria-hidden', 'true');
  if (lienKet) { const a = h('a', 'cong-nut cong-nut--vien', lienKet[0]); a.href = lienKet[1]; c.append(a); }
  return c;
}
function hienVao() {
  goc.setAttribute('aria-busy', 'false');
  if (giamChuyenDong() || !window.gsap) return;
  than.classList.add('dang-vao');
  gsap.from(than.querySelectorAll('.tk-the'), { y: 18, opacity: 0, duration: motion.dur.md, ease: motion.ease.out, stagger: motion.glass.cardStagger,
    clearProps: 'transform,opacity', onComplete: () => than.classList.remove('dang-vao') });
}
function biaNho(src, ten, ma) {
  const b = h(ma ? 'button' : 'div', 'tk-bia');
  if (ma) { b.type = 'button'; b.dataset.moSach = ma; b.setAttribute('aria-label', 'Mở sách ' + ten); }
  if (src) { const i = h('img'); i.src = src; i.alt = ''; i.loading = 'lazy'; b.append(i); }
  else b.append(h('i', 'bi bi-book'));
  return b;
}

// ───────────── Tổng quan ─────────────
async function tongQuan() {
  const d = await api.lay('/api/cong/tai-khoan/tong-quan');
  const t = d.the;
  const loiChao = the('tk-the--rong-2 tk-chao');
  loiChao.append(h('p', 'tk-chao__xin', 'Xin chào,'), h('p', 'tk-chao__ten', t.hoTen));
  const theBd = h('div', 'tk-the-bd');
  const dl = h('dl');
  for (const [k, v] of [['Mã SV/HV/CB', t.maSvHvCb], ['Đối tượng', t.doiTuong], ['Lớp/đơn vị', t.lopDonVi], ['Số thẻ', t.soTheBd],
    ['Hạn thẻ', `${ngay(t.hsdTuNgay)} – ${ngay(t.hsdDenNgay)}`], ['Được mượn tối đa', `${t.soBanToiDa} bản`]])
    dl.append(h('dt', null, k), h('dd', null, v));
  theBd.append(dl);
  loiChao.append(theBd);
  const tongNgay = (new Date(t.hsdDenNgay) - new Date(t.hsdTuNgay)) / 864e5;
  loiChao.append(h('p', 'tk-phu', t.soNgayConHanThe >= 0 ? `Thẻ còn hạn ${t.soNgayConHanThe} ngày` : 'Thẻ đã hết hạn'),
    tienDo(tongNgay > 0 ? 100 - (t.soNgayConHanThe / tongNgay) * 100 : 100, t.soNgayConHanThe <= 30, 'Thời gian hiệu lực thẻ đã dùng'));
  than.append(loiChao);

  const so = [['Đang mượn', d.dangMuon, 'bi-book', '/tai-khoan/dang-muon', 'xanh'], ['Sắp đến hạn (≤ 3 ngày)', d.sapDenHan, 'bi-calendar-event', '/tai-khoan/dang-muon', 'vang'],
    ['Quá hạn', d.quaHan, 'bi-alarm', '/tai-khoan/dang-muon', 'do'], ['Đặt chỗ đang giữ', d.datChoDangGiu, 'bi-bookmark-check', '/tai-khoan/dat-cho', 'tim']];
  for (const [nhan, gt, icon, href, mau] of so) {
    const c = the('tk-the--so tk-mau--' + mau);
    const a = h('a', 'tk-so'); a.href = href;
    const i = h('i', 'bi ' + icon); i.setAttribute('aria-hidden', 'true');
    a.append(i, h('span', 'tk-so__gia-tri', String(gt)), h('span', 'tk-so__nhan', nhan));
    c.append(a);
    than.append(c);
  }

  const viec = the('tk-the--rong-2', 'Việc cần làm', 'vang');
  if (d.viecCanLam.length) { const ul = h('ul', 'tk-viec'); d.viecCanLam.forEach((v) => ul.append(h('li', null, v))); viec.append(ul); }
  else viec.append(h('p', 'tk-phu', 'Không có việc nào cần làm. Chúc bạn đọc sách vui!'));
  than.append(viec);

  // Biểu đồ nhỏ “Số sách đã mượn theo tháng (12 tháng)” – cột bo góc (file 09 mục 3), có bảng thay thế cho trình đọc màn hình
  const bd = the('tk-the--rong-2', 'Số sách đã mượn theo tháng', 'xanh');
  const max = Math.max(1, ...d.muonTheoThang.map((x) => x.soBan));
  const cot = h('div', 'tk-cot'); cot.setAttribute('aria-hidden', 'true');
  const bang = h('table', 'visually-hidden');
  bang.innerHTML = '<caption>Số sách đã mượn theo tháng, 12 tháng gần nhất</caption><thead><tr><th>Tháng</th><th>Số bản</th></tr></thead><tbody></tbody>';
  for (const x of d.muonTheoThang) {
    const c = h('div', 'tk-cot__muc');
    const b = h('span', 'tk-cot__thanh'); b.style.setProperty('--cao', (x.soBan / max) * 100 + '%');
    if (x.soBan) b.dataset.so = x.soBan;
    c.append(b, h('span', 'tk-cot__nhan', `T${x.thang}`));
    cot.append(c);
    const tr = h('tr'); tr.append(h('td', null, `${p2(x.thang)}/${x.nam}`), h('td', null, String(x.soBan)));
    bang.tBodies[0].append(tr);
  }
  bd.append(cot, bang);
  than.append(bd);
}

// ───────────── Sách đang mượn ─────────────
async function dangMuon() {
  const ds = await api.lay('/api/cong/tai-khoan/dang-muon');
  if (!ds.length) { than.append(rong('Bạn không có sách nào đang mượn.', ['Xem kệ sách', '/#ke-sach'])); return; }
  for (const s of ds) {
    const qua = s.soNgayConLai < 0;
    const c = the('tk-the--sach', qua ? 'Quá hạn' : s.soNgayConLai <= 3 ? 'Sắp đến hạn' : 'Đang mượn', qua ? 'do' : s.soNgayConLai <= 3 ? 'vang' : 'xanh');
    const noi = h('div', 'tk-sach');
    noi.append(biaNho(s.anhBia, s.tenSach, s.maDauSach));
    const tt = h('div', 'tk-sach__tt');
    tt.append(h('h2', 'tk-sach__ten', s.tenSach), h('p', 'tk-phu', `Bản ${s.maBanSach} · Phiếu mượn ${s.soPMuon}`));
    const dl = h('dl', 'tk-dl');
    dl.append(h('dt', null, 'Ngày mượn'), h('dd', null, ngay(s.ngayMuon)), h('dt', null, 'Hạn trả'),
      h('dd', null, ngay(s.hanTra) + (s.soLanGiaHan ? ` (đã gia hạn ${s.soLanGiaHan} lần)` : '')));
    tt.append(dl);
    tt.append(h('p', 'tk-con-lai ' + (qua ? 'do' : s.soNgayConLai <= 3 ? 'vang' : 'xanh'),
      qua ? `Quá hạn ${-s.soNgayConLai} ngày – vui lòng trả sách tại quầy` : s.soNgayConLai === 0 ? 'Đến hạn trả hôm nay' : `Còn ${s.soNgayConLai} ngày`));
    const tong = (new Date(s.hanTra) - new Date(s.ngayMuon)) / 864e5;
    const da = (Date.now() - new Date(s.ngayMuon)) / 864e5;
    tt.append(tienDo(tong > 0 ? (da / tong) * 100 : 100, qua || s.soNgayConLai <= 3, 'Thời gian mượn đã dùng'));
    noi.append(tt);
    c.append(noi);
    than.append(c);
  }
  than.append(h('p', 'tk-ghi-chu', 'Gia hạn, trả sách và thanh toán thực hiện tại quầy mượn trả.'));
}

// ───────────── Đặt chỗ (file 06 mục 6) ─────────────
const demNguoc = [];
function batDem(el, hetHan) {
  let cu = null;
  const cap = () => {
    const con = new Date(hetHan) - Date.now();
    if (con <= 0) { el.textContent = 'Đã hết hạn'; el.classList.add('canh-bao'); return; }
    const phut = Math.floor(con / 60000);
    if (phut === cu) return;
    cu = phut;
    el.textContent = `${Math.floor(phut / 60)} giờ ${p2(phut % 60)} phút`;
    el.classList.toggle('canh-bao', con < NHAC_GIO * 3600000);
    if (!giamChuyenDong() && window.gsap) gsap.fromTo(el, { rotateX: -80, opacity: 0.3 }, { rotateX: 0, opacity: 1, duration: motion.dur.sm });
  };
  cap();
  demNguoc.push(setInterval(cap, 1000));
}
const MAU_TT = { 'Đang giữ': 'xanh', 'Đã nhận': 'tim', 'Hết hạn': 'vang', 'Đã hủy': 'xam' };

async function datCho() {
  demNguoc.forEach(clearInterval); demNguoc.length = 0;
  const ds = await api.lay('/api/cong/dat-cho/cua-toi');
  than.replaceChildren();
  if (!ds.length) { than.append(rong('Bạn chưa đặt chỗ sách nào. Mở một cuốn sách trên kệ và bấm “Đặt chỗ”.', ['Xem kệ sách', '/#ke-sach'])); return; }
  const chon = new URLSearchParams(location.search).get('ma');
  for (const d of ds) {
    const giu = d.trangThai === 'Đang giữ';
    const c = the('tk-the--dat-cho' + (giu ? ' tk-the--rong-2' : ''), d.trangThai, MAU_TT[d.trangThai]);
    c.id = 'datCho-' + d.maDatCho;
    const noi = h('div', 'tk-sach');
    noi.append(biaNho(d.anhBia ? d.anhBia.replace(/\.webp$/, '_nho.webp') : null, d.tenSach, d.maDauSach));
    const tt = h('div', 'tk-sach__tt');
    tt.append(h('h2', 'tk-sach__ten', d.tenSach), h('p', 'tk-phu', `Mã đặt chỗ ${d.maDatCho} · đặt lúc ${ngayGio(d.tgDat)}`));
    const dl = h('dl', 'tk-dl');
    dl.append(h('dt', null, 'Bản sách'), h('dd', null, `${d.maBanSach} · ${d.viTri}`));
    if (giu) {
      dl.append(h('dt', null, 'Hạn giữ'), h('dd', null, ngayGio(d.tgHetHan)), h('dt', null, 'Nhận tại'), h('dd', null, d.quayNhan));
      const con = h('dd', 'tk-dem'); dl.append(h('dt', null, 'Còn lại'), con);
      batDem(con, d.tgHetHan);
    } else if (d.tgXuLy) {
      dl.append(h('dt', null, d.trangThai === 'Đã nhận' ? 'Nhận lúc' : d.trangThai === 'Đã hủy' ? 'Hủy lúc' : 'Hết hạn lúc'), h('dd', null, ngayGio(d.tgXuLy)));
      if (d.soPMuon) dl.append(h('dt', null, 'Phiếu mượn'), h('dd', null, d.soPMuon));
      if (d.lyDoHuy) dl.append(h('dt', null, 'Lý do'), h('dd', null, d.lyDoHuy));
    }
    tt.append(dl);
    if (giu) {
      const tong = new Date(d.tgHetHan) - new Date(d.tgDat);
      tt.append(tienDo(((Date.now() - new Date(d.tgDat)) / tong) * 100, new Date(d.tgHetHan) - Date.now() < NHAC_GIO * 3600000, 'Thời gian giữ chỗ đã dùng'));
      const vach = h('div', 'tk-ma-vach'); vach.append(veMaVach(d.maDatCho, { nhan: 'Mã vạch đặt chỗ ' + d.maDatCho }));
      tt.append(vach);
      const huy = h('button', 'cong-nut cong-nut--vien tk-nut-huy');
      huy.type = 'button';
      huy.innerHTML = '<i class="bi bi-x-circle" aria-hidden="true"></i> Hủy đặt chỗ';
      huy.addEventListener('click', async () => {
        if (!confirm(`Hủy đặt chỗ ${d.maDatCho}?`)) return;
        huy.disabled = true;
        try { await api.xoa('/api/cong/dat-cho/' + encodeURIComponent(d.maDatCho)); await datCho(); hienVao(); }
        catch (e) { alert(e.message); huy.disabled = false; }
      });
      tt.append(huy);
    }
    noi.append(tt);
    c.append(noi);
    than.append(c);
    if (chon === d.maDatCho) requestAnimationFrame(() => { c.classList.add('tk-the--noi-bat'); c.scrollIntoView({ block: 'center' }); });
  }
}

// ───────────── Lịch sử ─────────────
async function lichSu(nam) {
  const d = await api.lay('/api/cong/tai-khoan/lich-su' + (nam ? '?nam=' + nam : ''));
  than.replaceChildren();
  const loc = the('tk-the--rong-2 tk-loc');
  const lb = h('label', null, 'Năm'); lb.htmlFor = 'cboNamLichSu';
  const sel = h('select', 'cong-o tk-chon'); sel.id = 'cboNamLichSu';
  for (const n of d.cacNam) { const o = h('option', null, String(n)); o.value = n; o.selected = n === d.nam; sel.append(o); }
  sel.addEventListener('change', async () => { await lichSu(sel.value); hienVao(); document.getElementById('cboNamLichSu')?.focus(); });
  loc.append(lb, sel);
  than.append(loc);
  if (!d.dong.length) { than.append(rong(`Không có chứng từ nào trong năm ${d.nam}.`)); return; }
  for (const x of d.dong) {
    const mau = x.loai === 'Phiếu mượn sách' ? 'xanh' : x.loai === 'Phiếu gia hạn sách' ? 'vang' : 'tim';
    const c = the('tk-the--rong-2 tk-the--dong', x.loai, mau);
    c.append(h('h2', 'tk-sach__ten', `${x.soChungTu} · ${ngay(x.ngay)}`), h('p', 'tk-phu', x.noiDung));
    const ul = h('ul', 'tk-ds-sach'); x.sach.forEach((s) => ul.append(h('li', null, s)));
    c.append(ul);
    than.append(c);
  }
}

// ───────────── Vi phạm ─────────────
async function viPham() {
  const ds = await api.lay('/api/cong/tai-khoan/vi-pham');
  if (!ds.length) { than.append(rong('Bạn không có biên bản vi phạm nào.')); return; }
  const no = ds.filter((b) => b.htThanhToan === 'Chưa thanh toán').reduce((s, b) => s + b.tongTien, 0);
  const tong = the('tk-the--rong-2 ' + (no > 0 ? 'tk-mau--do' : 'tk-mau--xanh'));
  tong.append(h('p', 'tk-so__gia-tri', tien(no)), h('p', 'tk-so__nhan', no > 0 ? 'Khoản phải nộp chưa thanh toán – vui lòng thanh toán tại quầy' : 'Không còn khoản phải nộp'));
  than.append(tong);
  for (const b of ds) {
    const chua = b.htThanhToan === 'Chưa thanh toán';
    const c = the('tk-the--rong-2', chua ? 'Chưa thanh toán' : 'Đã thanh toán – ' + b.htThanhToan, chua ? 'do' : 'xanh');
    c.append(h('h2', 'tk-sach__ten', `Biên bản ${b.soBienBan} · ${ngayGio(b.thoiGianLap)}`), h('p', 'tk-phu', `Phiếu mượn ${b.soPMuon} · Tổng ${tien(b.tongTien)}`));
    const bang = h('table', 'tk-bang');
    bang.innerHTML = '<thead><tr><th scope="col">Sách</th><th scope="col">Nội dung</th><th scope="col" class="so">SL</th><th scope="col" class="so">Thành tiền</th></tr></thead><tbody></tbody>';
    for (const d of b.chiTiet) {
      const tr = h('tr');
      tr.append(h('td', null, `${d.tenSach} (${d.maBanSach})`), h('td', null, d.noiDung), h('td', 'so', String(d.soLuong)), h('td', 'so', tien(d.thanhTien)));
      bang.tBodies[0].append(tr);
    }
    const cuon = h('div', 'tk-cuon-ngang'); cuon.append(bang);
    c.append(cuon);
    than.append(c);
  }
}

// ───────────── Yêu thích ─────────────
async function yeuThich() {
  const ds = await api.lay('/api/cong/yeu-thich');
  than.replaceChildren();
  if (!ds.length) { than.append(rong('Chưa có sách yêu thích. Bấm biểu tượng trái tim trong trang sách để lưu.', ['Xem kệ sách', '/#ke-sach'])); return; }
  for (const s of ds) {
    const c = the('tk-the--yeu-thich');
    c.append(biaNho(s.anhBia, s.tenSach, s.maDauSach));
    c.append(h('h2', 'tk-sach__ten', s.tenSach), h('p', 'tk-phu', s.tacGia || ''),
      h('p', 'tk-con-lai ' + (s.soBanCoTheMuon > 0 ? 'xanh' : 'vang'), s.soBanCoTheMuon > 0 ? `Còn ${s.soBanCoTheMuon} bản có thể mượn` : 'Đang được mượn hết'));
    const bo = h('button', 'tk-bo-yeu-thich');
    bo.type = 'button';
    bo.setAttribute('aria-label', 'Bỏ yêu thích ' + s.tenSach);
    bo.innerHTML = '<i class="bi bi-heart-fill" aria-hidden="true"></i>';
    bo.addEventListener('click', async () => {
      bo.disabled = true;
      try { await api.xoa('/api/cong/yeu-thich/' + encodeURIComponent(s.maDauSach)); await yeuThich(); hienVao(); }
      catch (e) { alert(e.message); bo.disabled = false; }
    });
    c.append(bo);
    than.append(c);
  }
}

// ───────────── Thông báo ─────────────
async function thongBao() {
  const d = await api.lay('/api/cong/thong-bao?soLuong=100');
  than.replaceChildren();
  const dau = the('tk-the--rong-2 tk-loc');
  dau.append(h('p', 'tk-phu', d.chuaDoc ? `${d.chuaDoc} thông báo chưa đọc` : 'Đã đọc tất cả thông báo'));
  if (d.chuaDoc) {
    const nut = h('button', 'cong-nut cong-nut--vien', 'Đánh dấu tất cả đã đọc'); nut.type = 'button'; nut.id = 'btnDocTatCa';
    nut.addEventListener('click', async () => { await api.them('/api/cong/thong-bao/da-doc-tat-ca', {}); await thongBao(); hienVao(); });
    dau.append(nut);
  }
  than.append(dau);
  if (!d.ds.length) { than.append(rong('Chưa có thông báo nào.')); return; }
  const c = the('tk-the--rong-2 tk-the--thong-bao');
  const ul = h('ul', 'tk-thong-bao');
  d.ds.forEach((t) => ul.append(dongThongBao(t, async (x) => { if (!x.daDoc) await api.them(`/api/cong/thong-bao/${x.maThongBao}/da-doc`, {}).catch(() => {}); })));
  c.append(ul);
  than.append(c);
}

// ───────────── Đổi mật khẩu ─────────────
function doiMatKhau() {
  const c = the('tk-the--rong-2 tk-the--form');
  const f = h('form', 'tk-form'); f.id = 'frmDoiMatKhau'; f.noValidate = true;
  f.innerHTML = `
    <div class="cong-loi" role="alert" hidden></div>
    <div class="cong-thanh-cong" role="status" hidden></div>
    <div class="cong-truong"><label for="matKhauCu">Mật khẩu hiện tại</label><input class="cong-o" id="matKhauCu" type="password" autocomplete="current-password" required /></div>
    <div class="cong-truong"><label for="matKhauMoi">Mật khẩu mới</label><input class="cong-o" id="matKhauMoi" type="password" autocomplete="new-password" required aria-describedby="goiYMk" />
      <small class="cong-goi-y" id="goiYMk">Ít nhất 8 ký tự, gồm cả chữ và số.</small></div>
    <div class="cong-truong"><label for="nhapLaiMk">Nhập lại mật khẩu mới</label><input class="cong-o" id="nhapLaiMk" type="password" autocomplete="new-password" required /></div>
    <button type="submit" class="cong-nut cong-nut--chinh" id="btnDoiMatKhau"><i class="bi bi-key" aria-hidden="true"></i> Đổi mật khẩu</button>`;
  const loi = f.querySelector('.cong-loi'), ok = f.querySelector('.cong-thanh-cong');
  f.addEventListener('submit', async (e) => {
    e.preventDefault();
    loi.hidden = ok.hidden = true;
    const nut = f.querySelector('#btnDoiMatKhau'); nut.disabled = true;
    try {
      const kq = await api.them('/api/cong/tai-khoan/doi-mat-khau',
        { matKhauCu: f.querySelector('#matKhauCu').value, matKhauMoi: f.querySelector('#matKhauMoi').value, nhapLai: f.querySelector('#nhapLaiMk').value });
      ok.textContent = kq.thongBao; ok.hidden = false; f.reset();
    } catch (err) {
      loi.textContent = Object.values(err.truong || {}).filter((v) => v && v !== err.truong?.huongXuLy).join(' ') || err.message;
      loi.hidden = false;
    } finally { nut.disabled = false; }
  });
  c.append(f);
  than.append(c);
}

// ───────────── Khởi động ─────────────
const TRANG = { '': tongQuan, 'dang-muon': dangMuon, 'dat-cho': datCho, 'lich-su': () => lichSu(), 'vi-pham': viPham, 'yeu-thich': yeuThich, 'thong-bao': thongBao, 'doi-mat-khau': doiMatKhau };
(async () => {
  than.replaceChildren();
  try { await TRANG[trang]?.(); }
  catch (e) {
    than.replaceChildren();
    const c = the('tk-the--rong-2'); c.append(h('p', 'cong-loi', e.message)); than.append(c);
  }
  hienVao();
})();
