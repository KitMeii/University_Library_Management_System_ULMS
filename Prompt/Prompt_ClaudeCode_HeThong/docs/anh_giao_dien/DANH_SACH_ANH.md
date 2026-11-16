# DANH SÁCH ẢNH GIAO DIỆN – đối chiếu mục IV Bài 4

Ảnh chụp tự động bằng Playwright (`sh scripts/test-docker.sh anh`) trên CSDL kiểm thử vừa tạo lại với dữ liệu mẫu, không có nhãn “DỮ LIỆU MẪU”.
Máy tính 1366×768 (ảnh “toàn trang” ghép theo chiều dọc), điện thoại 390×844 (đuôi `_mobile`).
Đối chiếu số mục (SPEC mục 6): 6.1.k ↔ 4.2.k · 6.2.k ↔ 4.3.k · 6.4.1–6.4.4 ↔ 4.4.1–4.4.4 · 6.4.5–6.4.6 ↔ 4.5.1–4.5.2 · 6.3.k ↔ 4.6.k.
TT1/TT2/TT3 = trạng thái 1/2/3 của form chứng từ theo bảng “Control và xử lý” (SPEC mục 5, 6.2).

Cập nhật: 16/11/2026 (hết Pha 4).

## 4.2 Giao diện danh mục

| Tệp | Mục | Giao diện | Trạng thái / nội dung |
|---|---|---|---|
| `4.2.1_Danh_muc_sach.png` | 4.2.1 | Danh mục sách (`frmDanhMucSach`) – N3 | Đã chọn 1 dòng `dgvSach` (thông tin đổ lên các ô chỉ đọc, btnSua bật); thống kê theo tình trạng 03.04.01; `dgvChuyenDen` |
| `4.2.1_Danh_muc_sach_mobile.png` | 4.2.1 | Danh mục sách | Điện thoại, khi mở form |
| `4.2.2_Danh_muc_khoan_thu.png` | 4.2.2 | Danh mục khoản thu (`frmDanhMucKhoanThu`) – N4 | Đã chọn 1 dòng `dgvKhoanThu` |
| `4.2.3_Danh_muc_ban_doc.png` | 4.2.3 | Danh mục Bạn đọc (`frmDanhMucBanDoc`) – 01.01.01, N1 | Khi mở form |
| `4.2.3_Danh_muc_ban_doc_Kiem_tra_dieu_kien.png` | 4.2.3 | Danh mục Bạn đọc | Sau `txtMaSV` + `btnTimKiem` (SV22000001) và `btnKiemTraDK`: kết quả điều kiện mượn, sách đang mượn |
| `4.2.4_Danh_muc_khu_vuc.png` | 4.2.4 | Danh mục khu vực lưu trữ (`frmDanhMucKhuVuc`) – N4 | Đã chọn 1 dòng `dgvKhuVuc` |
| `4.2.4_Danh_muc_khu_vuc_Them.png` | 4.2.4 | Danh mục khu vực lưu trữ | Sau `btnThem`: ô nhập mở, btnLuu/btnHuy bật |

## 4.3 Giao diện lập chứng từ

