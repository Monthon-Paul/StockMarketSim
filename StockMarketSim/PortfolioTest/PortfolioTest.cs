﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using Stock;

using static Stock.Portfolio;

namespace PortfolioTest;

/// <summary>
/// Testing the Funcitonality of Portfolio
/// </summary>
///
/// Author: Monthon Paul
/// Version: May 9, 2024
[TestClass]
public class PortfolioTest {
	// ********************* Valid Testing *********************//

	/// <summary>
	/// Test Constructors
	/// </summary>
	[TestMethod]
	public void TestConstructor() {
		Portfolio user1 = new("Bob");
		Portfolio user = new();

		Assert.AreEqual(0, user.GetShares("AAPL"));
		Assert.AreEqual("Paul", user.Name);
		Assert.AreEqual(10_000, user.UserCashBalance);

		Assert.AreEqual(0, user1.GetShares("AAPL"));
		Assert.AreEqual("Bob", user1.Name);
		Assert.AreEqual(10_000, user1.UserCashBalance);
	}

	[TestMethod]
	public void Test1Constructor() {
		Portfolio user = new("Bob");
        _ = user.BuyStocks("AAPL", 20);

		Assert.AreEqual(20, user.GetShares("AAPL"));
		Assert.AreEqual("Bob", user.Name);
		Assert.AreEqual(user.UserCashBalance, user.UserCashBalance);
	}

	[TestMethod]
	public void Test2Constructors() {
		Portfolio user = new();
        _ = user.BuyStocks("DAL", 15);
		user.Save("TestSanitySave.txt");

		Portfolio load = new("TestSanitySave.txt", "stk");

		Assert.AreEqual(15, load.GetShares("DAL"));
		Assert.AreEqual("Paul", load.Name);
		Assert.AreEqual(load.UserCashBalance, load.UserCashBalance);
	}

	[TestMethod]
	public void TestBuyGetShares() {
		Portfolio user = new();
        _ = user.BuyStocks("AAPL", 10);
		Thread.Sleep(1000);
        _ = user.BuyStocks("DAL", 15);
		Thread.Sleep(1000);
        _ = user.BuyStocks("F", 500);

		Assert.AreEqual(10, user.GetShares("AAPL"));
		Assert.AreEqual(15, user.GetShares("DAL"));
		Assert.AreEqual(500, user.GetShares("F"));
		Assert.AreEqual(user.UserCashBalance, user.UserCashBalance);
	}

	[TestMethod]
	public void TestSellGetShares() {
		Portfolio user = new();

		Assert.AreEqual(10_000, user.UserCashBalance);

        _ = user.BuyStocks("AAPL", 20);
		Thread.Sleep(1000);
        _ = user.BuyStocks("DAL", 15);
		Thread.Sleep(1000);

        _ = user.SellStocks("AAPL", 15);
		Thread.Sleep(1000);
        _ = user.SellStocks("DAL", 15);
		Thread.Sleep(1000);

		Assert.AreEqual(0, user.GetShares("DAL"));
		Assert.AreEqual(5, user.GetShares("AAPL"));
		Assert.AreEqual(user.UserCashBalance, user.UserCashBalance);
	}

	[TestMethod]
	public void TestSellSmallGetShares() {
		Portfolio user = new();

		Assert.AreEqual(10_000, user.UserCashBalance);

        _ = user.BuyStocks("AAPL", 30);
		Thread.Sleep(5000);

        _ = user.SellStocks("AAPL", 15);
		Thread.Sleep(5000);
        _ = user.SellStocks("AAPL", 15);
		Thread.Sleep(5000);

		Assert.AreEqual(0, user.GetShares("AAPL"));
		Assert.AreEqual(user.UserCashBalance, user.UserCashBalance);
	}

	[TestMethod]
	public void TestBuySharesBroker() {
		Portfolio user = new();
		user.Brokerslider(true);

		Thread.Sleep(5000);
        _ = user.BuyStocks("AAPL", 20);
		Thread.Sleep(5000);
        _ = user.BuyStocks("DAL", 50);
		Thread.Sleep(5000);

		Assert.AreEqual(20, user.GetShares("AAPL"));
		Assert.AreEqual(50, user.GetShares("DAL"));
		Assert.AreEqual(user.UserCashBalance, user.UserCashBalance);
	}

	[TestMethod]
	public void TestSellSharesBroker() {
		Portfolio user = new();

		Assert.AreEqual(10_000, user.UserCashBalance);

		user.Brokerslider(true);

        _ = user.BuyStocks("AAPL", 20);
		Thread.Sleep(5000);
        _ = user.SellStocks("AAPL", 15);
		Thread.Sleep(5000);

		Assert.AreEqual(5, user.GetShares("AAPL"));
		Assert.AreEqual(user.UserCashBalance, user.UserCashBalance);
	}

	//********************** Exception Testing ************************//

	/// <summary>
	/// Testing Buying Zero Shares
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(LowStockException))]
	public void TestZeroStocks() {
		Portfolio user = new();
        _ = user.BuyStocks("AAPL", 0);
	}

	/// <summary>
	/// Testing Buying Negative Stocks
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(LowStockException))]
	public void TestNegativeStocks() {
		Portfolio user = new();
        _ = user.BuyStocks("AAPL", -5);
	}

	/// <summary>
	/// Testing Buying less than Ask Stocks
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(LowStockException))]
	public void TestLessThanAskStocks() {
		Portfolio user = new();
        _ = user.BuyStocks("F", 400);
	}

	/// <summary>
	/// Testing Buying less than Bid Stocks
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(LowStockException))]
	public void TestLessThanBidStocks() {
		Portfolio user = new();
        _ = user.BuyStocks("F", 500);
		Thread.Sleep(1000);
        _ = user.SellStocks("F", 1);
	}

	/// <summary>
	/// Testing Buying from Closed Market
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(ClosedMarketException))]
	public void TestBuyClosedStocks() {
		Portfolio user = new();
        _ = user.BuyStocks("AAPL", 20);
	}

	/// <summary>
	/// Testing Selling from Closed Market
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(ClosedMarketException))]
	public void TestSellClosedStocks() {
		Portfolio user = new();
        _ = user.SellStocks("AAPL", 20);
	}

	/// <summary>
	/// Testing Buying with Insufficient Funds
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(InsufficientFundsException))]
	public void TestBuyInsufficientStocks() {
		Portfolio user = new();
        _ = user.BuyStocks("AAPL", 100);
	}

	/// <summary>
	/// Testing Selling with Insufficient Funds
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(InsufficientFundsException))]
	public void TestSellInsufficientStocks() {
		Portfolio user = new();
        _ = user.SellStocks("AAPL", 100);
	}
}
