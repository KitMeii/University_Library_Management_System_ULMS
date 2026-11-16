// SPEC 6.3.5 Sao lưu và phục hồi dữ liệu (frmSaoLuu, 04.00.09).
//  (Load): chọn mặc định rdoSaoLuu; txtDuongDan ReadOnly (chỉ chọn qua btnChon); btnThucHien → False
//  rdoSaoLuu, rdoPhucHoi: xóa txtDuongDan; btnChon đổi thành chọn thư mục / chọn tệp; btnThucHien → False
//  btnChon: chọn thư mục hoặc tệp; hiển thị đường dẫn; btnThucHien → True
//  btnThucHien: sao lưu | phục hồi (hỏi xác nhận, tự sao lưu hiện tại); hiển thị tiến trình (%); btnThucHien → False
import { api } from '../../api.js';
import { thongBao, xacNhan } from '../hop-thoai.js';

const $ = (id) => document.getElementById(id);
const phucHoi = () => $('rdoPhucHoi').checked;
let duongDanChon = null;

function doiThaoTac() {
  $('txtDuongDan').value = '';
  duongDanChon = null;
  $('btnThucHien').disabled = true;
  $('btnChon').querySelector('span').textContent = phucHoi() ? 'Chọn tệp sao lưu…' : 'Chọn thư mục…';
  $('nhanDuongDan').textContent = phucHoi() ? 'Tệp sao lưu dùng để phục hồi' : 'Thư mục lưu tệp sao lưu';
  $('canhBaoPhien').hidden = true;
}
$('rdoSaoLuu').addEventListener('change', doiThaoTac);
$('rdoPhucHoi').addEventListener('change', doiThaoTac);

// ---------- btnChon: duyệt thư mục sao lưu của máy chủ ----------
const modal = new bootstrap.Modal($('modalChon'));
let thuMucDangXem = '';
let mucChon = null;

const kichThuoc = (b) => b > 1048576 ? (b / 1048576).toFixed(1) + ' MB' : Math.ceil(b / 1024) + ' KB';
const ngayGio = (v) => v ? new Date(v).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' }) : '';

async function duyet(tuongDoi) {
  thuMucDangXem = tuongDoi;
  mucChon = phucHoi() ? null : { duongDan: tuongDoi };
  $('btnChonXong').disabled = phucHoi();
  const ds = await api.lay('/api/quan-tri/sao-luu/duyet?duongDan=' + encodeURIComponent(tuongDoi));
  const nav = $('duongDanDuyet');
  nav.replaceChildren();
  const phan = tuongDoi ? tuongDoi.split('/') : [];
  [['Thư mục sao lưu', ''], ...phan.map((p, i) => [p, phan.slice(0, i + 1).join('/')])].forEach(([ten, dd], i, a) => {
    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'btn btn-link btn-sm p-0';
    b.textContent = ten;
    if (i === a.length - 1) b.setAttribute('aria-current', 'location');
    b.addEventListener('click', () => duyet(dd));
    nav.append(b);
    if (i < a.length - 1) nav.append(' / ');
  });
  const ul = $('dsDuyet');
  ul.replaceChildren();
  const hien = ds.filter((m) => m.laThuMuc || phucHoi());
  if (!hien.length) {
    const li = document.createElement('li');
    li.className = 'qt-duyet__rong';
    li.textContent = phucHoi() ? 'Thư mục này không có tệp sao lưu (.bak).' : 'Không có thư mục con. Bấm “Chọn” để dùng thư mục hiện tại.';
    ul.append(li);
  }
  for (const m of hien) {
    const li = document.createElement('li');
    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'qt-duyet__muc';
    b.setAttribute('role', 'option');
    b.setAttribute('aria-selected', 'false');
    b.innerHTML = `<i class="bi ${m.laThuMuc ? 'bi-folder2' : 'bi-file-earmark-zip'}" aria-hidden="true"></i><span></span><small></small>`;
    b.querySelector('span').textContent = m.ten;
    b.querySelector('small').textContent = m.laThuMuc ? '' : `${kichThuoc(m.kichThuoc)} · ${ngayGio(m.ngaySua)}`;
    b.addEventListener('click', () => {
      if (m.laThuMuc) { duyet(m.duongDan); return; }
      ul.querySelectorAll('[aria-selected="true"]').forEach((x) => x.setAttribute('aria-selected', 'false'));
      b.setAttribute('aria-selected', 'true');
      mucChon = m;
      $('btnChonXong').disabled = false;
    });
    li.append(b);
    ul.append(li);
  }
}

