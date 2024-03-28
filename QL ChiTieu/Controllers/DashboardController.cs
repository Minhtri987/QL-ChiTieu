using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_ChiTieu.Models;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {

        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class CustomUserService
        {
            private readonly ApplicationDbContext _dbContext;

            public CustomUserService(ApplicationDbContext dbContext)
            {
                _dbContext = dbContext;
            }


        }



        public async Task<ActionResult> Index1()
        {
            return View();
        }

        public async Task<ActionResult> AIChat()
        {


            return View();
        }

        public async Task<ActionResult> Index()
        {


            var idUser = HttpContext.Session.GetString("idUser");
            if (TempData.ContainsKey("RefreshDashboard") && (bool)TempData["RefreshDashboard"])
            {
                // Load lại dữ liệu của Dashboard
                TempData["RefreshDashboard"] = false; // Đặt lại giá trị để tránh việc reload dữ liệu liên tục
            }

            if (!string.IsNullOrEmpty(idUser) && int.TryParse(idUser, out int parsedUserId))
            {
                //Last 7 Days
                DateTime StartDate = DateTime.Today.AddDays(-6);
                DateTime EndDate = DateTime.Today;

                List<Transaction> SelectedTransactions = await _context.Transactions
                    .Include(x => x.Category)
                    .Where(y => y.Date >= StartDate && y.Date <= EndDate && y.idUser == parsedUserId)
                    .ToListAsync();

                //Total Income
                int TotalIncome = SelectedTransactions
           .Where(i => i.Category.Type == "Thu nhập")
           .Sum(j => j.Amount);

                // Định dạng số tiền là VND
                string formattedTotalIncome = TotalIncome.ToString("N0") + " đ";

                ViewBag.TotalIncome = formattedTotalIncome;

                //Total Expense
                int TotalExpense = SelectedTransactions
          .Where(i => i.Category.Type == "Chi tiêu")
          .Sum(j => j.Amount);

                // Định dạng số tiền là VND
                string formattedTotalExpense = TotalExpense.ToString("N0") + " đ";

                ViewBag.TotalExpense = formattedTotalExpense;


                //Balance
                int Balance = TotalIncome - TotalExpense;

                // Định dạng số tiền là VND
                string formattedBalance = Balance.ToString("N0") + " đ";

                ViewBag.Balance = formattedBalance;

                // Doughnut Chart - Expense By Category
                ViewBag.DoughnutChartData = SelectedTransactions
                    .Where(i => i.Category.Type == "Chi tiêu")
                    .GroupBy(j => j.Category.CategoryId)
                    .Select(k => new
                    {
                        categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                        amount = k.Sum(j => j.Amount),
                        formattedAmount = k.Sum(j => j.Amount).ToString("N0") + " đ", // Định dạng số tiền là VND
                    })
                    .OrderByDescending(l => l.amount)
                    .ToList();
                ViewBag.DoughnutChartData1 = SelectedTransactions
                     .Where(i => i.Category.Type == "Thu nhập")
                    .GroupBy(j => j.Category.CategoryId)
                    .Select(k => new
                    {
                        categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                        amount = k.Sum(j => j.Amount),
                        formattedAmount = k.Sum(j => j.Amount).ToString("N0") + " đ", // Định dạng số tiền là VND
                    })
                    .OrderByDescending(l => l.amount)
                    .ToList();


                //Spline Chart - Income vs Expense

                //Income
                List<SplineChartData> IncomeSummary = SelectedTransactions
                    .Where(i => i.Category.Type == "Thu nhập")
                    .GroupBy(j => j.Date)
                    .Select(k => new SplineChartData()
                    {
                        day = k.First().Date.ToString("dd-MMM"),
                        income = k.Sum(l => l.Amount)
                    })
                    .ToList();


                //Expense
                List<SplineChartData> ExpenseSummary = SelectedTransactions
                    .Where(i => i.Category.Type == "Chi tiêu")
                    .GroupBy(j => j.Date)
                    .Select(k => new SplineChartData()
                    {
                        day = k.First().Date.ToString("dd-MMM"),
                        expense = k.Sum(l => l.Amount)
                    })
                    .ToList();



                //Combine Income & Expense
                string[] Last7Days = Enumerable.Range(0, 7)
                    .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                    .ToArray();

                ViewBag.SplineChartData = from day in Last7Days
                                          join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                          from income in dayIncomeJoined.DefaultIfEmpty()
                                          join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                          from expense in expenseJoined.DefaultIfEmpty()
                                          select new
                                          {
                                              day = day,
                                              income = income == null ? 0 : income.income,
                                              expense = expense == null ? 0 : expense.expense,
                                          };
                //Recent Transactions
                ViewBag.RecentTransactions = await _context.Transactions
                        .Include(i => i.Category)
                    .Where(t => t.idUser == parsedUserId) // Lọc theo idUser của người dùng hiện tại
                         .OrderByDescending(j => j.TransactionId) // Sắp xếp theo ngày giảm dần
                      .Take(10)
                    .ToListAsync();
            }
            else
            {
                // Handle the case where idUser is not valid
                ViewBag.Error = "Lỗi: Id người dùng không hợp lệ.";
                return View();
            }


            return View();
        }




    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;

    }
}
