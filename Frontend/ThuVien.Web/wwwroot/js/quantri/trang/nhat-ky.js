// SPEC 6.3.4 Theo dõi nhật ký hệ thống (frmNhatKy, 04.00.03 / 04.00.08).
//  (Load): mở thẻ theo chức năng chọn trên menu; dtpTuNgay = ngày đầu tháng, dtpDenNgay = ngày hiện tại; cboNguoiDung = “Tất cả”
//  btnLoc: kiểm tra dtpTuNgay ≤ dtpDenNgay; lọc theo ma_nd và khoảng thời gian; hiển thị kèm ten_nd và số lượt
import { api } from '../../api.js';
import { LuoiDuLieu } from '../luoi.js';
import { thongBao } from '../hop-thoai.js';
import { layNgay, datNgay } from '../../chon-ngay.js';

const $ = (id) => document.getElementById(id);
const dangNhap = $('frmNhatKy').dataset.the === 'dang-nhap';

const p2 = (n) => String(n).padStart(2, '0');
const iso = (d) => `${d.getFullYear()}-${p2(d.getMonth() + 1)}-${p2(d.getDate())}`;
const homNay = new Date();
datNgay($('dtpTuNgay'), iso(new Date(homNay.getFullYear(), homNay.getMonth(), 1)));
datNgay($('dtpDenNgay'), iso(homNay));

const ngayGio = (v) => {
  if (!v) return '';
  const [d, t] = String(v).split('T');
  const [y, m, n] = d.split('-');
  return `${n}/${m}/${y} ${t.slice(0, 5)}`;
};

const nguoiDung = await api.lay('/api/quan-tri/nhat-ky/nguoi-dung');
nguoiDung.forEach((n) => $('cboNguoiDung').add(new Option(`${n.tenDangNhap} – ${n.tenNd}`, n.maNd)));

const luoi = new LuoiDuLieu($(dangNhap ? 'dgvDangNhap' : 'dgvHoatDong'), {
  url: '/api/quan-tri/nhat-ky/' + (dangNhap ? 'dang-nhap' : 'hoat-dong'),
  khoa: dangNhap ? 'maDangNhap' : 'maHoatDong',
  coTrang: 20,
  rong: 'Không có dữ liệu nhật ký trong khoảng thời gian đã chọn.',
  thamSoThem: () => ({ tuNgay: layNgay($('dtpTuNgay')), denNgay: layNgay($('dtpDenNgay')), maNd: $('cboNguoiDung').value }),
  cot: dangNhap
    ? [
        { ten: 'maDangNhap', nhan: 'Mã lượt đăng nhập' },
        { ten: 'tenDangNhap', nhan: 'Tên đăng nhập', dinhDang: (v, d) => `${v} – ${d.tenNd}` },
        { ten: 'tgDangNhap', nhan: 'Thời gian đăng nhập', sapXep: 'TgDangNhap', dinhDang: ngayGio },
        { ten: 'tgDangXuat', nhan: 'Thời gian đăng xuất', sapXep: 'TgDangXuat', dinhDang: (v) => v ? ngayGio(v) : '—' }
      ]
    : [
        { ten: 'maHoatDong', nhan: 'Mã hoạt động' },
        { ten: 'tenDangNhap', nhan: 'Người thực hiện', dinhDang: (v, d) => `${v} – ${d.tenNd}` },
        { ten: 'tgThucHien', nhan: 'Thời gian thực hiện', sapXep: 'TgThucHien', dinhDang: ngayGio },
        { ten: 'noiDung', nhan: 'Nội dung thao tác' }
      ]
});

async function loc() {
  const tu = layNgay($('dtpTuNgay')), den = layNgay($('dtpDenNgay'));
  if (!tu || !den || tu > den) {
    thongBao('Từ ngày phải nhỏ hơn hoặc bằng đến ngày.', 'loi');
    $('dtpTuNgay').setAttribute('aria-invalid', 'true');
    return;
  }
  $('dtpTuNgay').removeAttribute('aria-invalid');
  luoi.trang = 1;
  await luoi.tai();
  $('soLuot').textContent = luoi.tongSo.toLocaleString('vi-VN');
  if (luoi.tongSo === 0) thongBao('Không có dữ liệu nhật ký phù hợp điều kiện lọc.', 'tin');
}

$('btnLoc').addEventListener('click', loc);
$('btnDong').addEventListener('click', () => { location.href = '/quan-tri'; });
await luoi.tai();
$('soLuot').textContent = luoi.tongSo.toLocaleString('vi-VN');
