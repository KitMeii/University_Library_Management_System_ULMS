// Trang tổng quan theo nhóm (file 08 mục 4, file 09 mục 3, file 10 mục 4): thẻ KPI đếm lên, bấm được → chi tiết;
// biểu đồ ECharts theo nhóm N1–N4. Đặt chỗ (N1) bật lại từ Pha 6 (GIA_DINH D50) – chỉ vẽ khi trang có ô tương ứng
// (ẩn ở chế độ chụp ảnh Bài 4 – D61).
import { api } from '../../api.js';
import { motion, giamChuyenDong } from '../../motion.config.js';
import { BieuDo, dauMau, so, tien, tienGon, ngayNgan, ngayDayDu, thangNam, tooltipTruc } from '../../charts/chart-factory.js';
import { BANG_MAU_DO, THANG_DO } from '../../charts/chart-theme.js';
import { LuoiDuLieu } from '../luoi.js';
import { thongBao } from '../hop-thoai.js';

const $ = (id) => document.getElementById(id);
const nhom = $('trangTongQuan').dataset.nhom;
const bieuDo = {};
document.querySelectorAll('.qt-bieu-do').forEach((h) => { bieuDo[h.id] = new BieuDo(h); });

const tong = (ds) => ds.reduce((s, x) => s + x, 0);
const rutGon = (s, n) => (s.length > n ? s.slice(0, n - 1) + '…' : s);
const phanTram = (a, b) => (b ? Math.round((a / b) * 100) : 0);

/** KPI: số đếm lên (dashboard.countUp), thanh tiến độ mảnh chạy từ 0 (file 10 mục 4). */
function datKpi(id, giaTri, { dinhDang = so, tiLe = 1, phu } = {}) {
  const el = $(id);
  if (!el) return;
  const thanh = el.closest('.qt-kpi').querySelector('.qt-kpi__thanh span');
  if (phu !== undefined) $(id + 'Phu').textContent = phu;
  const cuoi = Number(giaTri) || 0;
  const ti = Math.max(0, Math.min(1, tiLe));
  if (window.gsap && !giamChuyenDong()) {
    const o = { v: Number(el.dataset.giaTri || 0) };
    gsap.to(o, { v: cuoi, duration: motion.dashboard.countUp, ease: motion.ease.out, onUpdate: () => { el.textContent = dinhDang(Math.round(o.v)); } });
    gsap.fromTo(thanh, { scaleX: 0 }, { scaleX: ti, duration: motion.dashboard.countUp, ease: motion.ease.out });
  } else {
    el.textContent = dinhDang(cuoi);
    thanh.style.transform = `scaleX(${ti})`;
  }
  el.dataset.giaTri = String(cuoi);
}

const kyNgay = (ds) => (ds.length ? `${ngayDayDu(ds[0])} – ${ngayDayDu(ds[ds.length - 1])}` : '');
const truc = (data, them = {}) => ({ type: 'category', data, ...them });
const trucSo = (them = {}) => ({ type: 'value', minInterval: 1, ...them });

