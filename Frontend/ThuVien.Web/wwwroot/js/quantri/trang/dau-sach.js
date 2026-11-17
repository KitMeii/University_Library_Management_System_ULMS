// Quản lý đầu sách (X02 – mở rộng, cổng bạn đọc) + gắn bản sách vào đầu sách (X03 – GIA_DINH D55).
// Cùng mẫu nút/ô với danh mục SPEC 6.1.x (FormDanhMuc). Ảnh bìa lưu 2 cỡ (lớn/nhỏ) qua /quan-tri/tai-len/anh-bia.
import { api, LoiApi } from '../../api.js';
import { FormDanhMuc, nhanTrangThai } from '../danh-muc.js';
import { LuoiDuLieu } from '../luoi.js';
import { thongBao, xacNhan } from '../hop-thoai.js';

const $ = (id) => document.getElementById(id);
const coSua = !!$('dgvBanSachChuaGan');
const lopTinhTrang = { 'Sẵn sàng': 'qt-nhan-tt--ok', 'Đang được mượn': 'qt-nhan-tt--tin', 'Hư hỏng': 'qt-nhan-tt--canh-bao', 'Mất': 'qt-nhan-tt--loi' };

// Thể loại cho ô chọn và bộ lọc
const theLoai = await api.lay('/api/quan-tri/the-loai/tat-ca');
for (const t of theLoai) {
  $('cboTheLoai').add(new Option(`${t.tenTheLoai}`, t.maTheLoai));
  $('cboLocTheLoai').add(new Option(t.tenTheLoai, t.maTheLoai));
}

function oBia(d) {
  const o = document.createElement('span');
  o.className = 'qt-bia-nho';
  if (d.anhBiaNho) {
    const img = document.createElement('img');
    img.src = d.anhBiaNho; img.alt = ''; img.width = 24; img.height = 36; img.loading = 'lazy';
    o.append(img);
  } else {
    o.classList.add('qt-bia-nho--chu');
    if (d.mauGay) o.style.backgroundColor = d.mauGay; // CSSOM – không phải style nội tuyến (CSP)
  }
  return o;
}

const form = new FormDanhMuc({
  api: '/api/quan-tri/dau-sach',
  khoa: 'maDauSach',
  doiTuong: 'đầu sách',
  moTa: (d) => `${d.maDauSach} – ${d.tenSach}`,
  truong: [
    { id: 'txtMaDauSach', ten: 'maDauSach', khoa: true },
    { id: 'txtTenSach', ten: 'tenSach' },
    { id: 'txtTacGia', ten: 'tacGia' },
    { id: 'cboTheLoai', ten: 'maTheLoai' },
    { id: 'txtNhaXuatBan', ten: 'nhaXuatBan' },
    { id: 'txtNamXb', ten: 'namXb', loai: 'so' },
    { id: 'txtIsbn', ten: 'isbn' },
    { id: 'txtMauGay', ten: 'mauGay' },
    { id: 'txtTuKhoa', ten: 'tuKhoa' },
    { id: 'txtGioiThieu', ten: 'gioiThieu' },
    { id: 'txtAnhBia', ten: 'anhBia' },
    { id: 'fileAnhBia', ten: 'fileAnhBia', khongGui: true }
  ],
  luoi: {
    id: 'dgvDauSach', sapXep: 'MaDauSach', coTrang: 12,
    thamSoThem: () => ({ maTheLoai: $('cboLocTheLoai').value }),
    cot: [
      { ten: 'anhBiaNho', nhan: 'Bìa', dinhDang: (v, d) => oBia(d) },
      { ten: 'maDauSach', nhan: 'Mã đầu sách', sapXep: 'MaDauSach' },
      { ten: 'tenSach', nhan: 'Tên sách', sapXep: 'TenSach' },
      { ten: 'tacGia', nhan: 'Tác giả', sapXep: 'TacGia' },
      { ten: 'tenTheLoai', nhan: 'Thể loại' },
      { ten: 'namXb', nhan: 'Năm XB', sapXep: 'NamXb', canPhai: true },
      { ten: 'soBanSach', nhan: 'Số bản', canPhai: true },
      { ten: 'soBanCoTheMuon', nhan: 'Có thể mượn', canPhai: true }
    ]
  },
  khiChon: (d) => { hienAnh(d.anhBia); hienMau(d.mauGay); taiBanSach(d); },
  khiThem: async () => {
    hienAnh(null); hienMau(''); anBanSach();
    try { $('txtMaDauSach').value = (await api.lay('/api/quan-tri/dau-sach/ma-ke-tiep')).ma; } catch { /* để trống */ }
  },
  khiVeTaiForm: () => { hienAnh(null); hienMau(''); anBanSach(); }
});

