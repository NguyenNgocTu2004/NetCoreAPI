using DemoMVC.Data;
using DemoMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    [Authorize(Roles = IdentitySeeder.AdminRole + "," + IdentitySeeder.DepartmentRole)]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var studentsQuery = GetStudents(searchString);

            var students = await studentsQuery
                .OrderBy(s => s.Id)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            if (IsAjaxRequest())
            {
                return PartialView("_StudentTable", students);
            }

            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> List(string? searchString)
        {
            var students = await GetStudents(searchString)
                .OrderBy(s => s.Id)
                .ToListAsync();

            return PartialView("_StudentTable", students);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return IsAjaxRequest()
                ? PartialView("_Details", student)
                : View(student);
        }


        [HttpGet]
        public IActionResult Create()
        {
            PopulateClassSelectList();
            return IsAjaxRequest()
                ? PartialView("_Form", new Student())
                : View(new Student());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            await ValidateClassAsync(student.ClassId, required: true);
            if (ModelState.IsValid)
            {
                student.StudentCode = student.StudentCode?.Trim() ?? string.Empty;
                student.Email = student.Email?.Trim();

                var studentCodeExists = await _context.Students
                    .AnyAsync(s => s.StudentCode == student.StudentCode);

                if (studentCodeExists)
                {
                    ModelState.AddModelError(nameof(Student.StudentCode), "Mã sinh viên đã tồn tại.");
                    return InvalidStudentForm(student);
                }

                _context.Students.Add(student);

                await _context.SaveChangesAsync();

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Thêm sinh viên thành công!" });
                }

                TempData["Success"] = "Thêm sinh viên thành công!";

                return RedirectToAction(nameof(Index));
            }

            return InvalidStudentForm(student);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            PopulateClassSelectList(student.ClassId);
            return IsAjaxRequest()
                ? PartialView("_Form", student)
                : View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            await ValidateClassAsync(student.ClassId, required: false);
            if (ModelState.IsValid)
            {
                student.StudentCode = student.StudentCode?.Trim() ?? string.Empty;

                var studentCodeExists = await _context.Students
                    .AnyAsync(s => s.StudentCode == student.StudentCode && s.Id != student.Id);

                if (studentCodeExists)
                {
                    ModelState.AddModelError(nameof(Student.StudentCode), "Mã sinh viên đã tồn tại.");
                    return InvalidStudentForm(student);
                }

                var existingStudent = await _context.Students.FindAsync(id);
                if (existingStudent == null)
                {
                    return NotFound();
                }

                existingStudent.StudentCode = student.StudentCode;
                existingStudent.FullName = student.FullName;
                existingStudent.School = student.School;
                existingStudent.Address = student.Address;
                existingStudent.Email = student.Email?.Trim();
                existingStudent.NgaySinh = student.NgaySinh;
                existingStudent.GioiTinh = student.GioiTinh;
                existingStudent.ClassId = student.ClassId;

                await _context.SaveChangesAsync();

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Cập nhật sinh viên thành công!" });
                }

                TempData["Success"] = "Cập nhật sinh viên thành công!";

                return RedirectToAction(nameof(Index));
            }

            return InvalidStudentForm(student);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            ViewData["IsDelete"] = true;
            return IsAjaxRequest()
                ? PartialView("_Details", student)
                : View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            if (await _context.Grades.AnyAsync(grade => grade.StudentId == id))
            {
                const string message = "Không thể xóa sinh viên đang có dữ liệu điểm.";
                if (IsAjaxRequest())
                {
                    return Conflict(new { message });
                }

                TempData["Error"] = message;
                return RedirectToAction(nameof(Index));
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            if (IsAjaxRequest())
            {
                return Json(new { message = "Xóa sinh viên thành công!" });
            }

            TempData["Success"] = "Xóa sinh viên thành công!";

            return RedirectToAction(nameof(Index));
        }

        private IQueryable<Student> GetStudents(string? searchString)
        {
            var studentsQuery = _context.Students
                .Include(s => s.Class)
                .AsQueryable();
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return studentsQuery;
            }

            var keyword = searchString.Trim();
            return studentsQuery.Where(s =>
                s.StudentCode.Contains(keyword) ||
                s.FullName.Contains(keyword) ||
                (s.Email != null && s.Email.Contains(keyword)) ||
                s.School.Contains(keyword) ||
                s.Address.Contains(keyword) ||
                (s.Class != null && s.Class.TenLop.Contains(keyword)));
        }

        private void PopulateClassSelectList(int? selectedClassId = null)
        {
            ViewData["ClassId"] = new SelectList(
                _context.Classes.OrderBy(studentClass => studentClass.TenLop),
                nameof(StudentClass.Id),
                nameof(StudentClass.TenLop),
                selectedClassId);
        }

        private async Task ValidateClassAsync(int? classId, bool required)
        {
            if (!classId.HasValue)
            {
                if (required)
                {
                    ModelState.AddModelError(nameof(Student.ClassId), "Vui lòng chọn lớp.");
                }

                return;
            }

            if (!await _context.Classes.AnyAsync(studentClass => studentClass.Id == classId.Value))
            {
                ModelState.AddModelError(nameof(Student.ClassId), "Lớp đã chọn không tồn tại.");
            }
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private IActionResult InvalidStudentForm(Student student)
        {
            PopulateClassSelectList(student.ClassId);
            if (IsAjaxRequest())
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return PartialView("_Form", student);
            }

            return View(student);
        }
    }
}