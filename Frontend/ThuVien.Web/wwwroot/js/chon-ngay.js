// DatePicker (file 03 mục D): hiển thị dd/MM/yyyy (file 01 mục 5), CHỌN ngày trên lịch – không bắt gõ tay ngày (SPEC 6.3.4).
// Nâng cấp mọi <input type="date" class="qt-o">: giữ nguyên id (control SPEC), đổi sang ô chữ chỉ chọn qua lịch.
// Giá trị trao đổi với API dạng yyyy-MM-dd qua layNgay()/datNgay().

const THANG = ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6', 'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'];
const THU = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
const p2 = (n) => String(n).padStart(2, '0');
const isoTu = (d) => `${d.getFullYear()}-${p2(d.getMonth() + 1)}-${p2(d.getDate())}`;

/** Chuỗi hiển thị/nhập (dd/MM/yyyy hoặc yyyy-MM-dd) → yyyy-MM-dd; '' nếu trống/không hợp lệ. */
export function chuanHoa(v) {
  v = (v || '').trim();
  let m = /^(\d{4})-(\d{2})-(\d{2})/.exec(v);
  if (m) return `${m[1]}-${m[2]}-${m[3]}`;
  m = /^(\d{1,2})\/(\d{1,2})\/(\d{4})$/.exec(v);
  if (m) return `${m[3]}-${p2(m[2])}-${p2(m[1])}`;
  return '';
}
const hienThi = (iso) => { const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(iso || ''); return m ? `${m[3]}/${m[2]}/${m[1]}` : ''; };

export const layNgay = (el) => chuanHoa(el.value);
export const datNgay = (el, iso) => { el.value = hienThi(chuanHoa(iso)); };
export const laOChonNgay = (el) => el?.dataset?.chonNgay !== undefined;

let dangMo = null;

export function ganChonNgay(el) {
  if (laOChonNgay(el)) return;
  const giaTri = el.value;
  el.type = 'text';
  el.dataset.chonNgay = '';
  el.inputMode = 'none';
  el.autocomplete = 'off';
  el.placeholder = el.placeholder || 'dd/mm/yyyy';
  el.setAttribute('aria-haspopup', 'dialog');
  el.value = hienThi(chuanHoa(giaTri));
  el.classList.add('qt-o--ngay');

  const moDuoc = () => !el.readOnly && !el.disabled;
  el.addEventListener('click', () => { if (moDuoc()) mo(el); });
  el.addEventListener('keydown', (e) => {
    if (['Enter', ' ', 'ArrowDown'].includes(e.key) && moDuoc()) { e.preventDefault(); mo(el); return; }
    if ((e.key === 'Delete' || e.key === 'Backspace') && moDuoc()) { e.preventDefault(); el.value = ''; el.dispatchEvent(new Event('change', { bubbles: true })); return; }
    if (e.key.length === 1 && !e.ctrlKey && !e.metaKey) e.preventDefault(); // không gõ tay
  });
  el.addEventListener('paste', (e) => e.preventDefault());
}

export function ganTatCa(goc = document) {
  goc.querySelectorAll('input[type="date"].qt-o').forEach(ganChonNgay);
}

function dong() {
  if (!dangMo) return;
  dangMo.hop.remove();
  document.removeEventListener('pointerdown', dangMo.ngoai, true);
  const el = dangMo.el;
  el.setAttribute('aria-expanded', 'false');
  dangMo = null;
  el.focus();
}

