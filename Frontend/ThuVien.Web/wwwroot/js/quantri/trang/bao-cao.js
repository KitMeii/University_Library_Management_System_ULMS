// 01.04.02 Tổng hợp tình hình mượn trả sách / 02.05.01 Tổng hợp tình hình vi phạm (SPEC 6.4.5–6.4.6, file 09 mục 3):
// kỳ báo cáo → btnTongHop → thẻ số liệu + biểu đồ + bảng chi tiết → btnLapBaoCao mở 01.04.03 / 02.05.02 với cùng kỳ.
import { api, chuoiTruyVan } from '../../api.js';
import { motion, giamChuyenDong } from '../../motion.config.js';
import { BieuDo, dauMau, so, tien, tienGon, ngayNgan, ngayDayDu, tooltipTruc } from '../../charts/chart-factory.js';
import { BANG_MAU_DO } from '../../charts/chart-theme.js';
import { LuoiDuLieu } from '../luoi.js';
import { nhanTrangThai } from '../danh-muc.js';
import { ganKy, docKy, baoLoiKy } from '../ky-bao-cao.js';

const $ = (id) => document.getElementById(id);
const muonTra = document.querySelector('[data-loai]').dataset.loai === 'muon-tra';
const bieuDo = {};
document.querySelectorAll('.qt-bieu-do').forEach((h) => { bieuDo[h.id] = new BieuDo(h); });
const kyDaTongHop = { gt: null };
let kyDangXem = null;

/**
 * Bảng chi tiết khi xem trước (GIA_DINH D82): API trả trang đầu (≤ 50 dòng) + tổng số dòng.
 * ≤ 50 dòng → lưới tĩnh đủ dòng như trước (giao diện không đổi – ảnh Bài 4 mục 4.5.1–4.5.2);
 * > 50 dòng → lưới phân trang phía server qua /chi-tiet/{bang}, mỗi trang 50 dòng. Số liệu tổng hợp, biểu đồ luôn tính toàn kỳ.
 */
let coTrangChiTiet = 50; // theo coTrang server trả về (mặc định 50)
async function ganBangChiTiet(l, trangDau, tongSo, bang) {
  if (tongSo <= coTrangChiTiet) {
    l.tc.url = undefined;
    l.tc.duLieuTinh = trangDau.map((x, i) => ({ ...x, khoa: i }));
    await l.tai();
    return;
  }
  const ky = kyDangXem;
  l.tc.duLieuTinh = null;
  l.tc.url = `/api/quan-tri/bao-cao/${muonTra ? 'muon-tra' : 'vi-pham'}/chi-tiet/${bang}`;
  l.tc.coTrang = coTrangChiTiet;
  l.tc.thamSoThem = () => ({ tuNgay: ky.tuNgay, denNgay: ky.denNgay });
  l.trang = 1;
  l.ve(trangDau, tongSo); // trang 1 đã có trong kết quả tổng hợp – không gọi lại
}

function datSo(id, giaTri, dinhDang = so) {
  const el = $(id);
  const thanh = el.closest('.qt-kpi').querySelector('.qt-kpi__thanh span');
  if (window.gsap && !giamChuyenDong()) {
    const o = { v: 0 };
    gsap.to(o, { v: giaTri, duration: motion.dashboard.countUp, ease: motion.ease.out, onUpdate: () => { el.textContent = dinhDang(Math.round(o.v)); } });
    gsap.fromTo(thanh, { scaleX: 0 }, { scaleX: 1, duration: motion.dashboard.countUp, ease: motion.ease.out });
  } else { el.textContent = dinhDang(giaTri); thanh.style.transform = 'scaleX(1)'; }
}

// Thẻ (tab) chi tiết: bàn phím ←/→ theo WAI-ARIA
function ganThe() {
  const the = [...document.querySelectorAll('[role="tab"]')];
  const chon = (t) => {
    the.forEach((x) => {
      const dang = x === t;
      x.setAttribute('aria-selected', String(dang));
      x.tabIndex = dang ? 0 : -1;
      $(x.getAttribute('aria-controls')).hidden = !dang;
    });
  };
  the.forEach((t, i) => {
    t.addEventListener('click', () => chon(t));
    t.addEventListener('keydown', (e) => {
      const j = e.key === 'ArrowRight' ? (i + 1) % the.length : e.key === 'ArrowLeft' ? (i - 1 + the.length) % the.length : -1;
      if (j >= 0) { e.preventDefault(); chon(the[j]); the[j].focus(); }
    });
  });
}