// ---------------- N1 – Mượn trả sách ----------------
async function veN1() {
  const d = await api.lay('/api/quan-tri/thong-ke/n1');
  datKpi('kpiLuotMuonHomNay', d.luotMuonHomNay, { phu: 'Ngày ' + ngayDayDu(d.homNay) });
  datKpi('kpiDangMuon', d.dangMuon);
  datKpi('kpiQuaHan', d.quaHan, { tiLe: d.dangMuon ? d.quaHan / d.dangMuon : 0,
    phu: d.dangMuon ? `${phanTram(d.quaHan, d.dangMuon)}% số bản đang mượn` : 'Không có bản sách đang mượn' });
  datKpi('kpiDatChoDangGiu', d.datChoDangGiu, { tiLe: d.datChoDangGiu ? d.datChoSapHetHan / d.datChoDangGiu : 0,
    phu: d.datChoSapHetHan ? `${so(d.datChoSapHetHan)} sắp hết hạn (< 2 giờ)` : 'Không có đặt chỗ sắp hết hạn' });
  if (bieuDo.bdDatCho) {
    const kq = d.datChoThangNay;
    bieuDo.bdDatCho.ve({
      tooltip: { trigger: 'item', formatter: (p) => `${dauMau(p.color)}${p.name}: <b>${so(p.value)}</b> đặt chỗ (${p.percent}%)` },
      legend: { bottom: 0, left: 'center' },
      series: [{ name: 'Đặt chỗ theo kết quả', type: 'pie', radius: ['48%', '72%'], center: ['50%', '45%'],
        label: { formatter: (p) => `${p.percent}%` }, data: kq.filter((x) => x.so > 0).map((x) => ({ name: x.ketQua, value: x.so })) }]
    }, {
      rong: tong(kq.map((x) => x.so)) === 0,
      bang: { cot: ['Kết quả', 'Số đặt chỗ'], dong: kq.map((x) => [x.ketQua, so(x.so)]) }
    });
  }

  const m30 = d.muonTra30Ngay;
  bieuDo.bdMuonTra30.ve({
    grid: { left: 8, right: 16, top: 40, bottom: 48, containLabel: true },
    legend: { top: 0, left: 0 },
    tooltip: tooltipTruc(),
    xAxis: truc(m30.map((x) => ngayNgan(x.ngay)), { boundaryGap: false }),
    yAxis: trucSo(),
    dataZoom: [{ type: 'inside' }, { type: 'slider', height: 16, bottom: 6 }],
    series: [
      { name: 'Lượt mượn', type: 'line', data: m30.map((x) => x.muon), areaStyle: { opacity: 0.14 } },
      { name: 'Lượt trả', type: 'line', data: m30.map((x) => x.tra), areaStyle: { opacity: 0.08 } }
    ]
  }, {
    ky: kyNgay(m30.map((x) => x.ngay)), rong: tong(m30.map((x) => x.muon + x.tra)) === 0,
    moTa: `Lượt mượn và lượt trả mỗi ngày trong 30 ngày gần nhất: tổng ${so(tong(m30.map((x) => x.muon)))} lượt mượn, ${so(tong(m30.map((x) => x.tra)))} lượt trả.`,
    bang: { cot: ['Ngày', 'Lượt mượn', 'Lượt trả'], dong: m30.map((x) => [ngayDayDu(x.ngay), so(x.muon), so(x.tra)]) }
  });

  const top = [...d.top10].reverse();
  bieuDo.bdTop10.ve({
    grid: { left: 8, right: 36, top: 8, bottom: 8, containLabel: true },
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' },
      formatter: (p) => `<b>${top[p[0].dataIndex].tenSach}</b><br>${top[p[0].dataIndex].tacGia}<br>${dauMau(p[0].color)}${so(p[0].value)} lượt mượn` },
    xAxis: trucSo(),
    yAxis: truc(top.map((x) => rutGon(x.tenSach, 26))),
    series: [{ name: 'Lượt mượn', type: 'bar', data: top.map((x) => x.soLuot), barMaxWidth: 16,
      itemStyle: { borderRadius: [0, 4, 4, 0] }, label: { show: true, position: 'right', formatter: (p) => so(p.value) } }]
  }, {
    rong: !top.length,
    bang: { cot: ['Tên sách', 'Tác giả', 'Lượt mượn'], dong: d.top10.map((x) => [x.tenSach, x.tacGia, so(x.soLuot)]) }
  });

  const lich = d.theoNgayTrongNam;
  const max = Math.max(1, ...lich.map((x) => x.soLuot));
  bieuDo.bdLichMuon.ve({
    tooltip: { formatter: (p) => `${ngayDayDu(p.value[0])}: <b>${so(p.value[1])}</b> lượt mượn` },
    visualMap: { min: 0, max, orient: 'horizontal', left: 'center', bottom: 0, itemWidth: 10, itemHeight: 110, calculable: false,
      text: ['Nhiều', 'Ít'], inRange: { color: THANG_DO } },
    calendar: { range: [lich[0].ngay, lich[lich.length - 1].ngay], top: 28, left: 34, right: 8, bottom: 44, cellSize: ['auto', 13],
      yearLabel: { show: false },
      dayLabel: { firstDay: 1, nameMap: ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'], fontSize: 10 },
      monthLabel: { nameMap: ['Th1', 'Th2', 'Th3', 'Th4', 'Th5', 'Th6', 'Th7', 'Th8', 'Th9', 'Th10', 'Th11', 'Th12'], fontSize: 10 } },
    series: [{ name: 'Lượt mượn', type: 'heatmap', coordinateSystem: 'calendar', data: lich.map((x) => [x.ngay, x.soLuot]) }]
  }, {
    ky: kyNgay(lich.map((x) => x.ngay)), rong: tong(lich.map((x) => x.soLuot)) === 0,
    bang: { cot: ['Ngày', 'Lượt mượn'], dong: lich.filter((x) => x.soLuot > 0).map((x) => [ngayDayDu(x.ngay), so(x.soLuot)]) }
  });
}

