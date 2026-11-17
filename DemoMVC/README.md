# DemoMVC

## Đăng nhập và phân quyền

- Các trang nghiệp vụ yêu cầu đăng nhập; `/Account/Login` và `/Account/Register` cho phép truy cập công khai.
- `Admin` quản lý tài khoản và có thể tạo, đổi vai trò hoặc xóa tài khoản `DP`/`User`. Tài khoản Admin và tài khoản Admin đang đăng nhập không thể bị đổi vai trò hoặc xóa từ giao diện quản lý.
- `DP` được xem danh sách tài khoản và được thêm, sửa, xem, xóa sinh viên, lớp học, môn học và điểm.
- Người tự đăng ký luôn được gán vai trò `User`; vai trò không thể chọn từ form đăng ký. `User` được xem sinh viên, lớp học, môn học và điểm ở chế độ chỉ đọc; không thể thêm, sửa, xóa hoặc truy cập mục Tài khoản.
- Vai trò cũ `Staff` được chuyển sang `DP` khi ứng dụng khởi động.
- Tài khoản Admin đầu tiên được khởi tạo từ cấu hình bảo mật, không dùng mật khẩu mặc định.

Chạy các lệnh sau từ thư mục gốc repository. Trước lần chạy đầu tiên, cập nhật database:

```powershell
dotnet ef database update --project DemoMVC\DemoMVC.csproj
```

Thiết lập tài khoản Admin ban đầu bằng .NET User Secrets (thay `<mật-khẩu-mạnh>` bằng mật khẩu của bạn; giá trị được lưu ngoài repository):

```powershell
dotnet user-secrets set "Authentication:Admin:Username" "admin" --project DemoMVC\DemoMVC.csproj
dotnet user-secrets set "Authentication:Admin:Email" "admin@example.com" --project DemoMVC\DemoMVC.csproj
dotnet user-secrets set "Authentication:Admin:Password" "<mật-khẩu-mạnh>" --project DemoMVC\DemoMVC.csproj
dotnet watch --project DemoMVC\DemoMVC.csproj
```

Ba giá trị trên chỉ bắt buộc khi database chưa có tài khoản Admin. Sau khi Admin đã được tạo, ứng dụng vẫn khởi động nếu không nạp lại cấu hình bootstrap, miễn là database còn ít nhất một Admin. Mật khẩu cần đáp ứng chính sách mật khẩu Identity của ứng dụng. Có thể dùng biến môi trường `Authentication__Admin__Username`, `Authentication__Admin__Email` và `Authentication__Admin__Password` thay cho User Secrets.

Đăng ký tại `/Account/Register` hoặc đăng nhập tại `/Account/Login`. Sau khi đăng nhập bằng Admin, mở mục **Tài khoản** để tạo và quản lý tài khoản.