$('btnChon').addEventListener('click', async () => {
  $('modalChonTieuDe').textContent = phucHoi() ? 'Chọn tệp sao lưu' : 'Chọn thư mục';
  try { await duyet(''); modal.show(); } catch (e) { thongBao(e.message, 'loi'); }
});

$('btnChonXong').addEventListener('click', async () => {
  if (!mucChon) return;
  duongDanChon = mucChon.duongDan;
  $('txtDuongDan').value = '/' + (duongDanChon || '');
  $('btnThucHien').disabled = false;
  modal.hide();
  if (phucHoi()) {
    // Tiền điều kiện: các tài khoản khác đã đăng xuất
    const phien = await api.lay('/api/quan-tri/sao-luu/phien-dang-mo');
    const cb = $('canhBaoPhien');
    cb.hidden = phien.length === 0;
    cb.textContent = phien.length
      ? `Có ${phien.length} phiên đăng nhập khác chưa đăng xuất (${phien.map((p) => p.tenDangNhap).join(', ')}). Nên đề nghị họ đăng xuất trước khi phục hồi.`
      : '';
  }
});

// ---------- btnThucHien ----------
function hienTienTrinh(p, moTa) {
  $('khuTienTrinh').hidden = false;
  $('phanTram').textContent = p + '%';
  $('moTaTienTrinh').textContent = moTa;
  $('thanhTienTrinh').setAttribute('aria-valuenow', String(p));
  $('thanhTienTrinh').firstElementChild.style.transform = `scaleX(${p / 100})`;
}

async function theoDoi(ma) {
  for (;;) {
    const t = await api.lay('/api/quan-tri/sao-luu/tien-trinh/' + ma);
    hienTienTrinh(t.phanTram, t.thongBao);
    if (t.xong) return t;
    await new Promise((r) => setTimeout(r, 600)); // hỏi trạng thái công việc nền (không phải hoạt ảnh)
  }
}

$('btnThucHien').addEventListener('click', async () => {
  const laPhucHoi = phucHoi();
  if (laPhucHoi) {
    const ok1 = await xacNhan({ tieuDe: 'Phục hồi dữ liệu', nguyHiem: true, nutDongY: 'Tiếp tục',
      noiDung: `Phục hồi sẽ GHI ĐÈ TOÀN BỘ dữ liệu hiện tại bằng tệp ${$('txtDuongDan').value}. Hệ thống sẽ tự sao lưu dữ liệu hiện tại trước khi phục hồi.` });
    if (!ok1) return;
    const ok2 = await xacNhan({ tieuDe: 'Xác nhận lần 2', nguyHiem: true, nutDongY: 'Phục hồi ngay',
      noiDung: 'Bạn chắc chắn muốn phục hồi? Mọi người dùng sẽ phải đăng nhập lại sau khi phục hồi.' });
    if (!ok2) return;
  }
  $('btnThucHien').disabled = true;
  $('btnChon').disabled = true;
  hienTienTrinh(0, 'Đang chuẩn bị…');
  try {
    const { ma } = await api.them('/api/quan-tri/sao-luu/' + (laPhucHoi ? 'phuc-hoi' : 'sao-luu'), { duongDan: duongDanChon });
    const t = await theoDoi(ma);
    thongBao(t.thongBao, t.thanhCong ? 'ok' : 'loi');
    if (t.thanhCong && laPhucHoi) {
      // Yêu cầu đăng nhập lại
      const f = document.createElement('form');
      f.method = 'post';
      f.action = '/quan-tri/dang-xuat';
      const i = document.createElement('input');
      i.type = 'hidden'; i.name = '__RequestVerificationToken';
      i.value = document.querySelector('meta[name="csrf-token"]').content;
      f.append(i);
      document.body.append(f);
      setTimeout(() => f.submit(), 1500);
    }
  } catch (e) {
    thongBao(e.message, 'loi');
  } finally {
    $('btnChon').disabled = false;
    $('btnThucHien').disabled = true; // SPEC: btnThucHien → False sau khi thực hiện
  }
});
$('btnDong').addEventListener('click', () => { location.href = '/quan-tri'; });
doiThaoTac();