// ---------------- N2 – Xử lý vi phạm ----------------
async function veN2() {
  const d = await api.lay('/api/quan-tri/thong-ke/n2');
  datKpi('kpiBienBanThang', d.bienBanThangNay, { phu: 'Tháng ' + thangNam(d.thang[d.thang.length - 1]) });
  datKpi('kpiTienThu', d.tienThuThangNay, { dinhDang: tien, phu: 'Biên bản đã thanh toán' });
  datKpi('kpiChuaThanhToan', d.chuaThanhToan, { phu: `Tổng ${tien(d.tienChuaThanhToan)}` });

  const thang = d.thang.map(thangNam);
  const coSo = d.viPhamTheoThang.filter((c) => tong(c.soTheoThang) > 0);
  bieuDo.bdViPhamTheoThang.ve({
    grid: { left: 8, right: 16, top: 40, bottom: 8, containLabel: true },
    legend: { top: 0, left: 0, type: 'scroll' },
    tooltip: tooltipTruc(),
    xAxis: truc(thang),
    yAxis: trucSo(),
    series: coSo.map((c) => ({ name: c.noiDungTt, type: 'bar', stack: 'viPham', barMaxWidth: 28, data: c.soTheoThang, emphasis: { focus: 'series' } }))
  }, {
    ky: `${thang[0]} – ${thang[thang.length - 1]}`, rong: !coSo.length,
    bang: { cot: ['Tháng', ...d.viPhamTheoThang.map((c) => c.noiDungTt)], dong: thang.map((t, i) => [t, ...d.viPhamTheoThang.map((c) => so(c.soTheoThang[i]))]) }
  });

  bieuDo.bdTienThu.ve({
    grid: { left: 8, right: 16, top: 24, bottom: 8, containLabel: true },
    tooltip: tooltipTruc(tien),
    xAxis: truc(thang, { boundaryGap: false }),
    yAxis: { type: 'value', axisLabel: { formatter: tienGon } },
    series: [{ name: 'Tiền thu', type: 'line', data: d.tienThuTheoThang, areaStyle: { opacity: 0.14 } }]
  }, {
    ky: `${thang[0]} – ${thang[thang.length - 1]}`, rong: tong(d.tienThuTheoThang) === 0,
    bang: { cot: ['Tháng', 'Tiền thu (đ)'], dong: thang.map((t, i) => [t, so(d.tienThuTheoThang[i])]) }
  });

  const ht = d.hinhThucThanhToan.filter((x) => x.soBienBan > 0);
  bieuDo.bdHinhThuc.ve({
    tooltip: { trigger: 'item', formatter: (p) => `${dauMau(p.color)}${p.name}: <b>${so(p.value)}</b> biên bản (${p.percent}%)` },
    legend: { bottom: 0, left: 'center' },
    series: [{ name: 'Hình thức thanh toán', type: 'pie', radius: ['48%', '72%'], center: ['50%', '45%'],
      label: { formatter: (p) => `${p.percent}%` }, data: ht.map((x) => ({ name: x.htThanhToan, value: x.soBienBan })) }]
  }, {
    rong: !ht.length,
    bang: { cot: ['Hình thức', 'Số biên bản', 'Tổng tiền (đ)'], dong: d.hinhThucThanhToan.map((x) => [x.htThanhToan, so(x.soBienBan), so(x.tongTien)]) }
  });
}

