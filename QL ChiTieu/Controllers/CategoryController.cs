
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_ChiTieu.Models;

using QL_ChiTieu.Controllers;

namespace Expense_Tracker.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;


        public CategoryController(ApplicationDbContext context)           
        {
            _context = context;                       
        }




        public int GetLoggedInUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            // Trả về giá trị mặc định nếu không tìm thấy userId hoặc không thể chuyển đổi thành kiểu int.
            return 0;
        }
        // GET: Category

        // GET: Category

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var idUserFromSession = HttpContext.Session.GetString("idUser");

                if (!string.IsNullOrEmpty(idUserFromSession))
                {
                    if (int.TryParse(idUserFromSession, out int userId))
                    {
                        var categories = await _context.Categories
                            .Where(c => c.idUser == userId)
                            .ToListAsync();

                        return View(categories);
                    }
                    else
                    {
                        // Xử lý trường hợp khi idUserFromSession không thể chuyển đổi thành số nguyên
                        ViewBag.Error = "Lỗi: Id người dùng không hợp lệ.";
                        return RedirectToAction("Error", "Shared");
                    }
                }
                else
                {
                    // Xử lý trường hợp khi idUserFromSession là null hoặc rỗng
                    ViewBag.Error = "Lỗi: Id người dùng không tồn tại.";
                    return RedirectToAction("Error", "Shared");
                }
            }
        }

        // GET: Category/AddOrEdit
        public IActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new Category() { idUser = GetLoggedInUserId() });
            else
                return View(_context.Categories.Find(id));
        }


        // POST: Category/AddOrEdit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("CategoryId,Title,Icon,Type,idUser")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.CategoryId == 0)
                {
                    var idUserFromSession = HttpContext.Session.GetString("idUser");

                    if (!string.IsNullOrEmpty(idUserFromSession) && int.TryParse(idUserFromSession, out int userId))
                    {
                        category.idUser = userId;
                        _context.Add(category);
                    }
                    else
                    {
                        ViewBag.Error = "Lỗi: Id người dùng không hợp lệ.";
                        return RedirectToAction("Error", "Shared");
                    }
                }
                else
                {
                    var originalCategory = _context.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);
                    if (originalCategory != null)
                        category.idUser = originalCategory.idUser;
                    _context.Update(category);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }




        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
