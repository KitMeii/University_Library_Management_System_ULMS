# TIẾN ĐỘ DỰ ÁN ULMS

| Pha | Trạng thái | Ghi chú |
|---|---|---|
| 0A – Khảo sát basecore | **Đã duyệt** (15/11/2026) | `BASECORE_PHAN_TICH.md` |
| 0 – Kế hoạch | **Đã duyệt** (15/11/2026) | `KE_HOACH_PHA_0.md`; quyết định D1–D24 |
| 1 – Nền tảng | **Đã duyệt** (16/11/2026) | D25–D33 đã duyệt |
| 2 – Danh mục + quản trị | **Đã duyệt** (16/11/2026) | D34–D43 đã duyệt (D35, D36, D41 theo quyết định chủ dự án); D44 SQL Server cài sẵn; ảnh 4.2.1–4.2.4, 4.6.1–4.6.5 |
| 3 – Chứng từ + in MB02–MB05 | **Đã duyệt** (16/11/2026) | D45, D46, D48 đã duyệt; D47 sửa theo quyết định chủ dự án (bảng X09); D49 kiểm thử trên ThuVien_Test; ảnh 4.3.1–4.3.4, 4.4.1–4.4.4 |
| 4 – Báo cáo MB06–MB07 + tổng quan | **Đã duyệt** (16/11/2026) | D50–D54 đã duyệt; X09 ghi vào file 02 (9 bảng mở rộng); ảnh 4.5.1–4.5.2, Tong_quan_N1–N4 → đủ bộ ảnh Bài 4 (`anh_giao_dien/DANH_SACH_ANH.md`) |
| 5 – Cổng: header, hero, coverflow + đầu sách/thể loại | **Đã duyệt** (17/11/2026) | D55–D59, D61 duyệt; D60 chờ số đo máy thật của chủ dự án; ảnh Cong_*, Pha5_* |
| 6 – Book Preview + đặt chỗ + tài khoản + Quầy đặt chỗ | **Đã duyệt** (17/11/2026) | D62–D71 duyệt (D63 thêm loại “Đặt chỗ bị hủy”, D69 băm CSP thay bản vá); ảnh Cong_Book_Preview*, Cong_Tai_khoan*, Pha6_* |
| 7 – Trang chủ hoàn chỉnh + học từ trang tham khảo | **Xong, đã commit + push theo yêu cầu chủ dự án** (18/11/2026) | D78 (FLIP bằng GSAP core) và D79 (THAM_KHAO_MTA) đã duyệt; D72–D77 *chờ duyệt* (D75 cần chủ dự án cung cấp thông tin thư viện); ảnh Cong_Trang_chu_hoan_chinh*, Cong_Kham_pha*, Cong_Huong_dan*, Cong_Menu*, Cong_Header_thu_gon, so_sanh_mta/ |
| 8 – Hoàn thiện, mobile, bảo mật, hiệu năng, prod.yml, tài liệu | **Đã duyệt, đã commit + push** (18/11/2026) – nghiệm thu cuối chờ D60 | D72–D85 duyệt (D82: phân trang xem trước MB06/MB07; còn một điểm mở: xem trước kỳ 12 tháng); D60 chờ số đo fps máy thật |

## Pha 1 – đã làm
- Git: init, xóa bí mật, commit gốc `a3c44a2` + push; commit xóa riêng `3c0956d` (chưa push – push khi Pha 1 được duyệt).
- Làm phẳng `Backend/` (git mv giữ lịch sử), đổi tên QuanTriService/KhoSachService, thêm MuonTraService, ViPhamService, Migrator; .NET 10; `ULMS.slnx`; quản lý phiên bản gói tập trung.
- `BaseCore.Common`: JWT, hasher D5, IClock, `[YeuCauQuyen]`, lỗi `{ma,thongBao,truong}`, health, Serilog, rate limit.
- CSDL: 26 bảng (18 SPEC + 8 mở rộng) đúng tên/kiểu, CHECK, FK giữa schema, chỉ mục lọc đặt chỗ, SEQUENCE nhật ký, thủ tục cấp số chứng từ; login SQL từng service.
- Dữ liệu khởi tạo (N1–N4, 173 quyền sinh từ SPEC, admin) + dữ liệu mẫu Development (4 NV, 4 khu vực, 8 thể loại, 60 đầu sách, 112 bản sách, 15 bạn đọc + tài khoản cổng, 5 khoản thu).
- Đăng nhập nhân viên SPEC 6.3.1 đầy đủ; đăng xuất; đổi mật khẩu; đăng nhập bạn đọc; gateway Ocelot; web BFF YARP; 2 layout; token CSS; motion.config.js.
- Docker: 7 Dockerfile, compose + override; kiểm thử: unit, tích hợp, E2E/ảnh.

