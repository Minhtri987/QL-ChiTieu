using Microsoft.AspNetCore.Mvc;
using QL_ChiTieu.Models;
using Accord.MachineLearning;
using MathNet.Numerics;
using Newtonsoft.Json.Linq;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra;



namespace QL_ChiTieu.Controllers
{
    public class MLController : Controller
    {

        private readonly ApplicationDbContext _context;

        public MLController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]

        [HttpGet]
        public IActionResult PredictAmountForNextMonth()
        {
            // Filter transactions for the given category and type "Expense"

            int idUser =  HttpContext.Session.GetInt32("idUser") ?? 0;
            List<int> categoryIds = _context.Categories
            .Where(c => c.Type == "Chi tiêu" && c.idUser == idUser)
            .Select(c => c.CategoryId)
            .ToList();

            if (categoryIds.Any())
            {
                // Lấy các giao dịch dựa trên danh sách CategoryId
                var transactions = _context.Transactions
                    .Where(t => categoryIds.Contains(t.CategoryId))
                    .ToList();

                // Prepare data for linear regression
                double[] inputs = transactions.Select(t => (double)t.Date.Ticks).ToArray();
            double[] outputs = transactions.Select(t => (double)t.Amount).ToArray();

            // Train linear regression model using gradient descent
            double learningRate = 0.0001;
            int epochs = 1000;
            double[] weights = TrainLinearRegression(inputs, outputs, learningRate, epochs);

            // Predict the amount for the next month
            DateTime nextMonth = DateTime.Now.AddMonths(1);
            double nextMonthInput = nextMonth.Ticks;
            double predictedAmount = PredictLinearRegression(nextMonthInput, weights);

           

            return View(predictedAmount);
            }
            else
            {
                // Xử lý trường hợp không tìm thấy Category
                return NotFound("Category not found");
            }
        }

        private double[] TrainLinearRegression(double[] inputs, double[] outputs, double learningRate, int epochs)
        {
            int m = inputs.Length; // Number of training examples
            double[] weights = new double[2] { 0, 0 }; // Initialize weights

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double[] predictions = inputs.Select(input => PredictLinearRegression(input, weights)).ToArray();

                // Update weights using gradient descent
                double[] errors = predictions.Zip(outputs, (p, actual) => p - actual).ToArray();
                double gradient1 = (1.0 / m) * errors.Zip(inputs, (e, input) => e * input).Sum();
                double gradient0 = (1.0 / m) * errors.Sum();

                weights[0] -= learningRate * gradient0;
                weights[1] -= learningRate * gradient1;
            }

            return weights;
        }

        private double PredictLinearRegression(double input, double[] weights)
        {
            return weights[0] + weights[1] * input;
        }



        public ActionResult Index()
        {
            return View();
        }



    }


   
}