// ---------- Màu gáy: ô xem trước ----------
function hienMau(mau) {
  const hop = /^#[0-9A-Fa-f]{6}$/.test(mau || '');
  $('oMauGay').classList.toggle('qt-mau-gay__o--trong', !hop);
  if (hop) $('oMauGay').style.backgroundColor = mau; else $('oMauGay').style.removeProperty('background-color');
}
$('txtMauGay').addEventListener('input', () => hienMau($('txtMauGay').value.trim()));

// ---------- Ảnh bìa ----------
function hienAnh(duongDan) {
  $('txtAnhBia').value = duongDan || '';
  $('imgAnhBia').hidden = !duongDan;
  if (duongDan) $('imgAnhBia').src = duongDan.replace(/\.webp$/, '_nho.webp');
  $('fileAnhBia').value = '';
  $('tenTepAnhBia').textContent = duongDan ? 'Đã có ảnh bìa' : 'Chưa có ảnh – cổng hiển thị bìa chữ theo màu gáy';
}
$('fileAnhBia').addEventListener('change', async () => {
  const tep = $('fileAnhBia').files[0];
  if (!tep) return;
  const fd = new FormData();
  fd.append('tep', tep);
  const res = await fetch('/quan-tri/tai-len/anh-bia', {
    method: 'POST', body: fd, credentials: 'same-origin',
    headers: { 'RequestVerificationToken': document.querySelector('meta[name="csrf-token"]').content }
  });
  const kq = await res.json().catch(() => null);
  if (!res.ok) { thongBao(kq?.thongBao || 'Không tải được ảnh bìa.', 'loi'); $('fileAnhBia').value = ''; return; }
  hienAnh(kq.duongDan);
});
$('btnBoAnhBia').addEventListener('click', () => { if (!$('fileAnhBia').disabled) hienAnh(null); });
// Nút “Bỏ ảnh” bật/tắt theo ô chọn tệp (FormDanhMuc đổi trạng thái ô tệp theo Thêm/Sửa)
new MutationObserver(() => { $('btnBoAnhBia').disabled = $('fileAnhBia').disabled; })
  .observe($('fileAnhBia'), { attributes: true, attributeFilter: ['disabled'] });

$('cboLocTheLoai').addEventListener('change', () => { form.veTaiForm(); form.luoi.trang = 1; form.luoi.tai(); });