## Pha 2 – đã làm
- 6.1.1 Danh mục sách (+ 03.01.02 tìm loại sách đang mượn, 03.04.01 thống kê tình trạng, dgvChuyenDen + btnCapNhatTT cho 03.01.04/03.02.03/03.03.02).
- 6.1.2 Khoản thu, 6.1.3 Bạn đọc (01.01.01: txtMaSV tìm/mở thêm, btnKiemTraDK, tạo TAIKHOAN_BANDOC, ảnh thẻ), 6.1.4 Khu vực.
- 6.3.2 Tài khoản (khóa/mở khóa, đặt lại MK), 6.3.3 Cấp quyền, 6.3.4 Nhật ký 2 thẻ, 6.3.5 Sao lưu – phục hồi (tiến trình % thật, sao lưu tự động 23:00).
- Khung frontend dùng chung: api.js, form-state.js, luoi.js (DataGrid), danh-muc.js, hop-thoai.js (modal/toast), chon-ngay.js (DatePicker dd/MM/yyyy).
- Sửa lỗi phát hiện khi kiểm thử: AntiForgery theo scheme NhanVien; DTO record không dịch được ORDER BY; BACKUP thất bại bị báo thành công (cờ FireInfoMessageEventOnUserErrors) + quyền ghi volume sao lưu; tràn ngang trên điện thoại.

## Pha 2 – sửa theo quyết định duyệt (16/11/2026)
- D36: sau phục hồi hủy ngay mọi phiên khác (mốc thu hồi = extended property `ULMS_MocThuHoiPhien`; service từ chối JWT cũ – 401 `PHIEN_DA_BI_HUY`; web kiểm tra cookie 15 giây/lần, BFF xóa cookie khi backend trả 401, trang đăng nhập báo lý do; đóng NHATKY_DANGNHAP đang mở; ghi nhật ký). Giữ phiên người phục hồi. Phục hồi chỉ nhận .bak của CSDL ThuVien (RESTORE HEADERONLY).
- D35: ảnh thẻ → WebP 600×800 bằng SkiaSharp 3.119.2 (xoay EXIF, bỏ siêu dữ liệu, chặn ảnh > 40 MP).
- D41: chặn xóa bạn đọc đã có phiếu mượn/gia hạn/trả/biên bản (409 `DA_CO_CHUNG_TU` nêu rõ loại).
- D44: chạy trên SQL Server cài sẵn (`TunKittt`, `DB_SERVER=host.docker.internal,1433`, profile `sql-docker` tắt); thư mục sao lưu theo `InstanceDefaultBackupPath` (đường dẫn Windows); `Database/01–03*.sql` sinh từ migrator (`scripts/xuat-sql.sh`), đã chạy thử 2 lần trên SQL Server trống.
- Kiểm thử mới: `MocThuHoiPhienTests` (unit), `ThuHoiPhienTests`, `BanDoc_DaCoChungTu_KhongXoaDuoc`, `PhucHoi_TuChoiTepCuaCsdlKhac` (tích hợp), `BanDoc_AnhThe_ChuyenWebP_ThuNho` (E2E). Đã chạy thử chu trình sao lưu → phục hồi thật trên SQL Server của máy.

