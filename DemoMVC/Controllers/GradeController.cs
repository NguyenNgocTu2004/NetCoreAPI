using DemoMVC.Data;
using DemoMVC.Models;
using DemoMVC.Services;
using DemoMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    [Authorize(Roles = IdentitySeeder.AdminRole + "," + IdentitySeeder.DepartmentRole)]
    public class GradeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GradeCalculator _gradeCalculator;

        public GradeController(
            ApplicationDbContext context,
            GradeCalculator gradeCalculator)
        {
            _context = context;
            _gradeCalculator = gradeCalculator;
        }

        public async Task<IActionResult> Index(
            string? searchString,
            int? classId,
            int? subjectId)
        {
            var grades = await GetGrades(searchString, classId, subjectId)
                .OrderBy(grade => grade.Student!.StudentCode)
                .ThenBy(grade => grade.Subject!.MaMonHoc)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            ViewData["SelectedClassId"] = classId;
            ViewData["SelectedSubjectId"] = subjectId;
            PopulateGradeFilters(classId, subjectId);

            return IsAjaxRequest()
                ? PartialView("_GradeTable", grades)
                : View(grades);
        }

        [HttpGet]
        public async Task<IActionResult> List(
            string? searchString,
            int? classId,
            int? subjectId)
        {
            var grades = await GetGrades(searchString, classId, subjectId)
                .OrderBy(grade => grade.Student!.StudentCode)
                .ThenBy(grade => grade.Subject!.MaMonHoc)
                .ToListAsync();
            return PartialView("_GradeTable", grades);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(item => item.Student)
                .Include(item => item.Subject)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (grade == null)
            {
                return NotFound();
            }

            return IsAjaxRequest()
                ? PartialView("_Details", grade)
                : View(grade);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateGradeFormOptions();
            return IsAjaxRequest()
                ? PartialView("_Form", new Grade())
                : View(new Grade());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Grade grade)
        {
            await ValidateGradeAsync(grade);
            if (ModelState.IsValid)
            {
                _gradeCalculator.Recalculate(grade);
                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Thêm điểm thành công!" });
                }

                return RedirectToAction(nameof(Index));
            }

            return InvalidGradeForm(grade);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                return NotFound();
            }

            PopulateGradeFormOptions(grade.StudentId, grade.SubjectId);
            return IsAjaxRequest()
                ? PartialView("_Form", grade)
                : View(grade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Grade grade)
        {
            if (id != grade.Id)
            {
                return NotFound();
            }

            await ValidateGradeAsync(grade, id);
            if (ModelState.IsValid)
            {
                var existingGrade = await _context.Grades.FindAsync(id);
                if (existingGrade == null)
                {
                    return NotFound();
                }

                existingGrade.StudentId = grade.StudentId;
                existingGrade.SubjectId = grade.SubjectId;
                existingGrade.DiemQuaTrinh = grade.DiemQuaTrinh;
                existingGrade.DiemThi = grade.DiemThi;
                _gradeCalculator.Recalculate(existingGrade);
                await _context.SaveChangesAsync();

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Cập nhật điểm thành công!" });
                }

                return RedirectToAction(nameof(Index));
            }

            return InvalidGradeForm(grade);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(item => item.Student)
                .Include(item => item.Subject)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (grade == null)
            {
                return NotFound();
            }

            ViewData["IsDelete"] = true;
            return IsAjaxRequest()
                ? PartialView("_Details", grade)
                : View(grade);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                return NotFound();
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();

            if (IsAjaxRequest())
            {
                return Json(new { message = "Xóa điểm thành công!" });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ByStudent(int studentId)
        {
            var student = await _context.Students
                .Include(item => item.Class)
                .FirstOrDefaultAsync(item => item.Id == studentId);
            if (student == null)
            {
                return NotFound();
            }

            var grades = await _context.Grades
                .Include(grade => grade.Subject)
                .Where(grade => grade.StudentId == studentId)
                .OrderBy(grade => grade.Subject!.MaMonHoc)
                .ToListAsync();

            return View(new StudentGradesViewModel
            {
                Student = student,
                Grades = grades
            });
        }

        private IQueryable<Grade> GetGrades(
            string? searchString,
            int? classId,
            int? subjectId)
        {
            var query = _context.Grades
                .Include(grade => grade.Student)
                    .ThenInclude(student => student!.Class)
                .Include(grade => grade.Subject)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim();
                query = query.Where(grade =>
                    grade.Student!.StudentCode.Contains(keyword) ||
                    grade.Student.FullName.Contains(keyword) ||
                    (grade.Student.Email != null && grade.Student.Email.Contains(keyword)) ||
                    grade.Subject!.MaMonHoc.Contains(keyword) ||
                    grade.Subject.TenMonHoc.Contains(keyword));
            }

            if (classId.HasValue)
            {
                query = query.Where(grade => grade.Student!.ClassId == classId.Value);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(grade => grade.SubjectId == subjectId.Value);
            }

            return query;
        }

        private async Task ValidateGradeAsync(Grade grade, int? excludeId = null)
        {
            if (!await _context.Students.AnyAsync(student => student.Id == grade.StudentId))
            {
                ModelState.AddModelError(nameof(Grade.StudentId), "Sinh viên đã chọn không tồn tại.");
            }

            if (!await _context.Subjects.AnyAsync(subject => subject.Id == grade.SubjectId))
            {
                ModelState.AddModelError(nameof(Grade.SubjectId), "Môn học đã chọn không tồn tại.");
            }

            if (grade.StudentId > 0 && grade.SubjectId > 0 &&
                await _context.Grades.AnyAsync(item =>
                    item.StudentId == grade.StudentId &&
                    item.SubjectId == grade.SubjectId &&
                    item.Id != excludeId))
            {
                ModelState.AddModelError(
                    nameof(Grade.SubjectId),
                    "Sinh viên đã có điểm của môn học này.");
            }

            if (decimal.Round(grade.DiemQuaTrinh, 2) != grade.DiemQuaTrinh)
            {
                ModelState.AddModelError(
                    nameof(Grade.DiemQuaTrinh),
                    "Điểm chỉ được nhập tối đa 2 chữ số thập phân.");
            }

            if (decimal.Round(grade.DiemThi, 2) != grade.DiemThi)
            {
                ModelState.AddModelError(
                    nameof(Grade.DiemThi),
                    "Điểm chỉ được nhập tối đa 2 chữ số thập phân.");
            }
        }

        private void PopulateGradeFormOptions(int? studentId = null, int? subjectId = null)
        {
            ViewData["StudentOptions"] = _context.Students
                .OrderBy(student => student.StudentCode)
                .Select(student => new SelectListItem
                {
                    Value = student.Id.ToString(),
                    Text = student.StudentCode + " - " + student.FullName,
                    Selected = student.Id == studentId
                })
                .ToList();

            ViewData["SubjectOptions"] = new SelectList(
                _context.Subjects.OrderBy(subject => subject.TenMonHoc),
                nameof(Subject.Id),
                nameof(Subject.TenMonHoc),
                subjectId);
        }

        private void PopulateGradeFilters(int? classId, int? subjectId)
        {
            ViewData["ClassFilterOptions"] = new SelectList(
                _context.Classes.OrderBy(studentClass => studentClass.MaLop),
                nameof(StudentClass.Id),
                nameof(StudentClass.TenLop),
                classId);

            ViewData["SubjectFilterOptions"] = new SelectList(
                _context.Subjects.OrderBy(subject => subject.MaMonHoc),
                nameof(Subject.Id),
                nameof(Subject.MaMonHoc),
                subjectId);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private IActionResult InvalidGradeForm(Grade grade)
        {
            PopulateGradeFormOptions(grade.StudentId, grade.SubjectId);
            if (IsAjaxRequest())
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return PartialView("_Form", grade);
            }

            return View(grade);
        }
    }
}
