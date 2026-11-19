using DemoMVC.Models;

namespace DemoMVC.ViewModels
{
    public class StudentGradesViewModel
    {
        public required Student Student { get; init; }
        public required IReadOnlyList<Grade> Grades { get; init; }
        public PagedResult<Grade>? PagedGrades { get; init; }
    }
}