## Pha 3 – đã làm
- 6.2.1 Lập Phiếu mượn sách (TT1/TT2/TT3 đúng SPEC, bổ sung phiếu trong ngày, sửa phiếu đã lưu, không đáp ứng + gợi ý bản thay thế), 6.2.2 Lập Phiếu gia hạn (kiểm tra điều kiện D45, hạn mới), 6.2.3 Lập Phiếu trả (bổ sung, quá hạn, mất/hư hỏng → trường hợp vi phạm), 6.2.4 Lập Biên bản vi phạm (dgvTruongHop, khoản thu, thành tiền/tổng tiền, ghi nhận thanh toán).
- Số chứng từ PM/GH/PT/BB cấp KHI LƯU qua `muontra.usp_CapSoChungTu` trong transaction (không mất số, không trùng khi đồng thời).
- Mẫu in MB02–MB05 (A5/A4, 02 liên, `@page`), in trong khung ẩn + tiến trình lưu %.
- “Chuyển giữa bộ phận” suy ra từ dữ liệu: dgvChuyenDen 03.01.04/03.02.03/03.03.02 (N3), trường hợp vi phạm (N2), kết quả xử lý → điều kiện mượn (N1).
- Dữ liệu mẫu lịch sử 2 tháng: 25 phiếu mượn, 8 gia hạn, 18 trả, 5 biên bản (quá hạn, mất, hư hỏng, 1 chưa thanh toán, 1 trường hợp chờ lập biên bản, vài dòng chờ kho).
- Kiểm thử: SPEC đọc trực tiếp bảng trạng thái nút (SpecDoc.TrangThaiNut) cho E2E; tích hợp luồng đầy đủ + cấp số đồng thời + quyền; ảnh 4.3.1–4.3.4 (TT1/TT2/TT3), 4.4.1–4.4.4.
- D45–D48 đã duyệt 16/11/2026.

## Pha 3 – sửa theo quyết định duyệt (16/11/2026)
- D47: bảng mở rộng X09 `khosach.XULY_CHUYENDEN` ghi việc N3 đã xử lý 03.03.02 (`btnCapNhatTT`, hoặc `btnSua` đổi tình trạng). Bản sách trả mất/hư hỏng chỉ bị chặn cho mượn tới khi N3 xử lý; sửa sách xong đặt “Sẵn sàng” thì mượn lại được, dòng 03.03.02 không xuất hiện lại. Migration `XuLyChuyenDen` (ghi nhận lùi dữ liệu cũ); dữ liệu mẫu ghi nhận sách mất đã xử lý. Kiểm thử `HuHong_N3CapNhat_SanSang_ChoMuonLai`, `HuHong_N3SuaThangSanSang_ChoMuonLai`.
- D48: Phụ lục A trong `GIA_DINH.md` – danh sách đầy đủ control thêm ngoài SPEC theo từng giao diện (tên, loại, mục đích, nhóm “Đặt tên”/“Thêm mới”).
- D49: kiểm thử chạy bộ hệ thống riêng `ulms-test` trên CSDL `ThuVien_Test` (tạo lại mỗi lần chạy); sao lưu kiểm thử ghi vào `SAO_LUU_THU_MUC_KIEM_THU` và được dọn sau khi chạy – không đụng CSDL ThuVien, không ghi vào thư mục Backup thật.

