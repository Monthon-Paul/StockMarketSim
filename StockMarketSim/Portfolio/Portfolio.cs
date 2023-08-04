using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Stock;

/// <summary>
/// This represents the basis of a User Portfolio on
/// the Stock Market Simulator. The User can Buy/Sell Stocks
/// in the Simulator to represent like real world Stock Market Trade
/// The User can also change if they want a Broker fee to play in trading like
/// real life.
/// </summary>
/// 
/// Author: Monthon Paul
/// Version: May 5 2023
[JsonObject(MemberSerialization.OptIn)]
public class Portfolio {

	// Initialize variables
	[JsonProperty(PropertyName = "Name")]
	public string name { get; set; }
	[JsonProperty(PropertyName = "CashBalance")]
	public decimal userCashBalance { get; private set; }
	[JsonProperty(PropertyName = "Shares")]
	private Dictionary<string, int> userPortfolio;
	[JsonProperty(PropertyName = "Versions")]
	private string version;
	private bool change;

	// Broker buyer fee of 1% and selling fee of $10
	private readonly decimal brokerBuyFee = 0.01m;
	private readonly decimal brokerSellFee = 10;
	private bool broker = false;

	/// <summary>
	/// 2 args Contructor for Portfolio
	/// </summary>
	/// <param name="pathToFile"> Pathfile to load</param>
	/// <param name="version"> the correct version </param>
	/// <exception cref="PortfolioLoadException"></exception>
	public Portfolio(string pathToFile, string version) {
		// Convert a file to a User Portfolio
		try {
			// Read the file that makes a json to a Portfolio
			string json = File.ReadAllText(pathToFile);
			var data = JsonConvert.DeserializeObject<Portfolio>(json);
			// Check if it's null when created
			if (data is null) {
				throw new PortfolioLoadException("Trouble opening your Portfolio");
			}
			// Check if the version match
			if (!data.version.Equals(version)) {
				throw new PortfolioLoadException("Wrong version, is not a Stock Portfolio");
			}
			// Initialize types
			name = data.name;
			userCashBalance = data.userCashBalance;
			userPortfolio = data.userPortfolio;
			this.version = version;
		} catch (Exception) {
			throw new PortfolioLoadException("Trouble opening your Portfolio");
		}
		change = true;
	}

	/// <summary>
	/// Constructor given input User Name
	/// </summary>
	/// <param name="name"> User name of Portfolio</param>
	public Portfolio(string name) {
		this.name = name;
		userCashBalance = 10_000;
		userPortfolio = new();
		version = "stk";
		change = true;
	}

	// Default Constructor
	public Portfolio() :
		this("Paul") {
	}

	public bool Changed { get => change; set => change = value; }

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
	/// <exception cref="PortfolioLoadException">Throws an exception if the Portfoliio fails to save</exception>
	public void Save(string filename) {
		// try to save the User Portfolio as a file
		try {
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			change = false;
			File.WriteAllText(filename, json);
		} catch (Exception) {
			throw new PortfolioLoadException("Problems on Saving Portfolio");
		}
	}

	/// <summary>
	/// Writes the contents of User Portfolio JSON format string.
	/// The JSON object should have the following fields:
	/// "Version" - the version ending in stk
	/// "Name" - Name of the Portfolio
	/// "CashBalance" - Amount of money the user have
	/// "Shares" - in a Dictionary, key value of stock company matches to amount of shares the User have
	/// 
	/// If there are any problems opening, the method should throw a
	/// PortfolioLoadException with an explanatory message.
	/// </summary>
	/// <exception cref="PortfolioLoadException">Throws an exception if the Spreadsheet fails to save</exception>
	public string Save() {
		// Write the User Portfolio as a JSON String
		string json = JsonConvert.SerializeObject(this, Formatting.Indented);
		change = false;
		return json;
	}

	/// <summary>
	/// Allow User to Buy Stocks from grabing from the API
	/// </summary>
	/// <param name="symbol"> Stock Ticker Symbol</param>
	/// <param name="quantity"> Shares must be higher than 10 </param>
	/// <exception cref="LowStockException"> an Error when either Shares are lower than 10 shares</exception>
	public void BuyStocks(string symbol, int quantity) {
		// User need to buy Stock more than 10
		if (quantity < 10) {
			throw new LowStockException("Can't make transaction less than 10 stocks");
		}
		// Retrieve stock data from API
		StockData stockData = GetStockData(symbol).Result;

		// Calculate total cost of purchase
		decimal totalCost = stockData.Price * quantity;
		decimal fee = totalCost * brokerBuyFee;
		// User choice to have a Broker for transaction
		switch (broker) {
			case true:
				// Deduct purchase cost from user's cash balance
				if ((totalCost + fee) <= userCashBalance) {
					userCashBalance -= (totalCost + fee);
				} else {
					Console.WriteLine("Insufficient funds.");
					return;
				}
				break;
			case false:
				// Deduct purchase cost from user's cash balance
				if (totalCost <= userCashBalance) {
					userCashBalance -= totalCost;
				} else {
					Console.WriteLine("Insufficient funds.");
					return;
				}
				break;
		}
		// Add shares to user's portfolio
		if (userPortfolio.ContainsKey(symbol)) {
			userPortfolio[symbol] += quantity;
		} else {
			userPortfolio[symbol] = quantity;
		}
		Console.WriteLine("Purchase successful!");
		change = true;
	}

