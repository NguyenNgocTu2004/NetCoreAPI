using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class StudentClass
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã lớp")]
        [StringLength(20, ErrorMessage = "Mã lớp không được quá 20 ký tự")]
        [Display(Name = "Mã lớp")]
        public string MaLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên lớp")]
        [StringLength(100, ErrorMessage = "Tên lớp không được quá 100 ký tự")]
        [Display(Name = "Tên lớp")]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập ngành học")]
        [StringLength(100, ErrorMessage = "Ngành học không được quá 100 ký tự")]
        [Display(Name = "Ngành học")]
        public string NganhHoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập khóa học")]
        [StringLength(50, ErrorMessage = "Khóa học không được quá 50 ký tự")]
        [Display(Name = "Khóa học")]
        public string KhoaHoc { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
