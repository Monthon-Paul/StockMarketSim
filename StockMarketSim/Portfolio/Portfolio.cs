﻿using System.Text.Json;
using Newtonsoft.Json;
using YahooQuotesApi;

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
/// Version: January 21, 2024
[JsonObject(MemberSerialization.OptIn)]
public class Portfolio {

	// Initialize variables
	[JsonProperty(PropertyName = "Name")]
	public string Name { get; set; }
	[JsonProperty(PropertyName = "CashBalance")]
	public decimal UserCashBalance { get; private set; }
	[JsonProperty(PropertyName = "Shares")]
	private Dictionary<string, int> userPortfolio;
	[JsonProperty(PropertyName = "Versions")]
	private readonly string version;
	private bool change;

	// Broker buyer fee of 1% and selling fee of $10
	private readonly decimal brokerBuyFee = 0.01m;
	private readonly decimal brokerSellFee = 10;
	private bool broker = false;
	private static YahooQuotes yahooQuotes = new YahooQuotesBuilder().WithoutHttpResilience().Build();

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
			var data = JsonConvert.DeserializeObject<Portfolio>(json) ?? throw new PortfolioLoadException("Trouble opening your Portfolio");
            // Check if the version match
            if (!data.version.Equals(version))
				throw new PortfolioLoadException("Wrong version, is not a Stock Portfolio");
				
			// Initialize types
			Name = data.Name;
			UserCashBalance = data.UserCashBalance;
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
		Name = name;
		UserCashBalance = 10_000;
		userPortfolio = [];
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
		// Retrieve stock data from API
		StockData stockData = GetStockData(symbol).Result;
		// Check the State of the Market
		if(stockData.State == "CLOSED")
			throw new ClosedMarketException("Can't Buy Shares when Market is Closed");
		// User need to buy Stock more than ask size
		if (quantity < stockData.AskSize)
			throw new LowStockException($"Can't make transaction less than {stockData.AskSize} stocks");

		// Calculate total cost of purchase
		decimal totalCost = stockData.Price * quantity;
		decimal fee = totalCost * brokerBuyFee;
		// User choice to have a Broker for transaction
		switch (broker) {
			case true:
				// Deduct purchase cost from user's cash balance
				if ((totalCost + fee) <= UserCashBalance) {
					UserCashBalance -= totalCost + fee;
				} else {
					Console.WriteLine("Insufficient funds.");
					return;
				}
				break;
			case false:
				// Deduct purchase cost from user's cash balance
				if (totalCost <= UserCashBalance) {
					UserCashBalance -= totalCost;
				} else {
					Console.WriteLine("Insufficient funds.");
					return;
				}
				break;
		}
		// Add shares to user's portfolio
		if (userPortfolio.ContainsKey(symbol))
			userPortfolio[symbol] += quantity;
		else
			userPortfolio[symbol] = quantity;
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
		// Retrieve stock data from API
		StockData stockData = GetStockData(symbol).Result;
		// Check the State of the Market
		if(stockData.State == "CLOSED")
			throw new ClosedMarketException("Can't Sell Shares when Market is Closed");
		// User can't sell shares less than bid size
		if (quantity < stockData.BidSize)
			throw new LowStockException("Can't sell Negative Stocks");
		
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
		if (userPortfolio.TryGetValue(symbol, out int value) && value >= quantity) {
			// Add sale price to user's cash balance
			UserCashBalance += totalSale;

			// Remove shares from user's portfolio
			userPortfolio[symbol] -= quantity;

			// If Shares are at 0, then remove Ticker symbol for User
			if (userPortfolio[symbol] is 0)
				userPortfolio.Remove(symbol);
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
		if (userPortfolio.TryGetValue(symbol, out int value))
			return value;
		else
			return 0;
	}

	/// <summary>
	/// Print in Console info about Portfolio
	/// </summary>
	public void ViewPortfolio() {
		Console.WriteLine("Portfolio:");
		Console.WriteLine("----------");
		// Display user's cash balance
		Console.WriteLine($"Cash: {UserCashBalance:C}");

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
		public decimal AskSize { get; set;}
		public decimal BidSize { get; set;}
		public DateTime Date { get; set; }
		public decimal Change { get; set; }
		public string Percent { get; set; }
		public string State { get; set; }
	}

	/// <summary>
	/// Grab Stock Data from the Alpha Vantage API
	/// </summary>
	/// <param name="symbol">Stock Ticker Symbol</param>
	/// <returns> StockData </returns>
	public static async Task<StockData> GetStockData(string symbol) {
		// Yahoo Finance API to grab Stock data
		var security = await yahooQuotes.GetAsync(symbol) ?? throw new Exception($"Failed to retrieve data for symbol {symbol}");
		var date = DateTimeOffset.FromUnixTimeSeconds(security.RegularMarketTimeSeconds);
		var percent = security.RegularMarketChangePercent ?? 0.0;

		StockData stockData = new() {
			Symbol = security.Symbol.Name,
			Price = security.RegularMarketPrice ?? 0m,
			AskSize = security.AskSize ?? 0m,
			BidSize = security.BidSize ?? 0m,
			Date = date.DateTime,
			Change = security.RegularMarketChange ?? 0m,
			Percent = percent.ToString("N2") + "%",
			State = security.MarketState
		};

		return stockData;
	}

    /// <summary>
    /// Thrown to indicate that a load attempt has failed.
    /// </summary>
    /// <remarks>
    /// Creates the exception with a message
    /// </remarks>
    public class PortfolioLoadException(string msg) : Exception(msg) {}

    /// <summary>
    /// Thrown to indicate that Low Stocks is not an option
    /// </summary>
    /// <remarks>
    /// Creates the exception with a message
    /// </remarks>
    public class LowStockException(string msg) : Exception(msg) {}

	/// <summary>
    /// Thrown to indicate that can't buy/sell shares when Market Closed
    /// </summary>
    /// <remarks>
    /// Creates the exception with a message
    /// </remarks>
    public class ClosedMarketException(string msg) : Exception(msg) {}
}
