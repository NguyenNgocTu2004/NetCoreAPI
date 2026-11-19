using DemoMVC.Data;
using DemoMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers;

[Authorize(Roles = IdentitySeeder.AdminRole + "," + IdentitySeeder.DepartmentRole)]
public class UsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UsersController(
        UserManager<IdentityUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? searchString,
        string? roleFilter,
        string? statusFilter,
        int page = 1)
    {
        var supportedRoles = new[]
        {
            IdentitySeeder.AdminRole,
            IdentitySeeder.DepartmentRole,
            IdentitySeeder.UserRole
        };
        if (!string.IsNullOrWhiteSpace(roleFilter) &&
            !supportedRoles.Contains(roleFilter, StringComparer.Ordinal))
        {
            return BadRequest();
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) &&
            statusFilter != "active" && statusFilter != "locked")
        {
            return BadRequest();
        }

        var usersQuery = _context.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var search = searchString.Trim();
            usersQuery = usersQuery.Where(user =>
                (user.UserName != null && user.UserName.Contains(search)) ||
                (user.Email != null && user.Email.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            usersQuery = from user in usersQuery
                        join userRole in _context.UserRoles on user.Id equals userRole.UserId
                        join role in _context.Roles on userRole.RoleId equals role.Id
                        where role.Name == roleFilter
                        select user;
        }

        if (statusFilter == "active")
        {
            usersQuery = usersQuery.Where(user =>
                !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow);
        }
        else if (statusFilter == "locked")
        {
            usersQuery = usersQuery.Where(user =>
                user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow);
        }

        var adminIds = _context.UserRoles
            .Where(userRole => _context.Roles
                .Any(role => role.Id == userRole.RoleId && role.Name == IdentitySeeder.AdminRole))
            .Select(userRole => userRole.UserId);
        var departmentIds = _context.UserRoles
            .Where(userRole => _context.Roles
                .Any(role => role.Id == userRole.RoleId && role.Name == IdentitySeeder.DepartmentRole))
            .Select(userRole => userRole.UserId);

        var pagedUsers = await PagedResult<IdentityUser>.CreateAsync(
            usersQuery
                .OrderBy(user => adminIds.Contains(user.Id) ? 0 :
                    departmentIds.Contains(user.Id) ? 1 : 2)
                .ThenBy(user => user.UserName),
            page);
        var currentUserId = _userManager.GetUserId(User);
        var managedUsers = new List<ManagedUserViewModel>(pagedUsers.Items.Count);

        foreach (var user in pagedUsers.Items)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var selectedRole = roles.Contains(IdentitySeeder.AdminRole, StringComparer.Ordinal)
                ? IdentitySeeder.AdminRole
                : roles.Contains(IdentitySeeder.DepartmentRole, StringComparer.Ordinal)
                    ? IdentitySeeder.DepartmentRole
                    : IdentitySeeder.UserRole;

            managedUsers.Add(new ManagedUserViewModel
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                Roles = roles.Count == 0 ? "Chưa gán" : string.Join(", ", roles),
                SelectedRole = selectedRole,
                IsAdmin = roles.Contains(IdentitySeeder.AdminRole, StringComparer.Ordinal),
                IsCurrentUser = user.Id == currentUserId,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
            });
        }

        ViewData["CurrentFilter"] = searchString;
        ViewData["RoleFilter"] = roleFilter;
        ViewData["StatusFilter"] = statusFilter;
        var result = new PagedResult<ManagedUserViewModel>
        {
            Items = managedUsers,
            CurrentPage = pagedUsers.CurrentPage,
            PageSize = pagedUsers.PageSize,
            TotalItems = pagedUsers.TotalItems,
            TotalPages = pagedUsers.TotalPages
        };
        return View(result);
    }

    [Authorize(Roles = IdentitySeeder.AdminRole)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateUserViewModel());
    }

    [Authorize(Roles = IdentitySeeder.AdminRole)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        model.Username = model.Username?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
        if (!IsManageableRole(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Chỉ có thể tạo tài khoản DP hoặc User.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Username,
            Email = model.Email,
            EmailConfirmed = true
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();
        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync();
            AddIdentityErrors(createResult);
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync();
            AddIdentityErrors(roleResult);
            return View(model);
        }

        await transaction.CommitAsync();
        TempData["Success"] = "Tạo tài khoản thành công.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = IdentitySeeder.AdminRole)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string? id, string? role)
    {
        if (string.IsNullOrWhiteSpace(id) || !IsManageableRole(role))
        {
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Không thể thay đổi vai trò của tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(IdentitySeeder.AdminRole, StringComparer.Ordinal))
        {
            TempData["Error"] = "Không thể thay đổi vai trò của tài khoản Admin duy nhất.";
            return RedirectToAction(nameof(Index));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        if (!roles.Contains(role!, StringComparer.Ordinal))
        {
            var addResult = await _userManager.AddToRoleAsync(user, role!);
            if (!addResult.Succeeded)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = FormatIdentityErrors(addResult);
                return RedirectToAction(nameof(Index));
            }
        }

        var rolesToRemove = roles
            .Where(existingRole => existingRole != IdentitySeeder.AdminRole && existingRole != role)
            .ToArray();
        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = FormatIdentityErrors(removeResult);
                return RedirectToAction(nameof(Index));
            }
        }

        await transaction.CommitAsync();
        TempData["Success"] = "Đã cập nhật vai trò tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = IdentitySeeder.AdminRole)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(IdentitySeeder.AdminRole, StringComparer.Ordinal))
        {
            TempData["Error"] = "Không thể xóa tài khoản Admin duy nhất.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["Error"] = FormatIdentityErrors(result);
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Đã xóa tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private static bool IsManageableRole(string? role)
    {
        return role == IdentitySeeder.DepartmentRole || role == IdentitySeeder.UserRole;
    }

    private static string FormatIdentityErrors(IdentityResult result)
    {
        return string.Join(" ", result.Errors.Select(error => error.Description));
    }
}
