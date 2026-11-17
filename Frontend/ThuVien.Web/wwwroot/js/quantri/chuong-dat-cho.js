// Chuông topbar dashboard (file 06 mục 5): số đặt chỗ “Đang giữ” sắp hết hạn (≤ NhacTruocGio giờ). Bấm → Quầy đặt chỗ.
import { api } from '../api.js';

const lnk = document.getElementById('lnkChuongDatCho');
const so = document.getElementById('soDatChoSapHetHan');

async function nap() {
  try {
    const d = await api.lay('/api/quan-tri/dat-cho/tom-tat');
    so.hidden = d.sapHetHan <= 0;
    so.textContent = String(d.sapHetHan);
    lnk.setAttribute('aria-label', `Quầy đặt chỗ – ${d.dangGiu} đang giữ, ${d.sapHetHan} sắp hết hạn trong ${d.nhacTruocGio} giờ`);
    lnk.title = `${d.dangGiu} đặt chỗ đang giữ · ${d.sapHetHan} sắp hết hạn`;
  } catch { /* không chặn trang */ }
}

if (lnk) {
  nap();
  setInterval(() => { if (!document.hidden) nap(); }, 60000);
  document.addEventListener('ulms:dat-cho-thay-doi', nap);
}