## Pha 4 – đã làm
- Báo cáo MB06 (01.04.02 Tổng hợp → 01.04.03 Lập báo cáo) và MB07 (02.05.01 → 02.05.02): kỳ báo cáo (DatePicker + chọn nhanh), btnTongHop, 5 số liệu theo công thức SPEC 6.4.5–6.4.6, biểu đồ ECharts, bảng chi tiết theo thẻ; trang lập báo cáo xem trước bản in, ngày lập tự sinh, btnIn ghi nhật ký rồi in; trang in A4 01 bản, biểu đồ SVG.
- API muontra `/api/quan-tri/bao-cao/muon-tra`, vipham `/api/quan-tri/bao-cao/vi-pham` (+ `/lap`), tổng quan `/api/quan-tri/thong-ke/n1…n4` – quyền theo SPEC 3.4.
- Trang tổng quan theo nhóm N1–N4: KPI đếm lên + thanh tiến độ, KPI bấm được (danh sách chi tiết / trang lọc sẵn), biểu đồ area + dataZoom, bar ngang Top 10, heatmap lịch, bar chồng, donut, line; “Xem bảng số liệu”, “Tải ảnh”, skeleton, trạng thái rỗng; tắt hiệu ứng khi giảm chuyển động. Đặt chỗ ẩn (D50).
- Khung biểu đồ dùng chung `js/charts/chart-theme.js`, `chart-factory.js`; ECharts 5.6.0 tự host; `motion.config.js` thêm `chart`.
- Sửa lỗi phát hiện: trang in bị chặn nhúng khung (btnIn Pha 3 in khung trống) – D54; lưới dữ liệu tĩnh ghi sai “1–15 / n dòng”; đường biểu đồ làm mượt vượt điểm dữ liệu.
- Kiểm thử: tích hợp `Pha4ApiTests` (so truy vấn kiểm chứng MB06, MB07, N1–N4; kỳ 422; quyền 403; nhật ký lập báo cáo), E2E `Pha4Tests` (tổng quan, KPI chi tiết, luồng tổng hợp → lập → xem trước → in, trang in SVG, quyền, điện thoại, axe, CSP thật), ảnh 4.5.1–4.5.2, Tong_quan_N1–N4.
- D51–D54 và bảng X09 đã duyệt 16/11/2026.

## Pha 5 – đã làm
- Cổng bạn đọc: header 2 tầng (Times New Roman, giờ mở cửa hôm nay, thanh tiến độ cuộn, menu Offcanvas trên điện thoại, đánh dấu khu đang xem), footer thông tin thư viện; cuộn mượt Lenis 1.3.27 (máy tính) đồng bộ ScrollTrigger; xuất hiện khi cuộn.
- Hero: 4 tranh minh họa SVG (D56, đọc mọi ảnh trong `wwwroot/img/hero`), chuyển ảnh GSAP theo file 04, nút Tạm dừng/Phát, chấm điều hướng, dừng khi trỏ/focus, ảnh sau decode() xong mới chuyển, giảm chuyển động → mờ chéo.
- Kệ sách nổi bật coverflow (file 05): một biến `pos` + `layout(pos)`, khối sách CSS 3D (bìa, 2 gáy), cung lùi sâu, thu nhỏ dần, gáy tối dần; chữ trang trí chấm bán sắc; bóng sàn; tên sách đổi theo ký tự; thanh thời gian (năm / thứ hạng); nút ← →, phím, cuộn ngang, kéo có quán tính, bấm gáy, con trỏ tròn “Mở” + bìa nghiêng theo chuột; đổi kệ (Sách mới về, Mượn nhiều nhất, từng thể loại) – yêu cầu mới nhất thắng; state machine (`state-machine.js`); điện thoại: chip kệ, vuốt; không JS: lưới bìa; giảm chuyển động: mờ chéo. Bấm cuốn giữa → “Xem nhanh” (D57).
- API khosach: `/api/cong/ke-sach` (+ `/danh-sach`), quản lý đầu sách `/api/quan-tri/dau-sach` (gắn/bỏ gắn bản sách – D55), thể loại `/api/quan-tri/the-loai`; tải ảnh bìa 2 cỡ. Trang dashboard Quản lý đầu sách, Quản lý thể loại (nhóm Kho sách – D61).
- Kiểm thử: tích hợp `Pha5ApiTests` (kệ so truy vấn kiểm chứng, CRUD + ràng buộc + quyền), E2E `Pha5Tests` (header/hero, kệ: phím, nút, bấm gáy, bấm liên tục, kéo, thanh thời gian, đổi kệ, xem nhanh, giảm chuyển động, không JS, điện thoại, axe, CSP thật, số đo khung hình), ảnh `ChupAnhPha5Tests`.
- D57–D59, D61 duyệt 17/11/2026 (D61: ẩn phần mở rộng ở chế độ chụp ảnh Bài 4, khôi phục 20 ảnh 4.2–4.6 từ df9a031). D60: chủ dự án đo trên máy thật, chưa gửi kết quả; dưới 55 fps thì tối ưu theo file 05 mục 7.

