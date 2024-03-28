namespace QL_ChiTieu.Models
{
    public class LinearRegression
    {
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public int TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageAmount { get; set; }
        public object[] ChartData { get; set; }
        public class DataPoint
        {
            public int Index { get; set; }
            public int Amount { get; set; }
        }

    }

}