// ---------------- N3 – Kho sách ----------------
async function veN3() {
  const d = await api.lay('/api/quan-tri/thong-ke/n3');
  datKpi('kpiTongBanSach', d.tongBanSach, { phu: `${so(d.theoKhuVuc.length)} khu vực lưu trữ` });
  datKpi('kpiSanSang', d.sanSang, { tiLe: d.tongBanSach ? d.sanSang / d.tongBanSach : 0, phu: `${phanTram(d.sanSang, d.tongBanSach)}% tổng bản sách` });
  datKpi('kpiDangDuocMuon', d.dangDuocMuon, { tiLe: d.tongBanSach ? d.dangDuocMuon / d.tongBanSach : 0, phu: `${phanTram(d.dangDuocMuon, d.tongBanSach)}% tổng bản sách` });
  datKpi('kpiHuHongMat', d.huHong + d.mat, { tiLe: d.tongBanSach ? (d.huHong + d.mat) / d.tongBanSach : 0, phu: `Hư hỏng ${so(d.huHong)} · Mất ${so(d.mat)}` });

  const tt = d.theoTinhTrang.filter((x) => x.soSach > 0);
  bieuDo.bdTinhTrang.ve({
    tooltip: { trigger: 'item', formatter: (p) => `${dauMau(p.color)}${p.name}: <b>${so(p.value)}</b> bản (${p.percent}%)` },
    legend: { bottom: 0, left: 'center' },
    series: [{ name: 'Tình trạng', type: 'pie', radius: ['48%', '72%'], center: ['50%', '45%'],
      label: { formatter: (p) => `${p.percent}%` }, data: tt.map((x) => ({ name: x.tinhTrang, value: x.soSach })) }]
  }, {
    rong: !tt.length,
    bang: { cot: ['Tình trạng', 'Số bản sách'], dong: d.theoTinhTrang.map((x) => [x.tinhTrang, so(x.soSach)]) }
  });

  bieuDo.bdKhuVuc.ve({
    grid: { left: 8, right: 16, top: 24, bottom: 8, containLabel: true },
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' },
      formatter: (p) => { const k = d.theoKhuVuc[p[0].dataIndex]; return `<b>${k.maKhuVuc} – ${k.viTri}</b> (${k.loaiKhuVuc})<br>${dauMau(p[0].color)}${so(p[0].value)} bản sách`; } },
    xAxis: truc(d.theoKhuVuc.map((k) => k.maKhuVuc)),
    yAxis: trucSo(),
    series: [{ name: 'Số bản sách', type: 'bar', barMaxWidth: 36, data: d.theoKhuVuc.map((k) => k.soSach),
      label: { show: true, position: 'top', formatter: (p) => so(p.value) } }]
  }, {
    rong: !d.theoKhuVuc.length,
    bang: { cot: ['Khu vực', 'Vị trí', 'Loại', 'Số bản sách'], dong: d.theoKhuVuc.map((k) => [k.maKhuVuc, k.viTri, k.loaiKhuVuc, so(k.soSach)]) }
  });

  const thang = d.thang.map(thangNam);
  bieuDo.bdHuHongMat.ve({
    grid: { left: 8, right: 16, top: 40, bottom: 8, containLabel: true },
    legend: { top: 0, left: 0 },
    tooltip: tooltipTruc(),
    xAxis: truc(thang),
    yAxis: trucSo(),
    series: [
      { name: 'Hư hỏng', type: 'bar', stack: 'tra', barMaxWidth: 28, data: d.huHongTheoThang },
      { name: 'Mất', type: 'bar', stack: 'tra', barMaxWidth: 28, data: d.matTheoThang, itemStyle: { color: BANG_MAU_DO[2] } }
    ]
  }, {
    ky: `${thang[0]} – ${thang[thang.length - 1]}`, rong: tong(d.huHongTheoThang) + tong(d.matTheoThang) === 0,
    bang: { cot: ['Tháng', 'Hư hỏng', 'Mất'], dong: thang.map((t, i) => [t, so(d.huHongTheoThang[i]), so(d.matTheoThang[i])]) }
  });
}

