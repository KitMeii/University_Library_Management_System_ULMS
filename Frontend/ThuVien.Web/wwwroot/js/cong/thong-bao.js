// Chuông thông báo trên header cổng (file 07 mục 4): số chưa đọc, danh sách 10 thông báo mới nhất, liên kết tới đúng chỗ.
import { api } from '../api.js';
import { dongThongBao } from './thong-bao-dong.js';

const nut = document.getElementById('btnChuongCong');
const so = document.getElementById('soThongBaoChuaDoc');
const ds = document.getElementById('dsThongBaoCong');
const docHet = document.getElementById('btnDocTatCaThongBao');

function capNhatSo(n) {
  so.hidden = n <= 0;
  so.textContent = n > 99 ? '99+' : String(n);
  nut.setAttribute('aria-label', n > 0 ? `Thông báo – ${n} chưa đọc` : 'Thông báo');
}

async function danhDau(t) {
  if (t.daDoc) return;
  try { await api.them(`/api/cong/thong-bao/${t.maThongBao}/da-doc`, {}); } catch { /* không chặn điều hướng */ }
}

function dongTrong(chu) {
  const li = document.createElement('li');
  li.className = 'cong-chuong__trong';
  li.textContent = chu;
  return li;
}

async function nap() {
  try {
    const d = await api.lay('/api/cong/thong-bao?soLuong=10');
    capNhatSo(d.chuaDoc);
    ds.replaceChildren(...(d.ds.length ? d.ds.map((t) => dongThongBao(t, danhDau)) : [dongTrong('Chưa có thông báo nào.')]));
  } catch {
    ds.replaceChildren(dongTrong('Không tải được thông báo.'));
  }
}

// Yêu thích (số lượng) trên header (file 04 mục 1)
const lnkYeuThich = document.getElementById('lnkYeuThich');
const soYeuThich = document.getElementById('soYeuThich');
function datSoYeuThich(n) {
  if (!lnkYeuThich) return;
  soYeuThich.hidden = n <= 0;
  soYeuThich.textContent = n > 99 ? '99+' : String(n);
  lnkYeuThich.setAttribute('aria-label', n > 0 ? `Sách yêu thích – ${n} cuốn` : 'Sách yêu thích');
}
if (lnkYeuThich) {
  const napYeuThich = () => api.lay('/api/cong/yeu-thich').then((ds) => datSoYeuThich(ds.length)).catch(() => {});
  napYeuThich();
  document.addEventListener('ulms:yeu-thich-doi', (e) => (typeof e.detail?.so === 'number' ? datSoYeuThich(e.detail.so) : napYeuThich()));
}

if (nut) {
  nap();
  nut.addEventListener('show.bs.dropdown', nap);
  docHet.addEventListener('click', async () => {
    await api.them('/api/cong/thong-bao/da-doc-tat-ca', {});
    await nap();
  });
  // Đặt chỗ thành công trong Book Preview → cập nhật số; làm mới định kỳ 2 phút
  document.addEventListener('ulms:thong-bao-moi', nap);
  setInterval(() => { if (!document.hidden) nap(); }, 120000);
}