## Pha 6 – đã làm
- Đặt chỗ 24 giờ (file 06 mục 2–3): `DatChoService` – 6 điều kiện, mỗi điều kiện mã lỗi + câu thông báo + hướng xử lý; một transaction, khóa ứng dụng BANDOC → DAUSACH → SACH, chọn bản Kệ mở/mã nhỏ nhất, chỉ mục lọc chặn trùng, 409 “Sách vừa được người khác đặt”; mã `DCyyyyMMdd-####`; không đổi SACH.tinhtrang; đặt chỗ quá hạn coi như hết ngay trong mọi truy vấn (muontra, khosach); hết hạn “lười” trước khi đặt (D71).
- Tác vụ nền `DatChoNenService` (mỗi `ChuKyQuetPhut`, bộ kiểm thử 3 giây): Đang giữ quá hạn → Hết hạn + THONGBAO; nhắc “Sắp hết hạn giữ” một lần; từ 7:00 nhắc “Sắp đến hạn trả” (còn 2 ngày), “Quá hạn trả”. Mọi thời điểm qua IClock.
- Book Preview (file 06 mục 1): bìa bay từ vị trí nguồn → mở quanh gáy → lật 3 tờ → hai trang giấy; StPageFlip 2.0.7 (máy tính 2 trang, điện thoại 1 trang vuốt, nút Đặt chỗ ghim đáy); đóng bằng ×/Esc/nền/Back, bay về nguồn; liên kết sâu `/sach/{ma}`; giữ focus; giảm chuyển động mờ 0.25s. Đăng nhập ngay trong trang sách rồi tự đặt chỗ; vé giữ chỗ (mã vạch Code128 SVG, đếm ngược lật số, đỏ khi < 2 giờ, tem ĐÃ GIỮ CHỖ + tia); yêu thích; chia sẻ liên kết; lý do không đủ điều kiện ngay trên trang.
- Tài khoản bạn đọc (file 07): kích hoạt / đặt lại mật khẩu `/kich-hoat` (sai 5 lần khóa 15 phút – D68); trang cá nhân kính mờ `/tai-khoan` 8 trang (tổng quan + biểu đồ cột 12 tháng, đang mượn, đặt chỗ, lịch sử theo năm, vi phạm, yêu thích, thông báo, đổi mật khẩu), thanh viên thuốc dọc / tab đáy trên điện thoại; chuông thông báo header cổng.
- Nghiệp vụ N1 (mở rộng, ẩn ở chế độ ảnh Bài 4): `dgvDatCho` + `btnNhanDatCho` trong 01.01.03, btnThemSach từ chối bản giữ cho người khác, btnIn chuyển đặt chỗ “Đã nhận” (so_pmuon, ma_nd_xuly) + THONGBAO; trang Quầy đặt chỗ (lọc, đếm ngược, hủy có lý do, in phiếu soạn sách A4); chuông topbar; tổng quan N1 bật lại KPI + donut đặt chỗ, Top 10 theo đầu sách (D64). N4 khóa/mở khóa tài khoản cổng ở Danh mục Bạn đọc (D66).
- Dữ liệu mẫu: 8 đặt chỗ đủ 4 trạng thái (2 đang giữ còn < 2 giờ) + thông báo + yêu thích.
- Duyệt 17/11/2026: D63 thêm loại THONGBAO “Đặt chỗ bị hủy” (migration + file 02); D65 thêm kiểm thử Production; D69 bỏ bản vá page-flip, CSP băm SHA-256, kiểm thử giải mã Code128 (`MaVachTests`); xóa 2 đặt chỗ + thông báo do smoke test để lại trong CSDL ThuVien.
- Kiểm thử: đơn vị `VongDoiDatChoTests`, `CheDoAnhBai4Tests`; tích hợp `Pha6ApiTests` (20 yêu cầu song song → đúng 1 thành công, tua 24 giờ → Hết hạn + thông báo, nhắc 1 lần, nhận tại quầy, từng điều kiện, kích hoạt/khóa tạm, trang cá nhân so truy vấn kiểm chứng, phân quyền); E2E `Pha6Tests` (Book Preview: Back/liên kết sâu/focus/điện thoại/giảm chuyển động/CSP thật/axe; đặt chỗ có đăng nhập trong trang; trang cá nhân + axe; N1 dgvDatCho, Quầy đặt chỗ; chế độ ảnh Bài 4 ẩn phần mở rộng); ảnh `ChupAnhPha6Tests`.