	/// <summary>
	/// Allow User to Sell Stocks from grabing from the API
	/// </summary>
	/// <param name="symbol"> Stock Ticker Symbol</param>
	/// <param name="quantity"> Shares must be higher than 10 </param>
	/// <exception cref="LowStockException"> an Error when either Shares are lower than 10 shares</exception>
	public void SellStocks(string symbol, int quantity) {
		// User can't sell shares in the Negative value
		if (quantity < 0) {
			throw new LowStockException("Can't sell Negative Stocks");
		}
		// Retrieve stock data from API
		StockData stockData = GetStockData(symbol).Result;

		// Calculate total sale price
		decimal totalSale = stockData.Price * quantity;
		// User choice to have a Broker for transaction
		if (broker) {
			if (brokerSellFee < totalSale) {
				totalSale -= brokerSellFee;
				goto Logic;
			} else {
				Console.WriteLine("Insufficient sale due to broker's $10 fee.");
				return;
			}
		}
		Logic:
		// Check if user owns enough shares to sell
		if (userPortfolio.ContainsKey(symbol) && userPortfolio[symbol] >= quantity) {
			// Add sale price to user's cash balance
			userCashBalance += totalSale;

			// Remove shares from user's portfolio
			userPortfolio[symbol] -= quantity;

			// If Shares are at 0, then remove Ticker symbol for User
			if (userPortfolio[symbol] is 0) {
				userPortfolio.Remove(symbol);
			}
			Console.WriteLine("Sale successful!");
			change = true;
		} else {
			Console.WriteLine("Insufficient shares Sold.");
		}
	}

	/// <summary>
	/// Allow the User to get Shares base on Ticker symbol
	/// </summary>
	/// <param name="symbol">Stock Ticker Symbol</param>
	/// <returns> The amount of Shares </returns>
	public int GetShares(string symbol) {
		if (userPortfolio.ContainsKey(symbol)) {
			return userPortfolio[symbol];
		} else {
			return 0;
		}
	}

	/// <summary>
	/// Print in Console info about Portfolio
	/// </summary>
	public void ViewPortfolio() {
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

	/// <summary>
	/// Switch between either User having a broker fee for transaction
	/// </summary>
	/// <returns></returns>
	public bool brokerslider() {
		return broker is true ? broker = false : broker = true;
	}

	/// <summary>
	/// Struct representing Stock Data
	/// </summary>
	public struct StockData {
		public string Symbol { get; set; }
		public decimal Price { get; set; }
		public DateTime Date { get; set; }
		public decimal Change { get; set; }
		public string Percent { get; set; }
	}

	/// <summary>
	/// Grab Stock Data from the Alpha Vantage API
	/// </summary>
	/// <param name="symbol">Stock Ticker Symbol</param>
	/// <returns> StockData </returns>
	public static async Task<StockData> GetStockData(string symbol) {
		string apiKey = GetAPIKey();
		string apiUrl = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={apiKey}";
		string format = "yyyy-MM-dd";

		using HttpClient client = new();
		HttpResponseMessage response = await client.GetAsync(apiUrl).ConfigureAwait(false);
		response.EnsureSuccessStatusCode();
		string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		using JsonDocument json = JsonDocument.Parse(result);
		JsonElement root = json.RootElement;
		JsonElement globalQuote = root.GetProperty("Global Quote");

		StockData stockData = new() {
			Symbol = globalQuote.GetProperty("01. symbol").GetString(),
			Price = Convert.ToDecimal(globalQuote.GetProperty("05. price").GetString()),
			Date = DateTime.ParseExact(globalQuote.GetProperty("07. latest trading day").GetString(), format, CultureInfo.InvariantCulture),
			Change = Convert.ToDecimal(globalQuote.GetProperty("09. change").GetString()),
			Percent = globalQuote.GetProperty("10. change percent").GetString()
		};

		//await Task.Delay(1000);

		return stockData;
	}

	/// <summary>
	/// Get Alpha Vantage API key
	/// </summary>
	/// <returns> string representation for Alpha Vantage API key</returns>
	private static string GetAPIKey() {
		// Create an instance of IConfiguration
		var config = new ConfigurationBuilder()
			.AddUserSecrets<Portfolio>()
			.Build();
		return config["apiKey"];
	}

	/// <summary>
	/// Thrown to indicate that a load attempt has failed.
	/// </summary>
	public class PortfolioLoadException : Exception {
		/// <summary>
		/// Creates the exception with a message
		/// </summary>
		public PortfolioLoadException(string msg)
			: base(msg) {
		}
	}

	/// <summary>
	/// Thrown to indicate that Negative Stocks is not an option
	/// </summary>
	public class LowStockException : Exception {
		/// <summary>
		/// Creates the exception with a message
		/// </summary>
		public LowStockException(string msg)
			: base(msg) {
		}
	}
}
