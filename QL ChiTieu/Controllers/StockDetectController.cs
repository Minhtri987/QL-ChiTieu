using Microsoft.AspNetCore.Mvc;
using static QL_ChiTieu.MLModel3;
using QL_ChiTieu.Models;

namespace QL_ChiTieu.Controllers
{
    public class StockDetectController : Controller
    {
        private readonly ApplicationDbContext _objModel;

        public StockDetectController(ApplicationDbContext objModel)
        {
            _objModel = objModel;
        }

        [HttpGet]
        public IActionResult Predict()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Predict(ModelInput input)
        {
            var prediction = MLModel3.Predict(input);
            ViewBag.Result = prediction;
            return View();
        }

        [HttpGet]
        public JsonResult GetTransactionIds(int categoryId)
        {
            // Retrieve transaction IDs based on the selected category
            var transactionIds = _objModel.Transactions
                .Where(t => t.CategoryId == categoryId)
                .Select(t => new { value = t.TransactionId, text = t.TransactionId.ToString() })
                .ToList();

            return Json(transactionIds);
        }
        [HttpGet]
        public ActionResult GetAmountForTransaction(int transactionId)
        {
            var amount = GetAmountForTransactionFromDatabase(transactionId);
            return Json(amount);
        }
        private decimal GetAmountForTransactionFromDatabase(int transactionId)
        {
            var transaction = _objModel.Transactions.FirstOrDefault(t => t.TransactionId == transactionId);
            return transaction?.Amount ?? 0.00m;
        }
    }
}
