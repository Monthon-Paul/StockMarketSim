namespace Stock;

public partial class Portfolio {
    /// <summary>
    /// Class representing Stock Data
    /// </summary>
    public class StockData {
        public required string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal AskSize { get; set; }
        public decimal BidSize { get; set; }
        public DateTime Date { get; set; }
        public decimal Change { get; set; }
        public required string Percent { get; set; }
        public required string State { get; set; }
    }
}
