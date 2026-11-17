using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã sinh viên")]
        [StringLength(10, MinimumLength = 10,
            ErrorMessage = "Mã sinh viên phải có đúng 10 ký tự")]
        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "Mã sinh viên phải gồm đúng 10 chữ số")]
        [Display(Name = "Mã sinh viên")]
        public string StudentCode { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng nhập tên trường")]
        [StringLength(100,
            ErrorMessage = "Tên trường không được quá 100 ký tự")]
        [Display(Name = "Trường")]
        public string School { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(200,
            ErrorMessage = "Địa chỉ không được quá 200 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(254, ErrorMessage = "Email không được quá 254 ký tự")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateOnly? NgaySinh { get; set; }

        [StringLength(20, ErrorMessage = "Giới tính không được quá 20 ký tự")]
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [Display(Name = "Lớp")]
        public int? ClassId { get; set; }

        public StudentClass? Class { get; set; }
    }
}