// ---------------- N4 – Quản trị ----------------
async function veN4() {
  const d = await api.lay('/api/quan-tri/thong-ke/n4');
  datKpi('kpiTaiKhoan', d.taiKhoanHoatDong, { tiLe: d.tongTaiKhoan ? d.taiKhoanHoatDong / d.tongTaiKhoan : 0, phu: `trên ${so(d.tongTaiKhoan)} tài khoản` });
  datKpi('kpiDangNhapHomNay', d.dangNhapHomNay);
  datKpi('kpiThaoTacHomNay', d.thaoTacHomNay);
  const sl = $('kpiSaoLuu');
  if (d.saoLuuGanNhat) {
    const [ngay, gio] = d.saoLuuGanNhat.split('T');
    sl.textContent = `${ngayDayDu(ngay)} ${gio.slice(0, 5)}`;
    sl.classList.add('qt-kpi__so--chu');
    const cach = Math.max(0, Math.round((Date.now() - Date.parse(d.saoLuuGanNhat)) / 86400000));
    $('kpiSaoLuuPhu').textContent = (cach === 0 ? 'Hôm nay' : `${cach} ngày trước`) + (d.saoLuuNoiDung?.includes('tự động') ? ' · tự động' : ' · thủ công');
  } else {
    sl.textContent = 'Chưa có';
    $('kpiSaoLuuPhu').textContent = 'Chưa sao lưu lần nào';
  }
  sl.closest('.qt-kpi').querySelector('.qt-kpi__thanh span').style.transform = 'scaleX(0)';

  const dn = d.dangNhap30Ngay;
  bieuDo.bdDangNhap30.ve({
    grid: { left: 8, right: 16, top: 24, bottom: 48, containLabel: true },
    tooltip: tooltipTruc(),
    xAxis: truc(dn.map((x) => ngayNgan(x.ngay)), { boundaryGap: false }),
    yAxis: trucSo(),
    dataZoom: [{ type: 'inside' }, { type: 'slider', height: 16, bottom: 6 }],
    series: [{ name: 'Lượt đăng nhập', type: 'line', data: dn.map((x) => x.soLuot), areaStyle: { opacity: 0.12 } }]
  }, {
    ky: kyNgay(dn.map((x) => x.ngay)), rong: tong(dn.map((x) => x.soLuot)) === 0,
    bang: { cot: ['Ngày', 'Lượt đăng nhập'], dong: dn.map((x) => [ngayDayDu(x.ngay), so(x.soLuot)]) }
  });

  const ngay = d.ngay14.map(ngayNgan);
  const coSo = d.thaoTacTheoNhom.filter((c) => tong(c.soTheoNgay) > 0);
  bieuDo.bdThaoTacNhom.ve({
    grid: { left: 8, right: 16, top: 40, bottom: 8, containLabel: true },
    legend: { top: 0, left: 0, type: 'scroll' },
    tooltip: tooltipTruc(),
    xAxis: truc(ngay),
    yAxis: trucSo(),
    series: coSo.map((c) => ({ name: `${c.maNhom} – ${c.tenNhom}`, type: 'bar', stack: 'thaoTac', barMaxWidth: 28, data: c.soTheoNgay, emphasis: { focus: 'series' } }))
  }, {
    ky: kyNgay(d.ngay14), rong: !coSo.length,
    bang: { cot: ['Ngày', ...d.thaoTacTheoNhom.map((c) => c.maNhom)], dong: d.ngay14.map((n, i) => [ngayDayDu(n), ...d.thaoTacTheoNhom.map((c) => so(c.soTheoNgay[i]))]) }
  });
}