function mo(el) {
  dong();
  const chon = layNgay(el);
  let xem = chon ? new Date(chon + 'T00:00:00') : new Date();
  const hop = document.createElement('div');
  hop.className = 'qt-lich';
  hop.setAttribute('role', 'dialog');
  hop.setAttribute('aria-label', 'Chọn ngày');
  el.setAttribute('aria-expanded', 'true');

  const ve = (tapTrung) => {
    const y = xem.getFullYear(), m = xem.getMonth();
    hop.innerHTML = `
      <div class="qt-lich__dau">
        <button type="button" class="qt-lich__nut" data-di="-1" aria-label="Tháng trước"><i class="bi bi-chevron-left" aria-hidden="true"></i></button>
        <span class="qt-lich__thang" aria-live="polite"></span>
        <select class="qt-lich__nam" aria-label="Năm"></select>
        <button type="button" class="qt-lich__nut" data-di="1" aria-label="Tháng sau"><i class="bi bi-chevron-right" aria-hidden="true"></i></button>
      </div>
      <div class="qt-lich__luoi" role="grid"></div>
      <div class="qt-lich__chan"><button type="button" class="qt-lich__nut-chu" data-hom-nay>Hôm nay</button><button type="button" class="qt-lich__nut-chu" data-xoa>Xóa</button></div>`;
    hop.querySelector('.qt-lich__thang').textContent = THANG[m];
    const nam = hop.querySelector('.qt-lich__nam');
    for (let n = 1940; n <= new Date().getFullYear() + 15; n++) nam.add(new Option(n, n, false, n === y));
    nam.addEventListener('change', () => { xem = new Date(+nam.value, m, 1); ve(); });
    hop.querySelectorAll('[data-di]').forEach((b) => b.addEventListener('click', () => { xem = new Date(y, m + +b.dataset.di, 1); ve(); }));
    hop.querySelector('[data-hom-nay]').addEventListener('click', () => chonNgay(isoTu(new Date())));
    hop.querySelector('[data-xoa]').addEventListener('click', () => chonNgay(''));

    const luoi = hop.querySelector('.qt-lich__luoi');
    THU.forEach((t) => { const s = document.createElement('span'); s.className = 'qt-lich__thu'; s.textContent = t; luoi.append(s); });
    const dau = new Date(y, m, 1);
    const lech = (dau.getDay() + 6) % 7;
    for (let i = 0; i < lech; i++) luoi.append(document.createElement('span'));
    const soNgay = new Date(y, m + 1, 0).getDate();
    const homNay = isoTu(new Date());
    for (let d = 1; d <= soNgay; d++) {
      const iso = `${y}-${p2(m + 1)}-${p2(d)}`;
      const b = document.createElement('button');
      b.type = 'button';
      b.className = 'qt-lich__ngay';
      b.textContent = d;
      b.dataset.iso = iso;
      b.setAttribute('aria-label', `${d} ${THANG[m].toLowerCase()} ${y}`);
      if (iso === chon) b.setAttribute('aria-pressed', 'true');
      if (iso === homNay) b.classList.add('hom-nay');
      b.tabIndex = -1;
      b.addEventListener('click', () => chonNgay(iso));
      luoi.append(b);
    }
    const dich = luoi.querySelector('[aria-pressed="true"]') || luoi.querySelector('.hom-nay') || luoi.querySelector('.qt-lich__ngay');
    dich.tabIndex = 0;
    if (tapTrung !== false) dich.focus();
  };

  const chonNgay = (iso) => {
    datNgay(el, iso);
    el.dispatchEvent(new Event('change', { bubbles: true }));
    dong();
  };

  hop.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') { e.preventDefault(); dong(); return; }
    const b = e.target.closest('.qt-lich__ngay');
    if (!b) return;
    const buoc = { ArrowLeft: -1, ArrowRight: 1, ArrowUp: -7, ArrowDown: 7 }[e.key];
    if (buoc) {
      e.preventDefault();
      const d = new Date(b.dataset.iso + 'T00:00:00');
      d.setDate(d.getDate() + buoc);
      if (d.getMonth() !== xem.getMonth()) { xem = new Date(d.getFullYear(), d.getMonth(), 1); ve(false); }
      const moi = hop.querySelector(`[data-iso="${isoTu(d)}"]`);
      hop.querySelectorAll('.qt-lich__ngay').forEach((x) => { x.tabIndex = -1; });
      moi.tabIndex = 0;
      moi.focus();
    }
  });

  el.insertAdjacentElement('afterend', hop);
  ve();
  const ngoai = (e) => { if (!hop.contains(e.target) && e.target !== el) dong(); };
  document.addEventListener('pointerdown', ngoai, true);
  dangMo = { el, hop, ngoai };
}
