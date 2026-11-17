using DemoMVC.Models;

namespace DemoMVC.Services
{
    public class GradeCalculator
    {
        public void Recalculate(Grade grade)
        {
            grade.DiemTongKet = Math.Round(
                grade.DiemQuaTrinh * 0.4m + grade.DiemThi * 0.6m,
                2,
                MidpointRounding.AwayFromZero);

            grade.XepLoai = grade.DiemTongKet switch
            {
                >= 8.5m => "Giỏi",
                >= 7.0m => "Khá",
                >= 5.0m => "Trung bình",
                _ => "Yếu"
            };
        }
    }
}
