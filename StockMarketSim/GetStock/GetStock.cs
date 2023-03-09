using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace StockData;

public class StockData {
	public string Symbol { get; set; }
	public decimal Open { get; set; }
	public decimal High { get; set; }
	public decimal Low { get; set; }
	public decimal Price { get; set; }
	public DateTime Date { get; set; }
	public string Percent { get; set; }
}

public class GetStock {

	private static decimal userCashBalance = 10_000;
	private static Dictionary<string, int> userPortfolio = new();

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
		Console.WriteLine($"Date: {stockData.Date.ToString("D")}");
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
				if (userPortfolio.ContainsKey(symbol)) {
					userPortfolio[symbol] += quantity;
				} else {
					userPortfolio[symbol] = quantity;
				}

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
		string apiKey = "CN0WTTYL7GCVQ5E3";
		string apiUrl = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={apiKey}";

		using (HttpClient client = new HttpClient()) {
			HttpResponseMessage response = await client.GetAsync(apiUrl);
			if (response.IsSuccessStatusCode) {
				string result = await response.Content.ReadAsStringAsync();
				JsonDocument json = JsonDocument.Parse(result);
				JsonElement root = json.RootElement;
				JsonElement globalQuote = root.GetProperty("Global Quote");

				StockData stockData = new StockData {
					Symbol = globalQuote.GetProperty("01. symbol").GetString(),
					Open = Convert.ToDecimal(globalQuote.GetProperty("02. open").GetString()),
					High = Convert.ToDecimal(globalQuote.GetProperty("03. high").GetString()),
					Low = Convert.ToDecimal(globalQuote.GetProperty("04. low").GetString()),
					Price = Convert.ToDecimal(globalQuote.GetProperty("05. price").GetString()),
					Date = DateFormat(globalQuote.GetProperty("07. latest trading day").GetString()),
					Percent = globalQuote.GetProperty("10. change percent").GetString()
				};

				return stockData;
			} else {
				throw new Exception($"Failed to retrieve data for symbol {symbol}");
			}
		}
	}

	//private static async Task<AlphaVantageResponse> GetStockDaily(string symbol) {
	//	string apiKey = "CN0WTTYL7GCVQ5E3";
	//	string apiUrl = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&apikey={apiKey}";

	//	using (HttpClient client = new HttpClient()) {
	//		HttpResponseMessage response = await client.GetAsync(apiUrl);
	//		if (response.IsSuccessStatusCode) {
	//			string result = await response.Content.ReadAsStringAsync();
	//		} else {
	//			throw new Exception($"Failed to retrieve data for symbol {symbol}");
	//		}
	//	}
	//}

	private static DateTime DateFormat(string date) {
		string[] arr = date.Split('-');
		return new DateTime(Int32.Parse(arr[0]), Int32.Parse(arr[1]), Int32.Parse(arr[2]));
	}
}

