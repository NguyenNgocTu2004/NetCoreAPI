using DemoMVC.Data;
using DemoMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    [Authorize(Roles = IdentitySeeder.AdminRole + "," + IdentitySeeder.DepartmentRole)]
    public class SubjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString)
        {
            var subjects = await GetSubjects(searchString)
                .OrderBy(subject => subject.MaMonHoc)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return IsAjaxRequest()
                ? PartialView("_SubjectTable", subjects)
                : View(subjects);
        }

        [HttpGet]
        public async Task<IActionResult> List(string? searchString)
        {
            var subjects = await GetSubjects(searchString)
                .OrderBy(subject => subject.MaMonHoc)
                .ToListAsync();
            return PartialView("_SubjectTable", subjects);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .Include(item => item.Grades)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            return IsAjaxRequest()
                ? PartialView("_Details", subject)
                : View(subject);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return IsAjaxRequest()
                ? PartialView("_Form", new Subject())
                : View(new Subject());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subject subject)
        {
            subject.MaMonHoc = subject.MaMonHoc?.Trim() ?? string.Empty;
            if (await _context.Subjects.AnyAsync(item => item.MaMonHoc == subject.MaMonHoc))
            {
                ModelState.AddModelError(nameof(Subject.MaMonHoc), "Mã môn học đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Thêm môn học thành công!" });
                }

                return RedirectToAction(nameof(Index));
            }

            return InvalidSubjectForm(subject);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            return IsAjaxRequest()
                ? PartialView("_Form", subject)
                : View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subject subject)
        {
            if (id != subject.Id)
            {
                return NotFound();
            }

            subject.MaMonHoc = subject.MaMonHoc?.Trim() ?? string.Empty;
            if (await _context.Subjects.AnyAsync(item =>
                    item.MaMonHoc == subject.MaMonHoc && item.Id != subject.Id))
            {
                ModelState.AddModelError(nameof(Subject.MaMonHoc), "Mã môn học đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                var existingSubject = await _context.Subjects.FindAsync(id);
                if (existingSubject == null)
                {
                    return NotFound();
                }

                existingSubject.MaMonHoc = subject.MaMonHoc;
                existingSubject.TenMonHoc = subject.TenMonHoc;
                existingSubject.SoTinChi = subject.SoTinChi;
                existingSubject.MoTa = subject.MoTa;
                await _context.SaveChangesAsync();

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Cập nhật môn học thành công!" });
                }

                return RedirectToAction(nameof(Index));
            }

            return InvalidSubjectForm(subject);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .Include(item => item.Grades)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            ViewData["IsDelete"] = true;
            return IsAjaxRequest()
                ? PartialView("_Details", subject)
                : View(subject);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            if (await _context.Grades.AnyAsync(grade => grade.SubjectId == id))
            {
                const string message = "Không thể xóa môn học đang có dữ liệu điểm.";
                if (IsAjaxRequest())
                {
                    return Conflict(new { message });
                }

                TempData["Error"] = message;
                return RedirectToAction(nameof(Index));
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            if (IsAjaxRequest())
            {
                return Json(new { message = "Xóa môn học thành công!" });
            }

            return RedirectToAction(nameof(Index));
        }

        private IQueryable<Subject> GetSubjects(string? searchString)
        {
            var query = _context.Subjects.AsQueryable();
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return query;
            }

            var keyword = searchString.Trim();
            return query.Where(item =>
                item.MaMonHoc.Contains(keyword) ||
                item.TenMonHoc.Contains(keyword));
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private IActionResult InvalidSubjectForm(Subject subject)
        {
            if (IsAjaxRequest())
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return PartialView("_Form", subject);
            }

            return View(subject);
        }
    }
}