## Pha 7 – đã làm
- Trang chủ đủ khu theo file 04 mục 1: header (Trang chủ, Khám phá sách, Danh mục, Kệ sách nổi bật, Hướng dẫn mượn sách, Thông tin thư viện; Tìm kiếm, Yêu thích có số lượng, Thông báo), Hero + thanh tìm kiếm + gợi ý từ khóa phổ biến, kệ coverflow, Danh mục thể loại (ô thể loại + số đầu sách), Khám phá sách, Hướng dẫn mượn và đặt chỗ (số liệu từ quy định máy chủ), Thư viện bằng số (đếm lên khi cuộn tới), Thông tin thư viện (chỉ khi có dữ liệu – D75), footer.
- Khám phá sách: tìm tên sách/tác giả/ISBN/từ khóa không dấu (D72), lọc thể loại, năm xuất bản, chỉ sách còn có thể mượn, sắp xếp (mới nhất, tên, mượn nhiều, năm); “Xem thêm”; hiệu ứng FLIP tự viết bằng GSAP core khi đổi bộ lọc (bỏ plugin Flip – D78); không đổi route, không tải lại, giữ danh sách cũ khi tải, yêu cầu mới nhất thắng. Book Card 2:3 (bìa ảnh hoặc bìa chữ), nhãn tình trạng, tim yêu thích, bấm → Book Preview. Từ khóa trong Book Preview → `/kham-pha?q=`.
- Trang `/kham-pha` (bộ lọc năm từ – đến), `/huong-dan` (mượn, đặt chỗ, gia hạn, trả, vi phạm, tài khoản – số liệu từ `GET /api/cong/quy-dinh`, D74).
- API khosach: `GET /api/cong/sach` (tìm/lọc/sắp xếp/phân trang), `/api/cong/sach/tu-khoa`, `/api/cong/sach/so-lieu`, `/api/cong/the-loai`; muontra `GET /api/cong/quy-dinh` (route gateway công khai mới).
- Hiệu năng: khu dưới kệ sách dùng `content-visibility: auto` – kéo kệ ở CPU chậm 4× giữ ~25 fps trong container không GPU (trang dài làm tụt xuống 15,8 fps trước khi tối ưu); CLS < 0,1.
- Kiểm thử: tích hợp `Pha7ApiTests` (tìm không dấu so truy vấn kiểm chứng, lọc, phân trang, thể loại/số liệu/từ khóa/quy định khớp CSDL); E2E `Pha7Tests` (đủ khu đúng thứ tự, tìm Hero không đổi route/không tải lại, lọc thể loại, Xem thêm, yêu cầu mới nhất thắng, thẻ → Book Preview → từ khóa → /kham-pha, /huong-dan, axe + CSP thật + CLS, điện thoại 2 cột); ảnh `ChupAnhPha7Tests`.
- Học từ trang tham khảo (THAM_KHAO_MTA.md, duyệt 17/11/2026 – D79): header máy tính thu gọn khi cuộn; menu thả xuống “Danh mục” (thể loại thật) và “Hướng dẫn mượn sách” dùng được bằng bàn phím (↓ mở, ↑↓ di chuyển, Esc đóng trả focus), trên điện thoại là nhóm thu gọn trong Offcanvas; Hero thêm nút ← → (và phím ← →) + gợi ý “Cuộn xuống”; tiêu đề khu có vạch nhấn + “Xem tất cả ▸”; ô thể loại có biểu tượng nền mờ; số liệu trong vòng viền; footer 3 cột + hoa văn kệ sách tự vẽ; nút lên đầu trang; xuất hiện khi cuộn so le (`ScrollTrigger.batch`). Không thêm thư viện, chỉ transform/opacity, có bản giảm chuyển động; CSP không `'unsafe-hashes'`/`'unsafe-inline'`. Ảnh tham khảo không đưa vào git.
- Đo lại kệ sách sau thay đổi (Chromium headless trong container, không GPU): hai lần chạy: CPU thường 59–59,6 fps, CPU chậm 4× 28,1–34,4 fps (trước thay đổi 60 / 24,6) – không giảm.
- Sửa kiểm thử tích hợp tính “hôm nay” theo giờ máy (UTC trong container) – lệch một ngày từ 00:00 đến 07:00 giờ Việt Nam; nay dùng `GioVietNam.HomNay` trùng IClock của service.
- Kiểm thử bổ sung: E2E `Pha7MtaTests` (menu thả xuống bằng bàn phím + nạp thể loại + bấm thể loại lọc không tải lại, nhóm thu gọn điện thoại, mũi tên Hero chuột/bàn phím, nút lên đầu + header thu gọn, CSP không unsafe, giảm chuyển động); ảnh `ChupAnhPha7Tests.Chup_DieuHuong_ThamKhao`; ảnh so sánh trước/sau trong `anh_giao_dien/so_sanh_mta/`.

