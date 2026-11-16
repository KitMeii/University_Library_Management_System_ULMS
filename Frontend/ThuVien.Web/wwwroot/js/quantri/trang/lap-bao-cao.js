// 01.04.03 Lập Báo cáo tình hình mượn trả sách / 02.05.02 Lập Báo cáo tình hình vi phạm (SPEC 6.4.5–6.4.6):
//  btnXemBaoCao: nhận số liệu tổng hợp của kỳ (TT070/TT096), hiển thị chi tiết báo cáo trong khung xem trước (TT071/TT097);
//  txtNgayLap tự sinh (TT072); btnIn: ghi nhật ký lập/in báo cáo rồi in khung xem trước (TT073/TT098).
import { api, chuoiTruyVan } from '../../api.js';
import { thongBao } from '../hop-thoai.js';
import { ganKy, docKy, baoLoiKy, iso } from '../ky-bao-cao.js';
import { ngayDayDu } from '../../charts/chart-factory.js';

const $ = (id) => document.getElementById(id);
const form = document.querySelector('[data-loai]');
const loai = form.dataset.loai;
const khung = $('khungXemTruoc');
let kyHienTai = null;

$('txtNgayLap').value = ngayDayDu(iso(new Date()));

function datTrangThaiIn(bat) {
  $('btnIn').disabled = !bat;
  $('lnkMoBanIn').hidden = !bat;
}

async function xem() {
  const ky = docKy();
  if (!ky) return;
  datTrangThaiIn(false);
  // Kiểm tra kỳ ở server trước (422 theo trường) để không hiện trang lỗi trong khung xem trước
  try {
    await api.lay(`/api/quan-tri/bao-cao/${loai}` + chuoiTruyVan(ky));
  } catch (e) { baoLoiKy(e); return; }
  const url = `/quan-tri/in/bao-cao-${loai}` + chuoiTruyVan(ky);
  kyHienTai = ky;
  $('khuChuaXem').hidden = true;
  khung.hidden = false;
  await new Promise((resolve) => {
    khung.addEventListener('load', resolve, { once: true });
    khung.src = url;
  });
  $('lnkMoBanIn').href = url + '&tuIn=1';
  datTrangThaiIn(true);
}

$('btnXemBaoCao').addEventListener('click', xem);
['dtpTuNgay', 'dtpDenNgay'].forEach((id) => $(id).addEventListener('change', () => datTrangThaiIn(false)));
$('btnIn').addEventListener('click', async () => {
  if (!kyHienTai) return;
  $('btnIn').disabled = true;
  try {
    const kq = await api.them(`/api/quan-tri/bao-cao/${loai}/lap`, kyHienTai);
    try {
      await khung.contentDocument?.fonts?.ready;
      khung.contentWindow.focus();
      khung.contentWindow.print();
    } catch { /* trình duyệt chặn in – vẫn còn “Mở bản in” */ }
    thongBao(kq.thongBao, 'ok');
  } catch (e) {
    baoLoiKy(e);
  } finally {
    $('btnIn').disabled = false;
  }
});
$('btnDong').addEventListener('click', () => { location.href = '/quan-tri'; });

if (ganKy({ tuNgay: form.dataset.tuNgay, denNgay: form.dataset.denNgay })) await xem();
