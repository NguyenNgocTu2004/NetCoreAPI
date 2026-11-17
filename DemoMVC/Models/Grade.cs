using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class Grade
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sinh viên")]
        [Display(Name = "Sinh viên")]
        public int StudentId { get; set; }

        public Student? Student { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học")]
        [Display(Name = "Môn học")]
        public int SubjectId { get; set; }

        public Subject? Subject { get; set; }

        [Range(typeof(decimal), "0", "10", ErrorMessage = "Điểm quá trình phải từ 0 đến 10")]
        [Display(Name = "Điểm quá trình")]
        public decimal DiemQuaTrinh { get; set; }

        [Range(typeof(decimal), "0", "10", ErrorMessage = "Điểm thi phải từ 0 đến 10")]
        [Display(Name = "Điểm thi")]
        public decimal DiemThi { get; set; }

        [Display(Name = "Điểm tổng kết")]
        public decimal DiemTongKet { get; set; }

        [StringLength(20)]
        [Display(Name = "Xếp loại")]
        public string XepLoai { get; set; } = "Yếu";
    }
}
