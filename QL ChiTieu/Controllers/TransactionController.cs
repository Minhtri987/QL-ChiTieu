using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MathNet.Numerics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QL_ChiTieu.Models;

namespace Expense_Tracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
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

        // GET: Transaction
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
                        var transactions = await _context.Transactions
                            .Where(c => c.idUser == userId)
                            .Include(t => t.Category) // Bổ sung Include ở đây để tải thông tin Category
                            .ToListAsync();

                        return View(transactions);
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


        // GET: Transaction/AddOrEdit
        public IActionResult AddOrEdit(int id = 0)
        {
            PopulateCategories();
            if (id == 0)
                return View(new Transaction() { idUser = GetLoggedInUserId() });
            else
                return View(_context.Transactions.Find(id));
        }

        // POST: Transaction/AddOrEdit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {

            {
                if (ModelState.IsValid)
                {

                    if (transaction.TransactionId == 0)
                    {

                        var idUserFromSession = HttpContext.Session.GetString("idUser");

                        if (!string.IsNullOrEmpty(idUserFromSession) && int.TryParse(idUserFromSession, out int userId))
                        {
                            transaction.idUser = userId;
                            _context.Update(transaction);

                        }
                        else
                        {
                            ViewBag.Error = "Lỗi: Id người dùng không hợp lệ.";
                            return RedirectToAction("Error", "Shared");
                        }

                    }
                    else
                    {

                        var idUserFromSession = HttpContext.Session.GetString("idUser");
                        if (!string.IsNullOrEmpty(idUserFromSession) && int.TryParse(idUserFromSession, out int userId))
                        {
                            transaction.idUser = userId;
                            _context.Update(transaction);
                        }
                        else
                        {
                            ViewBag.Error = "Lỗi: Id người dùng không hợp lệ.";
                            return RedirectToAction("Error", "Shared");
                        }

                    }

                    PopulateCategories();
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                return View(transaction);
            }

        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transactions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Transactions'  is null.");
            }
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [NonAction]
        public void PopulateCategories()
        {
            // Get idUser from HttpContext.Session
            var idUser = HttpContext.Session.GetString("idUser");

            if (!string.IsNullOrEmpty(idUser))
            {
                // Convert idUser to the appropriate data type (int, Guid, etc.) if needed
                // Example assuming idUser is of type int
                if (int.TryParse(idUser, out int userId))
                {
                    // Fetch the list of categories based on the userId
                    var CategoryCollection = _context.Categories
                        .Where(c => c.idUser == userId)
                        .ToList();

                    // Create a default category and insert it at the beginning of the list
                    Category DefaultCategory = new Category() { CategoryId = 0, Title = "Choose a Category" };
                    CategoryCollection.Insert(0, DefaultCategory);

                    // Store the filtered list of categories in the ViewBag
                    ViewBag.Categories = CategoryCollection;
                }
                else
                {
                }
            }

            else
            {

            }
        }

        [HttpGet]
        public IActionResult LinearRegression()
        {
            return View(new LinearRegression());
        }

        [HttpPost]
        public async Task<IActionResult> LinearRegression(LinearRegression model)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var idUserFromSession = HttpContext.Session.GetString("idUser");

                if (!string.IsNullOrEmpty(idUserFromSession) && int.TryParse(idUserFromSession, out int userId))
                {
                    var transactionsForCategory = await _context.Transactions
                        .Where(t => t.idUser == userId && t.CategoryId == model.CategoryId)
                        .ToListAsync();

                    int totalAmount = transactionsForCategory.Sum(t => t.Amount);
                    int transactionCount = transactionsForCategory.Count;
                    decimal averageAmount = transactionCount > 0 ? (decimal)totalAmount / transactionCount : 0;

                    // Gán các thuộc tính của model
                    model.UserId = userId;
                    model.TotalAmount = totalAmount;
                    model.TransactionCount = transactionCount;
                    model.AverageAmount = averageAmount;

                    // Lấy dữ liệu cho biểu đồ và gán vào ChartData
                    var data = transactionsForCategory.Select((t, index) => new { Index = index, Amount = t.Amount }).ToList();
                    model.ChartData = data.Cast<object>().ToArray();

                    return View(model);
                }
                else
                {
                    // Xử lý trường hợp khi idUserFromSession không thể chuyển đổi thành số nguyên
                    ViewBag.Error = "Lỗi: Id người dùng không hợp lệ.";
                    return RedirectToAction("Error", "Shared");
                }
            }
        }

        [HttpGet]
        public IActionResult DrawChart()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DrawChart(int categoryId)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var idUserFromSession = HttpContext.Session.GetString("idUser");

                if (!string.IsNullOrEmpty(idUserFromSession) && int.TryParse(idUserFromSession, out int userId))
                {
                    var transactionsForCategory = _context.Transactions
                        .Where(t => t.CategoryId == categoryId)
                        .ToList();

                    // Chuyển đổi dữ liệu thành định dạng phù hợp để vẽ biểu đồ
                    var chartData = transactionsForCategory
                        .Select((t, index) => new { Index = index, Amount = t.Amount })
                        .ToList();

                    // Dùng thư viện Math.NET để tính toán đường thẳng tối thiểu bình phương
                    var leastSquareFit = Fit.Line(chartData.Select(p => (double)p.Index).ToArray(), chartData.Select(p => (double)p.Amount).ToArray());

                    // Dữ liệu của đường thẳng
                    var trendlineData = Enumerable.Range(0, chartData.Count)
                        .Select(index => new { Index = index, Amount = leastSquareFit.Item1 * index + leastSquareFit.Item2 })
                        .ToList();

                    // Kết hợp dữ liệu biểu đồ và dữ liệu đường thẳng
                    var combinedData = new { ChartData = chartData, TrendlineData = trendlineData };

                    return Json(combinedData);
                }
                else
                {
                    // Xử lý trường hợp khi idUserFromSession không thể chuyển đổi thành số nguyên
                    ViewBag.Error = "Lỗi: Id người dùng không hợp lệ.";
                    return RedirectToAction("Error", "Shared");
                }
            }
        }
    }
}
