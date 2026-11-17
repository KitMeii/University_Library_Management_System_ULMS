// Mã vạch Code128 (bộ B) vẽ bằng SVG cho vé giữ chỗ (file 06 mục 6, file 13 – “hoặc tự vẽ SVG”, không cần JsBarcode).
// Mỗi ký hiệu gồm 6 phần tử vạch/khoảng (độ rộng 1–4 mô-đun), ký hiệu Stop gồm 7 phần tử.

const MAU = (
  '212222 222122 222221 121223 121322 131222 122213 122312 132212 221213 221312 231212 112232 122132 122231 113222 ' +
  '123122 123221 223211 221132 221231 213212 223112 312131 311222 321122 321221 312212 322112 322211 212123 212321 ' +
  '232121 111323 131123 131321 112313 132113 132311 211313 231113 231311 112133 112331 132131 113123 113321 133121 ' +
  '313121 211331 231131 213113 213311 213131 311123 311321 331121 312113 312311 332111 314111 221411 431111 111224 ' +
  '111422 121124 121421 141122 141221 112214 112412 122114 122411 142112 142211 241211 221114 413111 241112 134111 ' +
  '111242 121142 121241 114212 124112 124211 411212 421112 421211 212141 214121 412121 111143 111341 131141 114113 ' +
  '114311 411113 411311 113141 114131 311141 411131 211412 211214 211232 2331112').split(' ');
const START_B = 104, STOP = 106;
const NS = 'http://www.w3.org/2000/svg';

/** Dãy giá trị Code128B (gồm Start B, dữ liệu, checksum, Stop). Chỉ nhận ký tự ASCII 32–126. */
export function maHoa(chuoi) {
  const giaTri = [...chuoi].map((c) => {
    const m = c.charCodeAt(0);
    if (m < 32 || m > 126) throw new Error('Code128B chỉ mã hóa ký tự ASCII 32–126');
    return m - 32;
  });
  const tong = giaTri.reduce((s, v, i) => s + v * (i + 1), START_B);
  return [START_B, ...giaTri, tong % 103, STOP];
}

/**
 * Tạo phần tử SVG mã vạch.
 * @param {string} chuoi  nội dung (mã đặt chỗ)
 * @param {{cao?: number, nhan?: string}} [tc]
 */
export function veMaVach(chuoi, tc = {}) {
  const cao = tc.cao ?? 56;
  const vung = 10; // vùng yên tĩnh 10 mô-đun mỗi bên
  const dong = maHoa(chuoi).map((v) => MAU[v]).join('');
  const rong = [...dong].reduce((s, d) => s + Number(d), 0) + vung * 2;

  const svg = document.createElementNS(NS, 'svg');
  svg.setAttribute('viewBox', `0 0 ${rong} ${cao}`);
  svg.setAttribute('preserveAspectRatio', 'none');
  svg.setAttribute('class', 'ma-vach');
  svg.setAttribute('role', 'img');
  svg.setAttribute('aria-label', tc.nhan ?? `Mã vạch ${chuoi}`);
  let x = vung;
  [...dong].forEach((d, i) => {
    const w = Number(d);
    if (i % 2 === 0) { // phần tử chẵn là vạch, lẻ là khoảng trắng
      const r = document.createElementNS(NS, 'rect');
      r.setAttribute('x', x); r.setAttribute('y', 0); r.setAttribute('width', w); r.setAttribute('height', cao);
      svg.append(r);
    }
    x += w;
  });
  return svg;
}