// ---------------- Thẻ KPI bấm được → danh sách chi tiết (file 08 mục 4) ----------------
const COT_SACH = [
  { ten: 'soPMuon', nhan: 'Số phiếu mượn' },
  { ten: 'hoTen', nhan: 'Bạn đọc', dinhDang: (v, d) => `${d.maBanDoc} – ${v}` },
  { ten: 'maBanSach', nhan: 'Bản sách', dinhDang: (v, d) => `${v} – ${d.tenSach}` },
  { ten: 'ngayMuon', nhan: 'Ngày mượn', dinhDang: ngayDayDu },
  { ten: 'hanTra', nhan: 'Hạn trả', dinhDang: ngayDayDu },
  { ten: 'soNgayQuaHan', nhan: 'Quá hạn (ngày)', canPhai: true, dinhDang: (v) => (v ? so(v) : '—') }
];
const CHI_TIET = {
  'dang-muon': { tieuDe: 'Bản sách đang mượn', url: '/api/quan-tri/thong-ke/n1/sach?loai=dang-muon', khoa: 'maBanSach', cot: COT_SACH,
    rong: 'Không có bản sách đang mượn.' },
  'qua-han': { tieuDe: 'Bản sách quá hạn trả', url: '/api/quan-tri/thong-ke/n1/sach?loai=qua-han', khoa: 'maBanSach', cot: COT_SACH,
    rong: 'Không có bản sách quá hạn.' },
  'chua-thanh-toan': { tieuDe: 'Biên bản vi phạm chưa thanh toán', url: '/api/quan-tri/thong-ke/n2/chua-thanh-toan', khoa: 'soBienBan',
    rong: 'Không có biên bản chưa thanh toán.',
    cot: [
      { ten: 'soBienBan', nhan: 'Số biên bản' },
      { ten: 'thoiGianLap', nhan: 'Ngày lập', dinhDang: (v) => ngayDayDu(v.slice(0, 10)) },
      { ten: 'hoTen', nhan: 'Bạn đọc', dinhDang: (v, d) => `${d.maBanDoc} – ${v}` },
      { ten: 'lopDonVi', nhan: 'Lớp/Đơn vị' },
      { ten: 'soTruongHop', nhan: 'Số trường hợp', canPhai: true, dinhDang: so },
      { ten: 'tongTien', nhan: 'Tổng tiền (đ)', canPhai: true, dinhDang: so }
    ] }
};

async function moChiTiet(loai) {
  const ch = CHI_TIET[loai];
  if (!ch) return;
  const el = document.createElement('div');
  el.className = 'modal fade';
  el.tabIndex = -1;
  el.setAttribute('aria-labelledby', 'chiTietKpiTieuDe');
  el.innerHTML = `
    <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
      <div class="modal-content qt-modal">
        <div class="modal-header">
          <h2 class="modal-title fs-5" id="chiTietKpiTieuDe"></h2>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Đóng"></button>
        </div>
        <div class="modal-body"><div class="qt-luoi" id="dgvChiTietKpi"></div></div>
      </div>
    </div>`;
  el.querySelector('h2').textContent = ch.tieuDe;
  el.querySelector('.qt-luoi').setAttribute('aria-label', ch.tieuDe);
  document.body.append(el);
  const modal = new bootstrap.Modal(el);
  el.addEventListener('hidden.bs.modal', () => el.remove());
  modal.show();
  try {
    const ds = await api.lay(ch.url);
    const luoi = new LuoiDuLieu(el.querySelector('.qt-luoi'), { khoa: ch.khoa, duLieuTinh: ds, cot: ch.cot, rong: ch.rong });
    await luoi.tai();
    el.querySelector('h2').textContent = `${ch.tieuDe} (${so(ds.length)})`;
  } catch (e) { thongBao(e.message, 'loi'); }
}
document.querySelectorAll('[data-chi-tiet]').forEach((b) => b.addEventListener('click', () => moChiTiet(b.dataset.chiTiet)));

// ---------------- Tải số liệu ----------------
const VE = { N1: veN1, N2: veN2, N3: veN3, N4: veN4 };
async function tai() {
  try {
    await VE[nhom]();
    $('thoiDiemCapNhat').textContent = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  } catch (e) {
    Object.values(bieuDo).forEach((b) => b.loi(e.message));
    thongBao(e.message || 'Không tải được số liệu tổng quan.', 'loi');
  }
}

if (window.gsap && !giamChuyenDong()) {
  gsap.from('.qt-kpi, .qt-bieu-do', { y: 12, opacity: 0, duration: motion.dashboard.pageIn, stagger: motion.dashboard.cardStagger,
    ease: motion.ease.out, clearProps: 'opacity,transform' });
}
$('btnLamMoi')?.addEventListener('click', tai);
await tai();
