// Quầy đặt chỗ (file 06 mục 5 – mở rộng): danh sách “Đang giữ” sắp theo hạn gần nhất, đếm ngược (đỏ khi < 2 giờ),
// lọc trạng thái / ngày / mã SV, hủy đặt chỗ có lý do. Không có nút “Nhận” (nhận qua 01.01.03 – btnNhanDatCho).
import { api, chuoiTruyVan } from '../../api.js';
import { LuoiDuLieu } from '../luoi.js';
import { xacNhan, thongBao } from '../hop-thoai.js';
import { layNgay } from '../../chon-ngay.js';
import { nhanTrangThai } from '../danh-muc.js';

const $ = (id) => document.getElementById(id);
const huyDuoc = $('frmQuayDatCho').dataset.huyDuoc === '1';
const HAI_GIO = 2 * 3600000;
const p2 = (n) => String(n).padStart(2, '0');
const ngayGio = (v) => { const d = new Date(v); return `${p2(d.getHours())}:${p2(d.getMinutes())} ${p2(d.getDate())}/${p2(d.getMonth() + 1)}`; };
const MAU = { 'Đang giữ': 'qt-nhan-tt--ok', 'Đã nhận': 'qt-nhan-tt--tin', 'Hết hạn': 'qt-nhan-tt--canh-bao', 'Đã hủy': 'qt-nhan-tt--loi' };

function demNguoc(v, d) {
  if (d.trangThai !== 'Đang giữ') return '—';
  const con = new Date(v) - Date.now();
  if (con <= 0) return nhanTrangThai('Hết hạn', 'qt-nhan-tt--loi');
  const phut = Math.floor(con / 60000);
  const chu = `${Math.floor(phut / 60)} giờ ${p2(phut % 60)} phút`;
  const s = document.createElement('span');
  s.className = 'qt-dem-nguoc' + (con < HAI_GIO ? ' qt-dem-nguoc--gap' : '');
  s.dataset.hetHan = v;
  s.textContent = chu;
  return s;
}

const luoi = new LuoiDuLieu($('dgvDatChoQuay'), {
  khoa: 'maDatCho', duLieuTinh: [], coTrang: 20,
  rong: 'Không có đặt chỗ phù hợp điều kiện lọc.',
  cot: [
    { ten: 'maDatCho', nhan: 'Mã đặt chỗ' },
    { ten: 'hoTen', nhan: 'Bạn đọc', dinhDang: (v, d) => `${v} (${d.maSvHvCb})` },
    { ten: 'tenSach', nhan: 'Tên sách' },
    { ten: 'maBanSach', nhan: 'Mã bản sách' },
    { ten: 'viTri', nhan: 'Khu vực – vị trí', dinhDang: (v, d) => `${d.maKhuVuc} – ${v}` },
    { ten: 'tgHetHan', nhan: 'Hạn giữ', dinhDang: ngayGio },
    { ten: 'tgHetHan', nhan: 'Còn lại', dinhDang: demNguoc },
    { ten: 'trangThai', nhan: 'Trạng thái', dinhDang: (v, d) => {
      const n = nhanTrangThai(v, MAU[v] || 'qt-nhan-tt--tin');
      return d.soPMuon ? `${v} · ${d.soPMuon}` : d.lyDoHuy ? `${v} · ${d.lyDoHuy}` : n;
    } }
  ],
  khiChon: (d) => { if ($('btnHuyDatCho')) $('btnHuyDatCho').disabled = !(d && d.trangThai === 'Đang giữ'); },
  khiNhapDup: (d) => { if (huyDuoc && d.trangThai === 'Đang giữ') huy(d); }
});

async function loc() {
  const tuNgay = layNgay($('dtpTuNgay')), denNgay = layNgay($('dtpDenNgay'));
  if (tuNgay && denNgay && tuNgay > denNgay) { thongBao('Từ ngày phải nhỏ hơn hoặc bằng đến ngày.', 'loi'); return; }
  try {
    const ds = await api.lay('/api/quan-tri/dat-cho' + chuoiTruyVan({ trangThai: $('cboTrangThai').value, tuNgay, denNgay, maSv: $('txtMaSV').value.trim() }));
    luoi.tc.duLieuTinh = ds;
    luoi.trang = 1;
    await luoi.tai();
    $('soDatCho').textContent = String(ds.length);
    $('soSapHetHan').textContent = String(ds.filter((d) => d.trangThai === 'Đang giữ' && new Date(d.tgHetHan) - Date.now() < HAI_GIO).length);
    if ($('btnHuyDatCho')) $('btnHuyDatCho').disabled = true;
  } catch (e) { thongBao(e.message, 'loi'); }
}

/** Hủy đặt chỗ: bắt buộc nhập lý do, xác nhận (file 06 mục 5). */
async function huy(d) {
  const form = document.createElement('div');
  form.innerHTML = `
    <p class="mb-2">Đặt chỗ <strong></strong> – <span></span></p>
    <label class="qt-nhan" for="dlgLyDoHuy">Lý do hủy<span class="bat-buoc" aria-hidden="true">*</span></label>
    <textarea class="qt-o" id="dlgLyDoHuy" maxlength="200" rows="3" placeholder="Ví dụ: Bản sách bị hư hỏng, bạn đọc báo không đến nhận"></textarea>`;
  form.querySelector('strong').textContent = d.maDatCho;
  form.querySelector('span').textContent = `${d.tenSach} – ${d.hoTen} (${d.maSvHvCb})`;
  if (!(await xacNhan({ tieuDe: 'Hủy đặt chỗ', noiDung: form, nutDongY: 'Hủy đặt chỗ', nutHuy: 'Không', nguyHiem: true }))) return;
  const lyDo = form.querySelector('#dlgLyDoHuy').value.trim();
  if (!lyDo) { thongBao('Vui lòng nhập lý do hủy đặt chỗ.', 'loi'); return; }
  try {
    await api.them(`/api/quan-tri/dat-cho/${encodeURIComponent(d.maDatCho)}/huy`, { lyDo });
    thongBao(`Đã hủy đặt chỗ ${d.maDatCho}. Bản sách trở lại có thể mượn.`, 'ok');
    document.dispatchEvent(new CustomEvent('ulms:dat-cho-thay-doi'));
    await loc();
  } catch (e) { thongBao(e.message, 'loi'); }
}

$('btnLoc').addEventListener('click', loc);
$('txtMaSV').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); loc(); } });
$('btnHuyDatCho')?.addEventListener('click', () => { if (luoi.chon && luoi.chon.trangThai === 'Đang giữ') huy(luoi.chon); });
$('btnDong').addEventListener('click', () => { location.href = '/quan-tri'; });

// Đếm ngược cập nhật mỗi 30 giây (không vẽ lại cả lưới)
setInterval(() => {
  document.querySelectorAll('.qt-dem-nguoc').forEach((el) => {
    const con = new Date(el.dataset.hetHan) - Date.now();
    if (con <= 0) { el.textContent = 'Hết hạn'; el.classList.add('qt-dem-nguoc--gap'); return; }
    const phut = Math.floor(con / 60000);
    el.textContent = `${Math.floor(phut / 60)} giờ ${p2(phut % 60)} phút`;
    el.classList.toggle('qt-dem-nguoc--gap', con < HAI_GIO);
  });
}, 30000);

await loc();