// ---------- Bản sách của đầu sách (D55) ----------
let dauSachChon = null;
const cotBan = [
  { ten: 'maBanSach', nhan: 'Mã bản sách' },
  { ten: 'tenSach', nhan: 'Tên sách' },
  { ten: 'tacGia', nhan: 'Tác giả' },
  { ten: 'viTri', nhan: 'Khu vực – vị trí', dinhDang: (v, d) => `${d.maKhuVuc} – ${v}` },
  { ten: 'tinhTrang', nhan: 'Tình trạng', dinhDang: (v) => nhanTrangThai(v, lopTinhTrang[v]) }
];
function nut(nhan, icon, khiBam) {
  const b = document.createElement('button');
  b.type = 'button';
  b.className = 'qt-nut qt-nut--phu qt-nut--nho';
  b.innerHTML = `<i class="bi ${icon}" aria-hidden="true"></i><span></span>`;
  b.querySelector('span').textContent = nhan;
  b.addEventListener('click', (e) => { e.stopPropagation(); khiBam(); });
  return b;
}
const luoiDaGan = new LuoiDuLieu($('dgvBanSachDauSach'), {
  khoa: 'maBanSach', duLieuTinh: [], rong: 'Chưa có bản sách nào gắn vào đầu sách này.',
  cot: [...cotBan, ...(coSua ? [{ ten: 'maBanSach', nhan: 'Thao tác', dinhDang: (v) => nut('Bỏ gắn', 'bi-link-45deg', () => boGan(v)) }] : [])]
});
const luoiChuaGan = coSua ? new LuoiDuLieu($('dgvBanSachChuaGan'), {
  khoa: 'maBanSach', duLieuTinh: [], rong: 'Không có bản sách chưa gắn phù hợp.',
  cot: [...cotBan, { ten: 'maBanSach', nhan: 'Thao tác', dinhDang: (v) => nut('Gắn', 'bi-plus-lg', () => gan(v)) }]
}) : null;

function anBanSach() { dauSachChon = null; $('khuBanSach').hidden = true; }

async function taiBanSach(d) {
  dauSachChon = d;
  $('khuBanSach').hidden = false;
  $('lblDauSachChon').textContent = `${d.maDauSach} – ${d.tenSach}`;
  try {
    luoiDaGan.tc.duLieuTinh = await api.lay(`/api/quan-tri/dau-sach/${encodeURIComponent(d.maDauSach)}/ban-sach`);
    await luoiDaGan.tai();
    if (luoiChuaGan) await timChuaGan(d.tenSach);
  } catch (e) { thongBao(e.message, 'loi'); }
}

async function timChuaGan(macDinh) {
  if (macDinh !== undefined) $('txtTimBanSach').value = macDinh;
  luoiChuaGan.tc.duLieuTinh = await api.lay('/api/quan-tri/dau-sach/ban-sach-chua-gan?tuKhoa=' + encodeURIComponent($('txtTimBanSach').value.trim()));
  await luoiChuaGan.tai();
}

async function gan(maBanSach) {
  if (!dauSachChon) return;
  try {
    await api.them(`/api/quan-tri/dau-sach/${encodeURIComponent(dauSachChon.maDauSach)}/ban-sach`, { maBanSach: [maBanSach] });
    thongBao(`Đã gắn bản sách ${maBanSach} vào đầu sách ${dauSachChon.maDauSach}.`);
    await Promise.all([taiBanSach(dauSachChon), form.luoi.tai({ khoaChon: dauSachChon.maDauSach })]);
  } catch (e) { if (e instanceof LoiApi) thongBao(e.message, 'loi'); else throw e; }
}

async function boGan(maBanSach) {
  if (!dauSachChon) return;
  const ok = await xacNhan({ tieuDe: 'Bỏ gắn bản sách', nutDongY: 'Bỏ gắn',
    noiDung: `Bỏ gắn bản sách ${maBanSach} khỏi đầu sách ${dauSachChon.maDauSach} – ${dauSachChon.tenSach}?` });
  if (!ok) return;
  try {
    await api.xoa(`/api/quan-tri/dau-sach/${encodeURIComponent(dauSachChon.maDauSach)}/ban-sach/${encodeURIComponent(maBanSach)}`);
    thongBao(`Đã bỏ gắn bản sách ${maBanSach}.`);
    await Promise.all([taiBanSach(dauSachChon), form.luoi.tai({ khoaChon: dauSachChon.maDauSach })]);
  } catch (e) { if (e instanceof LoiApi) thongBao(e.message, 'loi'); else throw e; }
}

if (coSua) {
  $('btnTimBanSach').addEventListener('click', () => timChuaGan());
  $('txtTimBanSach').addEventListener('keydown', (e) => { if (e.key === 'Enter') { e.preventDefault(); timChuaGan(); } });
}

await form.batDau();
