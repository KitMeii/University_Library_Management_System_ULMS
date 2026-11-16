// SPEC 6.1.1 Danh mục sách (frmDanhMucSach) – kèm 03.01.02, 03.01.04, 03.02.03, 03.03.02, 03.04.01.
import { api } from '../../api.js';
import { FormDanhMuc, nhanTrangThai, dinhDangNgay } from '../danh-muc.js';
import { LuoiDuLieu } from '../luoi.js';
import { thongBao, xacNhan } from '../hop-thoai.js';

const $ = (id) => document.getElementById(id);
const lopTinhTrang = { 'Sẵn sàng': 'qt-nhan-tt--ok', 'Đang được mượn': 'qt-nhan-tt--tin', 'Mất': 'qt-nhan-tt--loi', 'Hư hỏng': 'qt-nhan-tt--canh-bao' };

// cboKhuVuc: chọn từ KHUVUC (phải tồn tại trong KHUVUC)
const kv = await api.lay('/api/quan-tri/khu-vuc?coTrang=200&sapXep=MaKhuVuc');
$('cboKhuVuc').replaceChildren(...kv.duLieu.map((k) => new Option(`${k.maKhuVuc} – ${k.viTri}`, k.maKhuVuc)));

const form = new FormDanhMuc({
  api: '/api/quan-tri/sach',
  khoa: 'maBanSach',
  doiTuong: 'bản sách',
  moTa: (d) => `${d.maBanSach} – ${d.tenSach}`,
  truong: [
    { id: 'txtMaBanSach', ten: 'maBanSach', khoa: true },
    { id: 'txtTenSach', ten: 'tenSach' },
    { id: 'txtTacGia', ten: 'tacGia' },
    { id: 'cboKhuVuc', ten: 'maKhuVuc', macDinh: () => kv.duLieu[0]?.maKhuVuc ?? '' },
    { id: 'cboTinhTrang', ten: 'tinhTrang', macDinh: () => 'Sẵn sàng' } // btnThem: tinhtrang mặc định “Sẵn sàng”
  ],
  luoi: {
    id: 'dgvSach', sapXep: 'MaBanSach', coTrang: 12,
    thamSoThem: () => ({ tinhTrang: $('cboLocTinhTrang').value }),
    cot: [
      { ten: 'maBanSach', nhan: 'Mã bản sách', sapXep: 'MaBanSach' },
      { ten: 'tenSach', nhan: 'Tên sách', sapXep: 'TenSach' },
      { ten: 'tacGia', nhan: 'Tác giả', sapXep: 'TacGia' },
      { ten: 'maKhuVuc', nhan: 'Khu vực – vị trí', sapXep: 'MaKhuVuc', dinhDang: (v, d) => `${v} – ${d.viTri}` },
      { ten: 'tinhTrang', nhan: 'Tình trạng', sapXep: 'TinhTrang', dinhDang: (v) => nhanTrangThai(v, lopTinhTrang[v]) }
    ]
  },
  khiVeTaiForm: () => { taiThongKe(); }
});

// 03.04.01: cboLocTinhTrang → hiển thị bản sách theo tình trạng và Số sách = COUNT(ma_bansach) theo tinhtrang
async function taiThongKe() {
  const tk = await api.lay('/api/quan-tri/sach/thong-ke-tinh-trang');
  const o = $('thongKeTinhTrang');
  const tong = tk.reduce((s, x) => s + x.soSach, 0);
  o.replaceChildren(...[{ tinhTrang: 'Tổng số', soSach: tong }, ...tk].map((x) => {
    const d = document.createElement('span');
    d.className = 'qt-thong-ke-nho__o';
    d.innerHTML = '<span></span><strong class="so-lieu"></strong>';
    d.querySelector('span').textContent = x.tinhTrang;
    d.querySelector('strong').textContent = x.soSach.toLocaleString('vi-VN');
    return d;
  }));
}
$('cboLocTinhTrang').addEventListener('change', () => { form.veTaiForm(); form.luoi.trang = 1; form.luoi.tai(); });