const luoi = (id, khoa, cot, rong) => new LuoiDuLieu($(id), { khoa, duLieuTinh: [], cot, rong });
const lopTra = { 'Bình thường': 'qt-nhan-tt--ok', 'Hư hỏng': 'qt-nhan-tt--canh-bao', 'Mất': 'qt-nhan-tt--loi' };

// ---------------- MB06 ----------------
const LUOI_MT = muonTra ? {
  luotMuon: luoi('dgvLuotMuon', 'khoa', [
    { ten: 'ngayMuon', nhan: 'Ngày mượn', dinhDang: ngayDayDu },
    { ten: 'soPMuon', nhan: 'Số phiếu mượn' },
    { ten: 'hoTen', nhan: 'Bạn đọc', dinhDang: (v, d) => `${d.maBanDoc} – ${v}` },
    { ten: 'maBanSach', nhan: 'Mã bản sách' },
    { ten: 'tenSach', nhan: 'Tên sách' },
    { ten: 'hanTra', nhan: 'Hạn trả', dinhDang: ngayDayDu }
  ], 'Không có lượt mượn trong kỳ.'),
  luotGiaHan: luoi('dgvLuotGiaHan', 'khoa', [
    { ten: 'ngayLapPGiaHan', nhan: 'Ngày lập', dinhDang: ngayDayDu },
    { ten: 'soPGiaHan', nhan: 'Số phiếu gia hạn' },
    { ten: 'hoTen', nhan: 'Bạn đọc' },
    { ten: 'maBanSach', nhan: 'Bản sách', dinhDang: (v, d) => `${v} – ${d.tenSach}` },
    { ten: 'ketQuaGiaHan', nhan: 'Kết quả', dinhDang: (v) => nhanTrangThai(v, v === 'Đồng ý' ? 'qt-nhan-tt--ok' : 'qt-nhan-tt--loi') },
    { ten: 'hanTraMoi', nhan: 'Hạn trả mới', dinhDang: (v) => (v ? ngayDayDu(v) : '—') }
  ], 'Không có lượt gia hạn trong kỳ.'),
  luotTra: luoi('dgvLuotTra', 'khoa', [
    { ten: 'ngayTra', nhan: 'Ngày trả', dinhDang: ngayDayDu },
    { ten: 'soPTra', nhan: 'Số phiếu trả' },
    { ten: 'hoTen', nhan: 'Bạn đọc' },
    { ten: 'maBanSach', nhan: 'Bản sách', dinhDang: (v, d) => `${v} – ${d.tenSach}` },
    { ten: 'tinhTrangTra', nhan: 'Tình trạng khi trả', dinhDang: (v) => nhanTrangThai(v, lopTra[v]) },
    { ten: 'soNgayQuaHan', nhan: 'Quá hạn (ngày)', canPhai: true, dinhDang: (v) => (v ? so(v) : '—') }
  ], 'Không có lượt trả trong kỳ.'),
  sachQuaHan: luoi('dgvSachQuaHan', 'khoa', [
    { ten: 'soPMuon', nhan: 'Số phiếu mượn' },
    { ten: 'hoTen', nhan: 'Bạn đọc', dinhDang: (v, d) => `${d.maBanDoc} – ${v}` },
    { ten: 'maBanSach', nhan: 'Bản sách', dinhDang: (v, d) => `${v} – ${d.tenSach}` },
    { ten: 'hanTra', nhan: 'Hạn trả hiện hành', dinhDang: ngayDayDu },
    { ten: 'soNgayQuaHan', nhan: 'Quá hạn (ngày)', canPhai: true, dinhDang: so }
  ], 'Không có sách quá hạn tính đến ngày cuối kỳ.')
} : null;

