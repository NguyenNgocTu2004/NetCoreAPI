
# cấu trúc thư mục của dự án .Net MVC
###
        Dự án .NET MVC được xây dựng theo mô hình MVC (Model – View – Controller)
        Cấu trúc chính được tổ chức theo thứ tự Models → Controllers → Views

        *Models
            -Chứa các lớp làm việc với dữ liệu
            -Đại diện cho các bảng trong cơ sở dữ liệu
            -Chứa các lớp Entity, DTO, ViewModel
            -Thực hiện truy vấn, thêm, sửa, xóa dữ liệu (thường kết hợp với Entity Framework, LINQ)
            -Không xử lý giao diện

        *Controllers
            -Chứa các lớp điều khiển luồng xử lý của chương trình
            -Nhận request từ người dùng thông qua URL
            -Gọi Models để xử lý nghiệp vụ và lấy dữ liệu
            -Truyền dữ liệu sang Views để hiển thị
            -Mỗi Controller thường tương ứng với một chức năng trong hệ thống
            -Các phương thức trong Controller được gọi là Action

        *Views
            -Chứa các mã nguồn giao diện người dùng
            -Sử dụng Razor (.cshtml) kết hợp HTML và C#
            -Nhận dữ liệu từ Controller để hiển thị
            -Mỗi thư mục View tương ứng với một Controller
            -Thư mục Shared chứa các giao diện dùng chung (Layout, Header, Footer)

        *wwwroot
            -Chứa các tài nguyên tĩnh của ứng dụng
            -Bao gồm CSS, JavaScript, hình ảnh, font chữ
            -Là thư mục duy nhất trình duyệt có thể truy cập trực tiếp

        *Properties
            -Chứa các tệp cấu hình hệ thống
            -Thường dùng để cấu hình môi trường chạy và cổng ứng dụng
            *appsettings.json
            -Chứa các cấu hình chung của ứng dụng
            -Lưu chuỗi kết nối cơ sở dữ liệu
            -Cấu hình logging và các tham số hệ thống khác

        *Program.cs / Startup.cs
            -Cấu hình và khởi tạo ứng dụng
            -Đăng ký các dịch vụ cần thiết
            -Cấu hình middleware và routing cho MVC
###

# ROUTE trong .NET MVC
###
    Định tuyến (Routing):

    Là cơ chế ánh xạ URL từ trình duyệt tới Controller và Action tương ứng
    Giúp xác định yêu cầu của người dùng sẽ được xử lý ở đâu trong ứng dụng

    Vai trò của Route:
        -Điều hướng request đúng Controller và Action
        -Giúp URL rõ ràng, dễ đọc, thân thiện với người dùng và SEO
        -Tách biệt URL với cấu trúc vật lý của thư mục

    Cấu trúc URL mặc định:
        {controller}/{action}/{id}
        Trong đó:
            -controller: tên Controller
            -action: tên phương thức trong Controller
            -id: tham số (không bắt buộc)
            -Route mặc định trong .NET MVC:
            -Khi không chỉ định rõ URL, hệ thống tự động gọi:
            -Controller mặc định: Home

    Action mặc định: Index
        -Định tuyến truyền thống (Conventional Routing):
        -Được cấu hình tập trung trong Program.cs hoặc Startup.cs
        -Sử dụng mẫu URL chung cho toàn bộ ứng dụng
        -Dễ sử dụng cho các dự án nhỏ và vừa
        -Định tuyến theo thuộc tính (Attribute Routing):
        -Khai báo trực tiếp trên Controller hoặc Action bằng [Route]
        -Cho phép tùy biến URL linh hoạt
        -Dễ quản lý với các API hoặc chức năng phức tạp

    Tham số trong Route:
        -Cho phép truyền dữ liệu qua URL
        -Có thể là bắt buộc hoặc không bắt buộc
        -Hỗ trợ kiểu dữ liệu như int, string
        -Thứ tự ưu tiên của Route:
        -Route được khai báo trước sẽ được kiểm tra trước
        -Nếu trùng URL, route đầu tiên phù hợp sẽ được sử dụng

    Kết luận:
        -Routing là thành phần cốt lõi trong .NET MVC
        -Giúp kết nối URL với logic xử lý của ứng dụng
        -Việc hiểu rõ Routing giúp xây dựng ứng dụng web rõ ràng, dễ mở rộng và dễ bảo trì
###
