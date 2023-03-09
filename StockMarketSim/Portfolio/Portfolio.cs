using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Channels;

namespace Stock;

[JsonObject(MemberSerialization.OptIn)]
public class Portfolio {

	// Initialize variables
	[JsonProperty(PropertyName = "Name")]
	public string name { get; }
	[JsonProperty(PropertyName = "CashBalance")]
	public decimal userCashBalance { get; private set; }
	[JsonProperty(PropertyName = "Shares")]
	private Dictionary<string, uint> userPortfolio;
	[JsonProperty(PropertyName = "Versions")]
	private string version;

	// Broker feenof 1%
	private readonly decimal brokerfee = 0.01m;
	private bool broker = false;

	/// <summary>
	/// 2 args Contructor for Portfolio
	/// </summary>
	/// <param name="pathToFile"> Pathfile to load</param>
	/// <param name="version"> the correct version </param>
	/// <exception cref="PortfolioLoadException"></exception>
	public Portfolio(string pathToFile, string version) {
		// Convert a file to a Spreadsheet i.e load a file to Spreadsheet
		try {
			//Read the file that makes a json to an acutal spreadsheet
			string json = File.ReadAllText(pathToFile);
			var data = JsonConvert.DeserializeObject<Portfolio>(json);
			// Check if the spreadsheet is null when created
			if (data is null) {
				throw new PortfolioLoadException("Trouble opening your Portfolio");
			}
			// Check if the version match
			if (!data.version.Equals(version)) {
				throw new PortfolioLoadException("Wrong version, is not a Stock Portfolio");
			}
			this.name = data.name;
			userCashBalance = data.userCashBalance;
			userPortfolio = data.userPortfolio;
			this.version = version;
		} catch (Exception) {
			throw new PortfolioLoadException("Trouble opening your Portfolio");
		}
	}

	/// <summary>
	/// Constructor given input User Name
	/// </summary>
	/// <param name="name"> User name of Portfolio</param>
	public Portfolio(string name) {
		this.name = name;
		userCashBalance = 10_000;
		userPortfolio = new();
		this.version = "stk";
	}

	// Default Constructor
	public Portfolio() :
		this("Paul") {
	}

	/// <summary>
	/// Writes the contents of User Portfolio to the named file using a JSON format.
	/// The JSON object should have the following fields:
	/// "Version" - the version ending in stk
	/// "Name" - Name of the Portfolio
	/// "CashBalance" - Amount of money the user have
	/// "Shares" - in a Dictionary, key value of stock company matches to amount of shares the User have
	/// 
	/// If there are any problems opening, the method should throw a
	/// PortfolioLoadException with an explanatory message.
	/// </summary>
	/// <param name="filename">file name being save as</param>
	/// <exception cref="PortfolioLoadException">Throws an exception if the Spreadsheet fail to save</exception>
	public void Save(string filename) {
		// try to save the User Portfolio as a file
		try {
			string json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);

			File.WriteAllText(filename, json);
		} catch (Exception) {
			throw new PortfolioLoadException("Problems on Saving Portfolio");
		}
	}

	public void BuyStocks(string symbol, uint quantity) {
		// Retrieve stock data from API
		StockData stockData = GetStockData(symbol).Result;

		// Calculate total cost of purchase
		decimal totalCost = stockData.Price * quantity;

		// Deduct purchase cost from user's cash balance
		if (totalCost <= userCashBalance) {
			userCashBalance -= totalCost;

			// Add shares to user's portfolio
			if (userPortfolio.ContainsKey(symbol)) {
				userPortfolio[symbol] += quantity;
			} else {
				userPortfolio[symbol] = quantity;
			}

			if (broker) {
				decimal fee = totalCost * brokerfee;
				userCashBalance -= fee;
			}
			Console.WriteLine("Purchase successful!");
		} else {
			Console.WriteLine("Insufficient funds.");
		}
	}

	public void SellStocks(string symbol, uint quantity) {
		// Retrieve stock data from API
		StockData stockData = GetStockData(symbol).Result;

		// Calculate total sale price
		decimal totalSale = stockData.Price * quantity;

		if (userPortfolio.ContainsKey(symbol) && userPortfolio[symbol] >= quantity) {
			// Add sale price to user's cash balance
			userCashBalance += totalSale;

			// Remove shares from user's portfolio
			userPortfolio[symbol] -= quantity;

			if (userPortfolio[symbol] == 0) {
				userPortfolio.Remove(symbol);
			}

			if (broker) {
				decimal fee = totalSale * brokerfee;
				userCashBalance -= fee;
			}
			Console.WriteLine("Sale successful!");
		} else {
			Console.WriteLine("Insufficient shares.");
		}
	}

	public uint GetShares(string symbol) {
		if (userPortfolio.ContainsKey(symbol)) {
			return userPortfolio[symbol];
		} else {
			return 0u;
		}
	}

	public void ViewPortfolio() {
		Console.WriteLine("Portfolio:");
		Console.WriteLine("----------");
		// Display user's cash balance
		Console.WriteLine($"Cash: {userCashBalance:C}");

		// Display user's stock holdings
		foreach (string symbol in userPortfolio.Keys) {
			uint quantity = userPortfolio[symbol];
			StockData stockData = GetStockData(symbol).Result;
			decimal value = quantity * stockData.Price;
			Console.WriteLine($"{symbol}: {quantity} shares worth {value:C}");
		}
	}

	/// <summary>
	/// Switch between either User having a broker fee for transaction
	/// </summary>
	/// <returns></returns>
	public bool brokerslider() {
		return broker is true ? broker = false : broker = true;
	}

	private struct StockData {
		public string Symbol { get; set; }
		public decimal Open { get; set; }
		public decimal High { get; set; }
		public decimal Low { get; set; }
		public decimal Price { get; set; }
		public DateTime Date { get; set; }
		public string Percent { get; set; }
	}

	private static async Task<StockData> GetStockData(string symbol) {
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
				//await Task.Delay(500);

				return stockData;
			} else {
				throw new Exception($"Failed to retrieve data for symbol {symbol}");
			}
		}
	}

	/// <summary>
	/// Changes Stock Data format to DateTime
	/// </summary>
	/// <param name="date">data string</param>
	/// <returns></returns>
	private static DateTime DateFormat(string date) {
		string[] arr = date.Split('-');
		return new DateTime(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
	}

	/// <summary>
	/// Thrown to indicate that a load attempt has failed.
	/// </summary>
	private class PortfolioLoadException : Exception {
		/// <summary>
		/// Creates the exception with a message
		/// </summary>
		public PortfolioLoadException(string msg)
			: base(msg) {
		}
	}
}