// btnTimKiem 03.01.02: ghi chú loại bản sách đang mượn
const timKiemGoc = form.timKiem.bind(form);
form.timKiem = async () => { $('ghiChuTimKiem').hidden = !$('txtTimKiem').value.trim(); await timKiemGoc(); };

await form.batDau();
await taiThongKe();

// ---------- dgvChuyenDen, btnCapNhatTT (03.01.04, 03.02.03, 03.03.02) ----------
const khuChuyenDen = $('dgvChuyenDen');
let luoiChuyenDen = null;
async function taiChuyenDen() {
  const ds = await api.lay('/api/quan-tri/sach/chuyen-den');
  const loc = $('frmDanhMucSach').dataset.tienTrinh;
  const duLieu = ['03.01.04', '03.02.03', '03.03.02'].includes(loc) ? ds.filter((x) => x.tienTrinh === loc) : ds;
  luoiChuyenDen.tc.duLieuTinh = duLieu.map((x, i) => ({ ...x, khoa: x.maBanSach + '|' + x.loai + '|' + i }));
  luoiChuyenDen.tc.rong = loc && loc !== '03.01.02' && loc !== '03.04.01'
    ? `Hiện không có thông tin chuyển đến cho tiến trình ${loc}.` : 'Hiện không có thông tin do bộ phận khác chuyển đến.';
  await luoiChuyenDen.tai();
  $('btnCapNhatTT').disabled = true; // chỉ bật khi chọn 1 dòng ở dgvChuyenDen
}
if (khuChuyenDen) {
  luoiChuyenDen = new LuoiDuLieu(khuChuyenDen, {
    khoa: 'khoa', duLieuTinh: [],
    cot: [
      { ten: 'tienTrinh', nhan: 'Tiến trình' },
      { ten: 'loai', nhan: 'Thông tin chuyển đến' },
      { ten: 'chungTu', nhan: 'Chứng từ' },
      { ten: 'ngay', nhan: 'Ngày', dinhDang: dinhDangNgay },
      { ten: 'maBanSach', nhan: 'Mã bản sách' },
      { ten: 'tenSach', nhan: 'Tên sách' },
      { ten: 'tinhTrangHienTai', nhan: 'Tình trạng hiện tại', dinhDang: (v) => nhanTrangThai(v, lopTinhTrang[v]) },
      { ten: 'tinhTrangDeXuat', nhan: 'Tình trạng mới đề xuất', dinhDang: (v) => nhanTrangThai(v, lopTinhTrang[v]) }
    ],
    // Chọn dòng sách chuyển đến → nạp bản sách, đề xuất tình trạng mới; btnCapNhatTT → Enable = True
    khiChon: (d) => {
      $('txtMaBanSach').value = d.maBanSach;
      $('txtTenSach').value = d.tenSach;
      $('btnCapNhatTT').disabled = false;
    }
  });
  $('btnCapNhatTT').addEventListener('click', async () => {
    const d = luoiChuyenDen.chon;
    if (!d) return;
    const ok = await xacNhan({ tieuDe: 'Cập nhật tình trạng sách', nutDongY: 'Cập nhật',
      noiDung: `Cập nhật bản sách ${d.maBanSach} từ “${d.tinhTrangHienTai}” sang “${d.tinhTrangDeXuat}” (${d.tienTrinh})?` });
    if (!ok) return;
    try {
      await api.them(`/api/quan-tri/sach/${encodeURIComponent(d.maBanSach)}/cap-nhat-tinh-trang`, { loai: d.loai });
      thongBao(`Đã cập nhật tình trạng bản sách ${d.maBanSach}: ${d.tinhTrangDeXuat}.`);
      form.veTaiForm();
      await Promise.all([form.luoi.tai(), taiChuyenDen()]);
    } catch (e) { thongBao(e.message, 'loi'); }
  });
  await taiChuyenDen();
}

// Mở từ menu theo tiến trình
const tt = $('frmDanhMucSach').dataset.tienTrinh;
if (tt === '03.01.02') $('txtTimKiem').focus();
else if (tt === '03.04.01') $('cboLocTinhTrang').focus();
else if (tt && khuChuyenDen) khuChuyenDen.scrollIntoView({ block: 'start' });