| Tệp | Mục | Giao diện | Trạng thái / nội dung |
|---|---|---|---|
| `4.3.1_Lap_phieu_muon_TT1.png` | 4.3.1 | Lập Phiếu mượn sách (`frmLapPhieuMuon`) – 01.01.03 | **TT1**: mở form, chỉ nhập mã SV/HV/CB hoặc số phiếu đã lưu |
| `4.3.1_Lap_phieu_muon_TT2.png` | 4.3.1 | Lập Phiếu mượn sách | **TT2**: đã tìm Bạn đọc (đủ điều kiện mượn), nhập bản sách |
| `4.3.1_Lap_phieu_muon_TT3.png` | 4.3.1 | Lập Phiếu mượn sách | **TT3**: đã thêm 2 bản sách vào `dgvChiTiet`, btnIn bật |
| `4.3.1_Lap_phieu_muon_mobile.png` | 4.3.1 | Lập Phiếu mượn sách | Điện thoại, TT1 |
| `4.3.2_Lap_phieu_gia_han_TT1.png` | 4.3.2 | Lập Phiếu gia hạn sách (`frmLapPhieuGiaHan`) – 01.02.01 | **TT1**: mở form |
| `4.3.2_Lap_phieu_gia_han_TT2.png` | 4.3.2 | Lập Phiếu gia hạn sách | **TT2**: đã nạp Phiếu mượn sách, chọn bản sách gia hạn |
| `4.3.2_Lap_phieu_gia_han_TT3.png` | 4.3.2 | Lập Phiếu gia hạn sách | **TT3**: đã chọn bản sách, kết quả “Đồng ý”, hạn trả mới; btnIn bật |
| `4.3.3_Lap_phieu_tra_TT1.png` | 4.3.3 | Lập Phiếu trả sách (`frmLapPhieuTra`) – 01.02.02 | **TT1**: mở form |
| `4.3.3_Lap_phieu_tra_TT2.png` | 4.3.3 | Lập Phiếu trả sách | **TT2**: đã nạp Phiếu mượn sách (bạn đọc đang mượn quá hạn) |
| `4.3.3_Lap_phieu_tra_TT3.png` | 4.3.3 | Lập Phiếu trả sách | **TT3**: đã chọn bản sách trả, số ngày quá hạn; btnIn bật |
| `4.3.4_Lap_bien_ban_vi_pham_TT1.png` | 4.3.4 | Lập Biên bản vi phạm mượn trả sách (`frmLapBienBan`) – 02.03.01 | **TT1**: mở form, `dgvTruongHop` – trường hợp vi phạm chờ lập biên bản |
| `4.3.4_Lap_bien_ban_vi_pham_TT2.png` | 4.3.4 | Lập Biên bản vi phạm | **TT2**: đã nạp trường hợp vi phạm (nhấp đúp), chọn khoản thu |
| `4.3.4_Lap_bien_ban_vi_pham_TT3.png` | 4.3.4 | Lập Biên bản vi phạm | **TT3**: đã thêm dòng `dgvChiTiet`, thành tiền/tổng tiền, hình thức thanh toán; btnIn bật |

## 4.4 Mẫu in chứng từ

| Tệp | Mục | Mẫu | Ghi chú |
|---|---|---|---|
| `4.4.1_Phieu_muon_sach_MB02.png` | 4.4.1 | Phiếu mượn sách (MB02) | A5, 02 liên (giao Bạn đọc / lưu) |
| `4.4.2_Phieu_gia_han_sach_MB03.png` | 4.4.2 | Phiếu gia hạn sách (MB03) | A5, 02 liên |
| `4.4.3_Phieu_tra_sach_MB04.png` | 4.4.3 | Phiếu trả sách (MB04) | A5, 02 liên |
| `4.4.4_Bien_ban_vi_pham_MB05.png` | 4.4.4 | Biên bản vi phạm mượn trả sách (MB05) | A4, 02 liên |

## 4.5 Báo cáo

| Tệp | Mục | Giao diện | Trạng thái / nội dung |
|---|---|---|---|
| `4.5.1_Tong_hop_muon_tra_TT1.png` | 4.5.1 | 01.04.02 Tổng hợp tình hình mượn trả sách (`frmTongHopMuonTra`) | Mở form: chọn kỳ báo cáo, chưa tổng hợp (btnLapBaoCao tắt) |
| `4.5.1_Tong_hop_muon_tra.png` | 4.5.1 | 01.04.02 Tổng hợp tình hình mượn trả sách | Sau `btnTongHop` (kỳ 60 ngày): số liệu tổng hợp, biểu đồ theo ngày, chi tiết lượt mượn |
| `4.5.1_Tong_hop_muon_tra_mobile.png` | 4.5.1 | 01.04.02 Tổng hợp tình hình mượn trả sách | Điện thoại, sau tổng hợp |
| `4.5.1_Lap_bao_cao_muon_tra.png` | 4.5.1 | 01.04.03 Lập Báo cáo tình hình mượn trả sách (`frmBaoCaoMuonTra`) | Xem trước MB06, ngày lập tự sinh, btnIn bật |
| `4.5.1_Bao_cao_muon_tra_MB06.png` | 4.5.1 | Báo cáo tình hình mượn trả sách (MB06) | Bản in A4, 01 bản – nơi nhận Ban giám đốc thư viện |
| `4.5.2_Tong_hop_vi_pham_TT1.png` | 4.5.2 | 02.05.01 Tổng hợp tình hình vi phạm mượn trả sách (`frmTongHopViPham`) | Mở form: chọn kỳ báo cáo, chưa tổng hợp |
| `4.5.2_Tong_hop_vi_pham.png` | 4.5.2 | 02.05.01 Tổng hợp tình hình vi phạm | Sau `btnTongHop` (kỳ 90 ngày): số liệu, biểu đồ theo khoản thu, tổng hợp theo khoản thu |
| `4.5.2_Tong_hop_vi_pham_mobile.png` | 4.5.2 | 02.05.01 Tổng hợp tình hình vi phạm | Điện thoại, sau tổng hợp |
| `4.5.2_Lap_bao_cao_vi_pham.png` | 4.5.2 | 02.05.02 Lập Báo cáo tình hình vi phạm (`frmBaoCaoViPham`) | Xem trước MB07, ngày lập tự sinh, btnIn bật |
| `4.5.2_Bao_cao_vi_pham_MB07.png` | 4.5.2 | Báo cáo tình hình vi phạm mượn trả sách (MB07) | Bản in A4, 01 bản – nơi nhận Ban giám đốc thư viện |