async function veMuonTra(d) {
  datSo('lblTongLuotMuon', d.tongLuotMuon);
  datSo('lblTongLuotGiaHan', d.tongLuotGiaHan);
  datSo('lblTongLuotTra', d.tongLuotTra);
  datSo('lblSoSachDangMuon', d.soSachDangMuon);
  datSo('lblSoSachQuaHan', d.soSachQuaHan);
  $('lblSoSachDangMuonPhu').textContent = 'Tính đến ' + ngayDayDu(d.denNgay);
  const ds = d.theoNgay;
  bieuDo.bdTheoNgay.ve({
    grid: { left: 8, right: 16, top: 40, bottom: ds.length > 31 ? 48 : 8, containLabel: true },
    legend: { top: 0, left: 0 },
    tooltip: tooltipTruc(),
    xAxis: { type: 'category', data: ds.map((x) => ngayNgan(x.ngay)) },
    yAxis: { type: 'value', minInterval: 1 },
    dataZoom: ds.length > 31 ? [{ type: 'inside' }, { type: 'slider', height: 16, bottom: 6 }] : [],
    series: [
      { name: 'Lượt mượn', type: 'bar', barMaxWidth: 14, data: ds.map((x) => x.muon) },
      { name: 'Lượt gia hạn', type: 'bar', barMaxWidth: 14, data: ds.map((x) => x.giaHan), itemStyle: { color: BANG_MAU_DO[4] } },
      { name: 'Lượt trả', type: 'bar', barMaxWidth: 14, data: ds.map((x) => x.tra), itemStyle: { color: BANG_MAU_DO[3] } }
    ]
  }, {
    ky: `${ngayDayDu(d.tuNgay)} – ${ngayDayDu(d.denNgay)}`,
    rong: d.tongLuotMuon + d.tongLuotGiaHan + d.tongLuotTra === 0,
    bang: { cot: ['Ngày', 'Lượt mượn', 'Lượt gia hạn (đồng ý)', 'Lượt trả'], dong: ds.map((x) => [ngayDayDu(x.ngay), so(x.muon), so(x.giaHan), so(x.tra)]) }
  });
  const gan = async (ten, bang, trangDau, tongSo) => {
    await ganBangChiTiet(LUOI_MT[ten], trangDau, tongSo, bang);
    document.querySelector(`[data-dem="${ten}"]`).textContent = `(${so(tongSo)})`;
  };
  await Promise.all([
    gan('luotMuon', 'luot-muon', d.luotMuon, d.soDongLuotMuon),
    gan('luotGiaHan', 'luot-gia-han', d.luotGiaHan, d.soDongLuotGiaHan),
    gan('luotTra', 'luot-tra', d.luotTra, d.soDongLuotTra),
    gan('sachQuaHan', 'sach-qua-han', d.sachQuaHan, d.soDongSachQuaHan)
  ]);
}

// ---------------- MB07 ----------------
const LUOI_VP = muonTra ? null : {
  theoKhoanThu: luoi('dgvTheoKhoanThu', 'maKhoanThu', [
    { ten: 'maKhoanThu', nhan: 'Mã khoản thu' },
    { ten: 'noiDungTt', nhan: 'Nội dung' },
    { ten: 'donGia', nhan: 'Đơn giá (đ)', canPhai: true, dinhDang: so },
    { ten: 'soTruongHop', nhan: 'Số trường hợp', canPhai: true, dinhDang: so },
    { ten: 'soLuong', nhan: 'Tổng số lượng', canPhai: true, dinhDang: so },
    { ten: 'tongTien', nhan: 'Tổng tiền (đ)', canPhai: true, dinhDang: so }
  ], 'Chưa có khoản thu.'),
  bienBan: luoi('dgvBienBan', 'soBienBan', [
    { ten: 'thoiGianLap', nhan: 'Ngày lập', dinhDang: (v) => ngayDayDu(v.slice(0, 10)) },
    { ten: 'soBienBan', nhan: 'Số biên bản' },
    { ten: 'hoTen', nhan: 'Bạn đọc', dinhDang: (v, d) => `${d.maBanDoc} – ${v}` },
    { ten: 'lopDonVi', nhan: 'Lớp/Đơn vị' },
    { ten: 'soTruongHop', nhan: 'Số trường hợp', canPhai: true, dinhDang: so },
    { ten: 'tongTien', nhan: 'Tổng tiền (đ)', canPhai: true, dinhDang: so },
    { ten: 'htThanhToan', nhan: 'Thanh toán', dinhDang: (v, d) => nhanTrangThai(v, d.daThanhToan ? 'qt-nhan-tt--ok' : 'qt-nhan-tt--canh-bao') }
  ], 'Không có biên bản vi phạm trong kỳ.')
};

