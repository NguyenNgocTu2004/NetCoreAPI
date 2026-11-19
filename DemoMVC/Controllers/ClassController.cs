using DemoMVC.Data;
using DemoMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.ViewModels;

namespace DemoMVC.Controllers
{
    [Authorize(Roles = IdentitySeeder.DataViewerRoles)]
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            var pagedClasses = await PagedResult<StudentClass>.CreateAsync(
                GetClasses(searchString).OrderBy(studentClass => studentClass.MaLop),
                page);

            ViewData["CurrentFilter"] = searchString;
            return IsAjaxRequest()
                ? PartialView("_ClassTable", pagedClasses)
                : View(pagedClasses);
        }

        [HttpGet]
        public async Task<IActionResult> List(string? searchString, int page = 1)
        {
            var pagedClasses = await PagedResult<StudentClass>.CreateAsync(
                GetClasses(searchString).OrderBy(studentClass => studentClass.MaLop),
                page);
            return PartialView("_ClassTable", pagedClasses);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentClass = await _context.Classes
                .Include(item => item.Students)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (studentClass == null)
            {
                return NotFound();
            }

            return IsAjaxRequest()
                ? PartialView("_Details", studentClass)
                : View(studentClass);
        }

        [Authorize(Roles = IdentitySeeder.DataManagerRoles)]
        [HttpGet]
        public IActionResult Create()
        {
            return IsAjaxRequest()
                ? PartialView("_Form", new StudentClass())
                : View(new StudentClass());
        }

        [Authorize(Roles = IdentitySeeder.DataManagerRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentClass studentClass)
        {
            studentClass.MaLop = studentClass.MaLop?.Trim() ?? string.Empty;
            if (await _context.Classes.AnyAsync(item => item.MaLop == studentClass.MaLop))
            {
                ModelState.AddModelError(nameof(StudentClass.MaLop), "Mã lớp đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Classes.Add(studentClass);
                await _context.SaveChangesAsync();

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Thêm lớp học thành công!" });
                }

                return RedirectToAction(nameof(Index));
            }

            return InvalidClassForm(studentClass);
        }

        [Authorize(Roles = IdentitySeeder.DataManagerRoles)]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentClass = await _context.Classes.FindAsync(id);
            if (studentClass == null)
            {
                return NotFound();
            }

            return IsAjaxRequest()
                ? PartialView("_Form", studentClass)
                : View(studentClass);
        }

        [Authorize(Roles = IdentitySeeder.DataManagerRoles)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentClass studentClass)
        {
            if (id != studentClass.Id)
            {
                return NotFound();
            }

            studentClass.MaLop = studentClass.MaLop?.Trim() ?? string.Empty;
            if (await _context.Classes.AnyAsync(item =>
                    item.MaLop == studentClass.MaLop && item.Id != studentClass.Id))
            {
                ModelState.AddModelError(nameof(StudentClass.MaLop), "Mã lớp đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                var existingClass = await _context.Classes.FindAsync(id);
                if (existingClass == null)
                {
                    return NotFound();
                }

                existingClass.MaLop = studentClass.MaLop;
                existingClass.TenLop = studentClass.TenLop;
                existingClass.NganhHoc = studentClass.NganhHoc;
                existingClass.KhoaHoc = studentClass.KhoaHoc;
                existingClass.MoTa = studentClass.MoTa;
                await _context.SaveChangesAsync();

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Cập nhật lớp học thành công!" });
                }

                return RedirectToAction(nameof(Index));
            }

            return InvalidClassForm(studentClass);
        }

        [Authorize(Roles = IdentitySeeder.DataManagerRoles)]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentClass = await _context.Classes
                .Include(item => item.Students)
                .FirstOrDefaultAsync(item => item.Id == id);
            if (studentClass == null)
            {
                return NotFound();
            }

            ViewData["IsDelete"] = true;
            return IsAjaxRequest()
                ? PartialView("_Details", studentClass)
                : View(studentClass);
        }

        [Authorize(Roles = IdentitySeeder.DataManagerRoles)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentClass = await _context.Classes.FindAsync(id);
            if (studentClass == null)
            {
                return NotFound();
            }

            if (await _context.Students.AnyAsync(student => student.ClassId == id))
            {
                const string message = "Không thể xóa lớp đang có sinh viên theo học.";
                if (IsAjaxRequest())
                {
                    return Conflict(new { message });
                }

                TempData["Error"] = message;
                return RedirectToAction(nameof(Index));
            }

            _context.Classes.Remove(studentClass);
            await _context.SaveChangesAsync();

            if (IsAjaxRequest())
            {
                return Json(new { message = "Xóa lớp học thành công!" });
            }

            return RedirectToAction(nameof(Index));
        }

        private IQueryable<StudentClass> GetClasses(string? searchString)
        {
            var query = _context.Classes.Include(item => item.Students).AsQueryable();
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return query;
            }

            var keyword = searchString.Trim();
            return query.Where(item =>
                item.MaLop.Contains(keyword) ||
                item.TenLop.Contains(keyword) ||
                item.NganhHoc.Contains(keyword));
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private IActionResult InvalidClassForm(StudentClass studentClass)
        {
            if (IsAjaxRequest())
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return PartialView("_Form", studentClass);
            }

            return View(studentClass);
        }
    }
}