## Pha 8 – đã làm
- Quyết định Pha 7 (18/11/2026): D72–D74, D76 duyệt; D77 – script một lần `Database/mot_lan/D77_cap_nhat_tu_khoa_mau.sql` (sinh từ migrator `--sql-tu-khoa-mau`), đã chạy trên ThuVien: 60/60 đầu sách mẫu, chạy lại 0; D79 – header điện thoại không thu gọn; xóa nhánh `wip/pha7-mta`.
- D75 – thông tin thư viện qua `.env` (`THUVIEN_*`, chỗ giữ `[ĐỊA CHỈ]`, `[EMAIL]`, `[SỐ ĐIỆN THOẠI]`, giờ mở cửa ví dụ): header “Hôm nay: …” (giờ Việt Nam), cột **Liên hệ** footer, **khu 8** trang chủ (giờ mở cửa gộp ngày, nội quy sinh từ `/api/cong/quy-dinh`).
- Triển khai: `docker-compose.prod.yml` + Caddy 2.10.2 (`deploy/Caddyfile`: HTTPS tự động / CA nội bộ, HTTP → HTTPS, HSTS, nén, ẩn Server, log truy cập), chỉ Caddy mở cổng (D80); `scripts/kiem-tra-trien-khai.sh` – nghiệm thu file 14 mục 8 trên bộ riêng (prod + SQL Server trong Docker); PowerShell `scripts/test-docker.ps1`, `scripts/trien-khai.ps1` (D81).
- Bảo mật: giới hạn tần suất đặt chỗ 10/phút/phiên (D83); web không lộ header `Server`; cookie Secure/HttpOnly/SameSite qua HTTPS; kiểm tra không có bí mật trong image, mọi image chạy user `app`.
- Hiệu năng (D82): truy vấn gộp số bản có thể mượn/lượt mượn, bộ nhớ đệm 60 giây (kệ sách, số liệu, bản đồ khu Khám phá, tổng quan N1–N4), tìm N3 so khớp nhị phân chữ hoa, tìm không dấu quét một lần, chỉ mục `IX_PHIEUTRA_so_pmuon INCLUDE (ngaytra)` (migration `ChiMucPhieuTraNgayTra`, file 02, `Database/01`); image web 942 → 376 MB.
- Hoàn thiện (D84, D85): tương phản bìa chữ màu tầm trung, liên kết mô tả dashboard, liên hệ khu 8; khung giữ chỗ khu Khám phá (CLS `/kham-pha` 0,135 → đạt); sửa bộ đo CLS/CSP trong E2E (init script không chạy); Razor xuất tiếng Việt nguyên dạng; build 0 cảnh báo; viên giờ mở cửa header gọn một dòng.
- Tài liệu: `docs/HUONG_DAN_TRIEN_KHAI.md`, `docs/HUONG_DAN_SU_DUNG.md`, README (bảng tài liệu, lệnh hiệu năng/kiểm tra triển khai/prod, phiên bản Caddy), THU_VIEN_BEN_THU_BA.
- Kiểm thử mới: `Pha8Tests` (axe **mọi** trang dashboard theo đúng nhóm + mọi trang cổng; iPhone 13 và Pixel 7 không tràn ngang trên mọi trang + sidebar thành Offcanvas; Pixel 7 kệ/Book Preview/tab đáy; LCP < 2,5 s, CLS < 0,05), `Pha7MtaTests.ThongTinThuVien_Khu8…`, `ThongTinThuVienTests` (đơn vị), `HieuNangTests`; ảnh `ChupAnhPha8Tests` (bản điện thoại mọi giao diện).
- D82 (chủ dự án quyết 18/11/2026): **xem trước** MB06/MB07 phân trang bảng chi tiết 50 dòng/trang (`/chi-tiet/{bang}`), số liệu tổng hợp + biểu đồ tính toàn kỳ; **bản in** `/in` đủ dòng; lưới ≤ 50 dòng giữ nguyên dạng tĩnh (ảnh Bài 4 4.5.1–4.5.2 không đổi). Kiểm thử `Pha8ApiTests` (2), E2E `BaoCaoMuonTra_XemTruoc_PhanTrang_KpiKhongDoi_BanInDuKy`.
- Kết quả cuối (18/11/2026): đơn vị 82/82, ảnh 39/39, tích hợp 104/104, E2E 124/124, kiểm tra triển khai 28/28; build 0 cảnh báo. Hiệu năng (50.112 bản sách, 200.040 dòng CT_PHIEUMUON, 200.030 dòng CT_PHIEUTRA): mọi API tương tác P95 5–235 ms (< 300) – gồm xem trước MB06 tháng 213 ms, trang chi tiết 21–189 ms, MB07 18–33 ms; bản in MB06 tháng 794 ms (< 1.000). Điểm mở: xem trước MB06 **12 tháng** P50 ~205 ms, P95 dao động 290–390 ms giữa các lần đo (D82). Lần gọi đầu khi bộ nhớ đệm hết hạn: kệ sách ~1,1–1,2 s, bản đồ Khám phá ~2,1 s (tối đa một lần mỗi 60 giây). Kệ sách: 59,6 fps CPU thường, 26,3 fps CPU chậm 4× (container không GPU; dao động 26–37 giữa các lần).

## Việc còn lại / chuyển pha sau
- Thông tin thư viện thật: thay chỗ giữ `THUVIEN_*` trong `.env` máy chủ trước khi mở cho bạn đọc (D75, HUONG_DAN_TRIEN_KHAI mục 3.3).
- D60: chủ dự án gửi số đo fps máy thật trước nghiệm thu Pha 8.
- Chính sách mật khẩu SQL (D85 – chủ dự án quyết 18/11/2026): dev giữ mật khẩu `.env` hiện có, không kiểm tra; máy chủ thật / SQL Server trong Docker: ≥ 8 ký tự, hoa, thường, số, ký tự đặc biệt – `scripts/trien-khai.sh prod` (`.ps1`) báo lỗi rõ nếu không đạt.
- Kiểm thử chạy trên bộ hệ thống riêng (project `ulms-test`, CSDL `ThuVien_Test` – D49).
- Không làm đặt lại mật khẩu qua email (D68 – chủ dự án quyết định): bạn đọc đặt lại tại /kich-hoat hoặc nhờ N4.