async function veViPham(d) {
  datSo('lblSoBienBan', d.soBienBan);
  datSo('lblSoTruongHop', d.soTruongHop);
  datSo('lblTongTienViPham', d.tongTienViPham, tien);
  datSo('lblTongTienDaThu', d.tongTienDaThu, tien);
  datSo('lblTongTienChuaThu', d.tongTienChuaThu, tien);
  const ky = `${ngayDayDu(d.tuNgay)} – ${ngayDayDu(d.denNgay)}`;
  const kt = d.theoKhoanThu;
  bieuDo.bdTheoKhoanThu.ve({
    grid: { left: 8, right: 16, top: 40, bottom: 8, containLabel: true },
    legend: { top: 0, left: 0 },
    tooltip: tooltipTruc(),
    xAxis: { type: 'category', data: kt.map((x) => x.maKhoanThu) },
    yAxis: { type: 'value', minInterval: 1 },
    series: [
      { name: 'Đã thanh toán', type: 'bar', stack: 'tt', barMaxWidth: 36, data: kt.map((x) => x.soTruongHopDaThanhToan) },
      { name: 'Chưa thanh toán', type: 'bar', stack: 'tt', barMaxWidth: 36, data: kt.map((x) => x.soTruongHopChuaThanhToan), itemStyle: { color: BANG_MAU_DO[5] } }
    ]
  }, {
    ky, rong: d.soTruongHop === 0,
    bang: { cot: ['Khoản thu', 'Đã thanh toán', 'Chưa thanh toán', 'Tổng tiền (đ)'],
      dong: kt.map((x) => [`${x.maKhoanThu} – ${x.noiDungTt}`, so(x.soTruongHopDaThanhToan), so(x.soTruongHopChuaThanhToan), so(x.tongTien)]) }
  });
  const coSo = kt.filter((x) => x.soTruongHop > 0);
  bieuDo.bdTyLeKhoanThu.ve({
    tooltip: { trigger: 'item', formatter: (p) => `${dauMau(p.color)}${p.name}: <b>${so(p.value)}</b> trường hợp (${p.percent}%)` },
    legend: { bottom: 0, left: 'center', type: 'scroll' },
    series: [{ name: 'Khoản thu', type: 'pie', radius: ['48%', '72%'], center: ['50%', '44%'], label: { formatter: (p) => `${p.percent}%` },
      data: coSo.map((x) => ({ name: x.noiDungTt, value: x.soTruongHop })) }]
  }, {
    ky, rong: !coSo.length,
    bang: { cot: ['Khoản thu', 'Số trường hợp', 'Tổng tiền'], dong: kt.map((x) => [x.noiDungTt, so(x.soTruongHop), tienGon(x.tongTien)]) }
  });
  LUOI_VP.theoKhoanThu.tc.duLieuTinh = kt;
  await Promise.all([LUOI_VP.theoKhoanThu.tai(), ganBangChiTiet(LUOI_VP.bienBan, d.bienBan, d.soDongBienBan, 'bien-ban')]);
  document.querySelector('[data-dem="bienBan"]').textContent = `(${so(d.soDongBienBan)})`;
}

// ---------------- btnTongHop ----------------
async function tongHop() {
  const ky = docKy();
  if (!ky) return;
  const nut = $('btnTongHop');
  nut.disabled = true;
  try {
    const d = await api.lay(`/api/quan-tri/bao-cao/${muonTra ? 'muon-tra' : 'vi-pham'}` + chuoiTruyVan(ky));
    $('khuKetQua').hidden = false;
    kyDangXem = ky;
    coTrangChiTiet = d.coTrang || 50;
    await (muonTra ? veMuonTra(d) : veViPham(d));
    kyDaTongHop.gt = ky;
    $('btnLapBaoCao').disabled = false;
  } catch (e) {
    baoLoiKy(e);
  } finally {
    nut.disabled = false;
  }
}

ganThe();
$('btnTongHop').addEventListener('click', tongHop);
// Đổi kỳ sau khi tổng hợp: phải tổng hợp lại mới lập báo cáo
['dtpTuNgay', 'dtpDenNgay'].forEach((id) => $(id).addEventListener('change', () => { $('btnLapBaoCao').disabled = true; }));
document.addEventListener('ky-nhanh', () => { $('btnLapBaoCao').disabled = true; });
$('btnLapBaoCao').addEventListener('click', () => {
  if (!kyDaTongHop.gt) return;
  location.href = `/quan-tri/bao-cao/${muonTra ? 'muon-tra' : 'vi-pham'}/lap` + chuoiTruyVan(kyDaTongHop.gt);
});
$('btnDong').addEventListener('click', () => { location.href = '/quan-tri'; });

if (ganKy()) await tongHop();
