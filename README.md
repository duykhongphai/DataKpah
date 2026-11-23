# DataKpah

Bộ lấy dữ liệu tự động từ máy chủ Khí Phách Anh Hùng (KPAH). Giao thức được viết lại từ client
Unity `KPAH_276` (phiên bản game `2.7.6`). Chương trình chạy headless, không cần mở Unity hay
điều khiển cửa sổ game.

Trang tra cứu: **https://duykhongphai.github.io/DataKpah/**

Giao diện web đọc trực tiếp kho `TeaMobi/DuLieu`. Trang có:
- tìm kiếm, sắp xếp, phân trang;
- xem chi tiết từng mục;
- chép ID hoặc JSON, mở JSON gốc.

Trang dùng được trên cả máy tính và điện thoại. Khi lượt lấy dữ liệu đầu tiên chưa xong, trang báo
đang chờ và đưa đường dẫn tới GitHub Actions.

## Dữ liệu lấy được

| File | Nội dung | Nguồn trong client |
|---|---|---|
| `EquipmentItems.json` | Mẫu trang bị: tên, loại, phái, cấp, độ bền, giá, icon | cmd `25` (`ItemTemplate`) |
| `ItemOptions.json` | Tên chỉ số trang bị | cmd `25` (`NameAttributeItem`) |
| `Potions.json` | Bình thuốc: tên, mô tả, giá, lượng hồi, hồi chiêu | cmd `25` + cmd `1` |
| `Gems.json` | Ngọc, đá, nguyên liệu ép đồ | cmd `25` (`gemTemplate`) |
| `ShopItems.json` | Vật phẩm cửa hàng | cmd `25` (`shopTemplate`) |
| `Monsters.json` | Mẫu quái: tên, HP, hệ, khung hình | cmd `26` (số lượng) + cmd `100` |
| `Skills.json` | Kỹ năng 5 phái, mô tả, giá học, chỉ số 11 cấp | `SKILL_NAME` + cmd `1` + cmd `35` |
| `Monsters/{id}.png` | Ảnh quái đã giải mã | cmd `-47` |
| `Icons/{id/1000}/{id}.png` | Icon vật phẩm | cmd `-51` |
| `SkillIcons/{phái}.png` | Bảng icon kỹ năng 20×20 | cmd `-8` loại 1 |

Tên và mô tả kỹ năng của cả 5 phái nằm sẵn trong gói vào game. Riêng chỉ số từng cấp (sát thương,
MP, hồi chiêu, tầm đánh) và bảng icon kỹ năng thì máy chủ chỉ gửi cho phái của nhân vật đang
đăng nhập. Muốn đủ chỉ số cả 5 phái, hãy thêm một nhân vật cho mỗi phái vào danh sách nhân vật
của workflow.

Thư mục `RawPackets/` giữ gói gốc của cmd `1`, `25` và `35` để kiểm tra khi máy chủ đổi định dạng.

## Cấu hình GitHub Actions

Tạo repository secret:

- `KPAH_TAIKHOAN`: `tài_khoản|mật_khẩu`

Mặc định workflow lần lượt đăng nhập 5 nhân vật, mỗi phái một nhân vật, bằng cùng tài khoản:

| Máy chủ | Slot | Phái |
|---|---|---|
| Vạn Xuân | 0 | Pháp sư |
| Vạn Xuân | 1 | Kiếm khách |
| Tràng An | 0 | Đấu sĩ (búa) |
| Hoàng Vương | 0 | Cung thủ |
| Tràng An | 1 | Chiến binh (đao) |

Repository variables (không bắt buộc):

- `KPAH_NHAN_VAT`: thay danh sách trên, mỗi dòng `Máy chủ|slot`, ví dụ `Vạn Xuân|0`.
- `KPAH_THUMUC`: thư mục kho dùng chung; mặc định `DuLieu`.

Tên máy chủ có thể viết có dấu hoặc không dấu. Danh sách máy chủ: Vạn Xuân, Cổ Loa, Thăng Long,
Hoa Lư, Kinh Môn, Hoàng Vương, Mỹ Sơn, Tràng An, Tiên La.

Workflow chạy tuần tự từng nhân vật vì một tài khoản chỉ vào được một nhân vật mỗi lúc. Nhân vật
nào lỗi thì được thử lại một lần, sau đó bỏ qua để không chặn các nhân vật còn lại. Job chỉ báo
lỗi khi không nhân vật nào thành công, và vẫn commit phần dữ liệu đã nhận được.

Workflow chạy hằng ngày khoảng 06:25 giờ Việt Nam. Bạn cũng có thể chạy tay trong tab Actions.

Bật trang web: **Settings → Pages → Deploy from a branch → `main` / `(root)`**.

## Chạy ở máy cá nhân

```powershell
$env:KPAH_TK = "tai_khoan"
$env:KPAH_MK = "mat_khau"
dotnet run --project HeadlessClient/DataKpah.HeadlessClient/DataKpah.HeadlessClient.csproj -c Release -- --maychu "Vạn Xuân" --nhan-vat 0 --ra .
```

Không ghi tài khoản hoặc mật khẩu trực tiếp vào repository.

## Ghi chú giao thức

Phần mạng giống dòng game TeaMobi/ME:
- Bắt tay bằng cmd `-40`. Máy chủ trả key XOR, từ đó mọi byte đều XOR với key cuộn.
- Gói lớn mở đầu bằng cmd `-128`: byte kế tiếp là lệnh thật, sau đó là 4 byte độ dài.

Trình tự đăng nhập theo đúng `LoginScr.doLogin`:
1. `cmd -1` (byte `2`).
2. `cmd 1`: `user, pass, "2.7.6", "nokia/1/1", "0", "-1", "-1", 4, chiều rộng, 0, route`.
3. Máy chủ trả danh sách nhân vật qua cmd `13`.
4. Chọn nhân vật cũng dùng cmd `13`: `byte 1` rồi `int id`.

Ảnh quái thường được đóng gói kiểu `FilePack`, XOR với chuỗi `NguyenVanMinh`, gồm hai tệp:
`{spalate}_h` (đầu PNG kèm bảng màu) và `data`. Ghép hai tệp lại là ra PNG dải dọc, mỗi khung
có kích thước rộng × cao của mẫu quái. Quái kiểu mới gửi thẳng danh sách PNG (`ImageObj`).
