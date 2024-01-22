using YahooQuotesApi;

namespace StockData;

public class StockData {
	public string? Symbol { get; set; }
	public decimal Open { get; set; }
	public decimal High { get; set; }
	public decimal Low { get; set; }
	public decimal Price { get; set; }
	public DateTime Date { get; set; }
	public string? Percent { get; set; }
}

public class GetStock {

	private static decimal userCashBalance = 10_000;
	private static Dictionary<string, int> userPortfolio = [];
	private static readonly YahooQuotes yahooQuotes = new YahooQuotesBuilder().Build();

	public static void Main(string[] args) {
		// Display menu
		
		while (true) {
			Console.WriteLine("Stock Market Simulator");
			Console.WriteLine("----------------------");
			Console.WriteLine("1. View stock prices");
			Console.WriteLine("2. Buy stocks");
			Console.WriteLine("3. Sell stocks");
			Console.WriteLine("4. View portfolio");
			Console.WriteLine("5. Exit");

			// Get user choice
			Console.Write("Enter choice: ");
			string choice = Console.ReadLine();

			// Perform action based on user choice
			switch (choice) {
				case "1":
					ViewStockPrices();
					break;
				case "2":
					BuyStocks();
					break;
				case "3":
					SellStocks();
					break;
				case "4":
					ViewPortfolio();
					break;
				case "5":
					Console.WriteLine("Exiting program...");
					return;
				default:
					Console.WriteLine("Invalid choice. Please try again.");
					break;
			}
		}
	}

	private static void ViewStockPrices() {
		// Get stock symbol from user
		Console.Write("Enter stock symbol: ");
		string symbol = Console.ReadLine();

		// Retrieve stock data from API
		StockData stockData = GetStockData(symbol).Result;

		// Display stock data
		Console.WriteLine($"Symbol: {stockData.Symbol}");
		Console.WriteLine($"Open: {stockData.Open:C}");
		Console.WriteLine($"High: {stockData.High:C}");
		Console.WriteLine($"Low: {stockData.Low:C}");
		Console.WriteLine($"Price: {stockData.Price:C}");
		Console.WriteLine($"Date: {stockData.Date:D}");
		Console.WriteLine($"Change Percent: {stockData.Percent}");
	}

	private static void BuyStocks() {
		// Get stock symbol and quantity from user
		Console.Write("Enter stock symbol: ");
		string symbol = Console.ReadLine();

		Console.Write("Enter quantity: ");
		int quantity = int.Parse(Console.ReadLine());

		// Retrieve stock data from API
		StockData stockData = GetStockData(symbol).Result;

		// Calculate total cost of purchase
		decimal totalCost = stockData.Price * quantity;

		// Confirm purchase with user
		Console.WriteLine($"Buy {quantity} shares of {stockData.Symbol} at {stockData.Price:C} each for a total cost of {totalCost:C}? (y/n)");
		string confirm = Console.ReadLine();
		if (confirm == "y") {
			// Deduct purchase cost from user's cash balance
			if (totalCost <= userCashBalance) {
				userCashBalance -= totalCost;

				// Add shares to user's portfolio
				if (userPortfolio.ContainsKey(symbol))
					userPortfolio[symbol] += quantity;
				else
					userPortfolio[symbol] = quantity;

				Console.WriteLine("Purchase successful!");
			} else {
				Console.WriteLine("Insufficient funds.");
			}
		}
	}

	private static void SellStocks() {
		// Get stock symbol and quantity from user
		Console.Write("Enter stock symbol: ");
		string symbol = Console.ReadLine();

		Console.Write("Enter quantity: ");
		int quantity = int.Parse(Console.ReadLine());

		// Retrieve stock data from API
		StockData stockData = GetStockData(symbol).Result;

		// Calculate total sale price
		decimal totalSale = stockData.Price * quantity;

		// Confirm sale with user
		Console.WriteLine($"Sell {quantity} shares of {stockData.Symbol} at {stockData.Price:C} each for a total sale price of {totalSale:C}? (y/n)");
		string confirm = Console.ReadLine();
		if (confirm == "y") {
			// Check if user owns enough shares to sell
			if (userPortfolio.ContainsKey(symbol) && userPortfolio[symbol] >= quantity) {
				// Add sale price to user's cash balance
				userCashBalance += totalSale;

				// Remove shares from user's portfolio
				userPortfolio[symbol] -= quantity;

				Console.WriteLine("Sale successful!");
			} else {
				Console.WriteLine("Insufficient shares.");
			}
		}
	}

	private static void ViewPortfolio() {
		// TODO: Implement logic to retrieve user's portfolio data and display it to the console
		Console.WriteLine("Portfolio:");
		Console.WriteLine("----------");
		// Display user's cash balance
		Console.WriteLine($"Cash: {userCashBalance:C}");

		// Display user's stock holdings
		foreach (string symbol in userPortfolio.Keys) {
			int quantity = userPortfolio[symbol];
			StockData stockData = GetStockData(symbol).Result;
			decimal value = quantity * stockData.Price;
			Console.WriteLine($"{symbol}: {quantity} shares worth {value:C}");
		}
	}
	public static async Task<StockData> GetStockData(string symbol) {
		// YahooClient yahooClient = new();
		// var autoCompleteList = await yahooClient.GetAutoCompleteInfoAsync("Google");
		// var marketSummaryList = await yahooClient.GetMarketSummaryAsync();
		// You could query multiple symbols with multiple fields through the following steps:
	
		var security = await yahooQuotes.GetAsync(symbol) ?? throw new Exception($"Failed to retrieve data for symbol {symbol}");
		var date = DateTimeOffset.FromUnixTimeSeconds(security.RegularMarketTimeSeconds);
		var percent = security.RegularMarketChangePercent ?? 0.0;

		StockData stockData = new() {
			Symbol = security.Symbol.Name,
			Open = security.RegularMarketOpen ?? 0m,
			High = security.RegularMarketDayHigh ?? 0m,
			Low = security.RegularMarketDayLow ?? 0m,
			Price = security.RegularMarketPrice ?? 0m,
			Date = date.DateTime,
			Percent = percent.ToString("N2") + "%"
		};

		return stockData;
    }
}