## 4.6 Giao diện quản trị hệ thống và đăng nhập

| Tệp | Mục | Giao diện | Trạng thái / nội dung |
|---|---|---|---|
| `4.6.1_Dang_nhap.png` | 4.6.1 | Đăng nhập (`frmDangNhap`) | Khi mở |
| `4.6.1_Dang_nhap_Sai_mat_khau.png` | 4.6.1 | Đăng nhập | Sau `btnDangNhap` với mật khẩu sai: thông báo lỗi |
| `4.6.1_Dang_nhap_mobile.png` | 4.6.1 | Đăng nhập | Điện thoại |
| `4.6.2_Quan_ly_tai_khoan.png` | 4.6.2 | Quản lý tài khoản người dùng (`frmTaiKhoan`) – 04.00.01 | Đã chọn tài khoản `muontra` trong `dgvNguoiDung` |
| `4.6.2_Quan_ly_tai_khoan_mobile.png` | 4.6.2 | Quản lý tài khoản người dùng | Điện thoại |
| `4.6.3_Cap_quyen.png` | 4.6.3 | Cấp quyền cho tài khoản (`frmPhanQuyen`) – 04.00.02 | `cboNhom` = N1, danh sách quyền đã cấp |
| `4.6.4_Nhat_ky_Lich_su_dang_nhap.png` | 4.6.4 | Theo dõi nhật ký hệ thống (`frmNhatKy`) – 04.00.03 | Thẻ Lịch sử đăng nhập |
| `4.6.4_Nhat_ky_Hoat_dong.png` | 4.6.4 | Theo dõi nhật ký hệ thống – 04.00.08 | Thẻ Nhật ký hoạt động |
| `4.6.5_Sao_luu_Chon_thu_muc.png` | 4.6.5 | Sao lưu và phục hồi dữ liệu (`frmSaoLuu`) – 04.00.09 | Sau `btnChon`: hộp chọn thư mục sao lưu |
| `4.6.5_Sao_luu_phuc_hoi.png` | 4.6.5 | Sao lưu và phục hồi dữ liệu | Sau `btnThucHien` (Sao lưu): tiến trình 100% và thông báo thành công. Tên tệp hiện `ThuVien_Test_…bak` vì ảnh chụp trên CSDL kiểm thử (GIA_DINH D49) |

## Ảnh tham khảo – không thuộc mục IV Bài 4

| Tệp | Nội dung |
|---|---|
| `Tong_quan_N1.png`, `Tong_quan_N1_mobile.png` | Trang tổng quan Bộ phận mượn trả sách (KPI, biểu đồ – file 09) |
| `Tong_quan_N2.png`, `Tong_quan_N3.png`, `Tong_quan_N4.png` | Trang tổng quan Bộ phận xử lý vi phạm, Kho sách, Quản trị |
| `Pha1_Man_hinh_mac_dinh_N1.png` … `_N4.png` | Màn hình mặc định ngay sau đăng nhập của từng nhóm (SPEC 3.3) |
| `Pha1_Tong_quan_N1.png` … `_N4.png`, `Pha1_Tong_quan_N4_mobile.png` | Trang Tổng quan mở từ menu (chụp lại mỗi lần chạy – nay là trang tổng quan Pha 4) |
| `Pha1_Menu_N4_mobile.png` | Menu chức năng dạng ngăn kéo trên điện thoại (N4) |
| `Cong_Trang_chu_khung.png`, `Cong_Trang_chu_khung_mobile.png` | Khung trang chủ cổng bạn đọc (hoàn thiện ở Pha 5–7) |
| `Cong_Dang_nhap.png`, `Cong_Dang_nhap_mobile.png` | Đăng nhập cổng bạn đọc |
