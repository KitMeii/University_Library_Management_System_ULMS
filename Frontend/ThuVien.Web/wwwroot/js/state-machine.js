// State machine dùng chung cho các khu vực tương tác lớn (file 10 mục 2): mọi chuyển trạng thái đi qua MỘT hàm chuyen(suKien)
// có guard; hành động (animation, tải dữ liệu) chỉ khởi động từ đây. Sự kiện đến khi đang bận: giữ TỐI ĐA 1 lệnh chờ
// (“yêu cầu mới nhất thắng”), lệnh chờ chạy khi máy về trạng thái rảnh.

/**
 * @typedef {{ den: string, khi?: (duLieu:any) => boolean, lam?: (duLieu:any, may:MayTrangThai) => (void|Promise<void>), xong?: string }} BuocChuyen
 *   den: trạng thái ngay khi nhận sự kiện; lam: hành động; xong: trạng thái sau khi `lam` kết thúc (bất đồng bộ).
 */
export class MayTrangThai {
  /**
   * @param {{ ten: string, batDau: string, ranh: string[], bang: Record<string, Record<string, BuocChuyen>>, khiDoi?: (tu:string, den:string, suKien:string) => void }} cfg
   *   ranh: các trạng thái “rảnh” – được chạy lệnh chờ; bang[trangThai][suKien] = bước chuyển
   */
  constructor(cfg) {
    this.cfg = cfg;
    this.hienTai = cfg.batDau;
    this.cho = null; // { suKien, duLieu } – chỉ một lệnh chờ
    this.phien = 0;
  }

  get ranh() { return this.cfg.ranh.includes(this.hienTai); }

  /** Có xử lý được sự kiện ở trạng thái hiện tại không (không tính lệnh chờ). */
  coThe(suKien, duLieu) {
    const b = this.cfg.bang[this.hienTai]?.[suKien];
    return !!b && (!b.khi || b.khi(duLieu));
  }

  /**
   * Gửi sự kiện. Đang bận và sự kiện không xử lý được ngay → ghi đè lệnh chờ (mới nhất thắng).
   * @returns {boolean} true nếu đã xử lý ngay
   */
  chuyen(suKien, duLieu) {
    const b = this.cfg.bang[this.hienTai]?.[suKien];
    if (!b || (b.khi && !b.khi(duLieu))) {
      if (!this.ranh) this.cho = { suKien, duLieu };
      return false;
    }
    const tu = this.hienTai;
    this.hienTai = b.den;
    const phien = ++this.phien;
    this.cfg.khiDoi?.(tu, b.den, suKien);
    const kq = b.lam?.(duLieu, this);
    const ket = () => {
      if (phien !== this.phien) return; // đã có chuyển trạng thái mới hơn
      if (b.xong) {
        const truoc = this.hienTai;
        this.hienTai = b.xong;
        this.cfg.khiDoi?.(truoc, b.xong, suKien + ':xong');
      }
      this.chayCho();
    };
    if (kq && typeof kq.then === 'function') kq.then(ket, (e) => { ket(); console.error(e); });
    else ket();
    return true;
  }

  chayCho() {
    if (!this.cho || !this.ranh) return;
    const { suKien, duLieu } = this.cho;
    this.cho = null;
    this.chuyen(suKien, duLieu);
  }
}
