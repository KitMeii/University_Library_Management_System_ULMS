// SPEC 6.3.3 Cấp quyền cho tài khoản (frmPhanQuyen, 04.00.02).
//  (Load): nạp NHOMNGUOIDUNG lên cboNhom, QUYEN lên dgvQuyen (cột chọn); btnLuu, btnHuy → False
//  cboNhom: đánh dấu các ma_quyen nhóm đang có; btnLuu, btnHuy → False
//  dgvQuyen đánh dấu/bỏ: ghi nhận tạm (quyền quản trị của N4 không cho bỏ); btnLuu, btnHuy → True; cboNhom → False
//  btnLuu: hỏi xác nhận → thêm/xóa PHANQUYEN → ghi nhật ký → btnLuu, btnHuy → False; cboNhom → True
//  btnHuy: nạp lại quyền hiện có của nhóm
import { api } from '../../api.js';
import { TrangThaiForm } from '../../form-state.js';
import { LuoiDuLieu } from '../luoi.js';
import { thongBao, xacNhan } from '../hop-thoai.js';

const $ = (id) => document.getElementById(id);
const tt = new TrangThaiForm({
  TaiForm: { bat: ['cboNhom'], tat: ['btnLuu', 'btnHuy'] },
  DaThayDoi: { bat: ['btnLuu', 'btnHuy'], tat: ['cboNhom'] }
});

let goc = new Set();   // quyền đang có trong PHANQUYEN
let chon = new Set();  // quyền đang đánh dấu trên lưới
let tatCa = [];

const luoi = new LuoiDuLieu($('dgvQuyen'), {
  khoa: 'maQuyen', duLieuTinh: [], rong: 'Không có quyền phù hợp bộ lọc.',
  cot: [
    { ten: 'chon', nhan: 'Chọn', dinhDang: (_, d) => oChon(d) },
    { ten: 'maQuyen', nhan: 'Mã quyền' },
    { ten: 'tenQuyen', nhan: 'Tên quyền' },
    { ten: 'loai', nhan: 'Loại' }
  ]
});

function oChon(d) {
  const o = document.createElement('input');
  o.type = 'checkbox';
  o.className = 'form-check-input';
  o.checked = chon.has(d.maQuyen);
  o.disabled = d.khoaBo;
  o.setAttribute('aria-label', `Cấp quyền ${d.maQuyen} – ${d.tenQuyen}`);
  if (d.khoaBo) o.title = 'Quyền quản trị của nhóm N4 không được bỏ';
  o.addEventListener('click', (e) => e.stopPropagation());
  o.addEventListener('change', () => {
    if (o.checked) chon.add(d.maQuyen); else chon.delete(d.maQuyen);
    capNhatTrangThai();
  });
  return o;
}

function capNhatTrangThai() {
  const doi = chon.size !== goc.size || [...chon].some((m) => !goc.has(m));
  tt.chuyen(doi ? 'DaThayDoi' : 'TaiForm');
  const them = [...chon].filter((m) => !goc.has(m)).length;
  const bo = [...goc].filter((m) => !chon.has(m)).length;
  $('tomTatQuyen').textContent = `Nhóm ${$('cboNhom').value} đang có ${goc.size}/${tatCa.length} quyền.` +
    (doi ? ` Thay đổi chưa lưu: thêm ${them}, thu hồi ${bo}.` : '');
}

function veLuoi() {
  const k = $('txtLocQuyen').value.trim().toLowerCase();
  const loai = $('cboLoaiQuyen').value;
  luoi.tc.duLieuTinh = tatCa.filter((q) => (!loai || q.loai === loai) &&
    (!k || q.maQuyen.toLowerCase().includes(k) || q.tenQuyen.toLowerCase().includes(k)));
  luoi.tai();
}

async function napNhom() {
  tatCa = await api.lay('/api/quan-tri/phan-quyen/' + encodeURIComponent($('cboNhom').value));
  goc = new Set(tatCa.filter((q) => q.coQuyen).map((q) => q.maQuyen));
  chon = new Set(goc);
  const cacLoai = [...new Set(tatCa.map((q) => q.loai))];
  if ($('cboLoaiQuyen').options.length === 1) cacLoai.forEach((l) => $('cboLoaiQuyen').add(new Option(l, l)));
  veLuoi();
  capNhatTrangThai();
}

const nhom = await api.lay('/api/quan-tri/nhom-nguoi-dung');
$('cboNhom').replaceChildren(...nhom.map((n) => new Option(`${n.maNhom} – ${n.tenNhom}`, n.maNhom)));
$('cboNhom').addEventListener('change', napNhom);
$('txtLocQuyen').addEventListener('input', veLuoi);
$('cboLoaiQuyen').addEventListener('change', veLuoi);
$('btnHuy').addEventListener('click', napNhom);
$('btnDong').addEventListener('click', () => { location.href = '/quan-tri'; });
$('btnLuu').addEventListener('click', async () => {
  const them = [...chon].filter((m) => !goc.has(m));
  const bo = [...goc].filter((m) => !chon.has(m));
  const ok = await xacNhan({ tieuDe: 'Lưu phân quyền', nutDongY: 'Lưu',
    noiDung: `Nhóm ${$('cboNhom').value}: cấp thêm ${them.length} quyền${them.length ? ' (' + them.join(', ') + ')' : ''}; ` +
      `thu hồi ${bo.length} quyền${bo.length ? ' (' + bo.join(', ') + ')' : ''}. Quyền mới có hiệu lực từ lần đăng nhập tiếp theo.` });
  if (!ok) return;
  try {
    const kq = await api.sua('/api/quan-tri/phan-quyen/' + encodeURIComponent($('cboNhom').value), { maQuyen: [...chon] });
    thongBao(`Đã lưu phân quyền nhóm ${$('cboNhom').value}: thêm ${kq.soThem}, thu hồi ${kq.soBo}.`);
    await napNhom();
  } catch (e) { thongBao(e.message, 'loi'); }
});

tt.chuyen('TaiForm');
await napNhom();
