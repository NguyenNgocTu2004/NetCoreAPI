using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;

namespace DemoMVC.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhanVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var nhanViens = await _context.NhanViens.ToListAsync();
            return IsAjaxRequest()
                ? PartialView("_NhanVienTable", nhanViens)
                : View(nhanViens);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            return PartialView(
                "_NhanVienTable",
                await _context.NhanViens.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return IsAjaxRequest()
                ? PartialView("_Details", nhanVien)
                : View(nhanVien);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return IsAjaxRequest()
                ? PartialView("_Form", new NhanVien())
                : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MaNhanVien,HoTen,ChucVu,PhongBan,Luong")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Thêm nhân viên thành công!" });
                }

                return RedirectToAction(nameof(Index));
            }

            return InvalidNhanVienForm(nhanVien);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }
            return IsAjaxRequest()
                ? PartialView("_Form", nhanVien)
                : View(nhanVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaNhanVien,HoTen,ChucVu,PhongBan,Luong")] NhanVien nhanVien)
        {
            if (id != nhanVien.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingNhanVien = await _context.NhanViens.FindAsync(id);
                    if (existingNhanVien == null)
                    {
                        return NotFound();
                    }

                    existingNhanVien.MaNhanVien = nhanVien.MaNhanVien;
                    existingNhanVien.HoTen = nhanVien.HoTen;
                    existingNhanVien.ChucVu = nhanVien.ChucVu;
                    existingNhanVien.PhongBan = nhanVien.PhongBan;
                    existingNhanVien.Luong = nhanVien.Luong;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        if (IsAjaxRequest())
                        {
                            return StatusCode(
                                StatusCodes.Status409Conflict,
                                new { message = "Bản ghi đã được thay đổi bởi một thao tác khác." });
                        }

                        throw;
                    }
                }

                if (IsAjaxRequest())
                {
                    return Json(new { message = "Cập nhật nhân viên thành công!" });
                }

                return RedirectToAction(nameof(Index));
            }

            return InvalidNhanVienForm(nhanVien);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            ViewData["IsDelete"] = true;
            return IsAjaxRequest()
                ? PartialView("_Details", nhanVien)
                : View(nhanVien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            _context.NhanViens.Remove(nhanVien);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException) when (IsAjaxRequest())
            {
                return StatusCode(
                    StatusCodes.Status409Conflict,
                    new { message = "Không thể xóa nhân viên vì dữ liệu này đang được sử dụng." });
            }

            if (IsAjaxRequest())
            {
                return Json(new { message = "Xóa nhân viên thành công!" });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(int id)
        {
            return _context.NhanViens.Any(e => e.Id == id);
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private IActionResult InvalidNhanVienForm(NhanVien nhanVien)
        {
            if (IsAjaxRequest())
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return PartialView("_Form", nhanVien);
            }

            return View(nhanVien);
        }
    }
}
