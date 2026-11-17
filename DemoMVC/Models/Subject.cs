using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã môn học")]
        [StringLength(20, ErrorMessage = "Mã môn học không được quá 20 ký tự")]
        [Display(Name = "Mã môn học")]
        public string MaMonHoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên môn học")]
        [StringLength(150, ErrorMessage = "Tên môn học không được quá 150 ký tự")]
        [Display(Name = "Tên môn học")]
        public string TenMonHoc { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số tín chỉ phải là số nguyên dương")]
        [Display(Name = "Số tín chỉ")]
        public int SoTinChi { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
