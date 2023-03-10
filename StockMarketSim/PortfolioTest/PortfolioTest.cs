using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stock;
using static Stock.Portfolio;

namespace PortfolioTest;

/// <summary>
/// Testing the Funcitonality of Portfolio
/// </summary>
///
/// Author: Monthon Paul
/// Version: March 8 2023
[TestClass]
public class PortfolioTest {
	// ********************* Valid Testing *********************//

	/// <summary>
	/// Test Constructors
	/// </summary>
	[TestMethod]
	public void TestConstructor() {
		Portfolio user1 = new Portfolio("Bob");
		Portfolio user = new Portfolio();

		Assert.AreEqual(0, user.GetShares("AAPL"));
		Assert.AreEqual("Paul", user.name);
		Assert.AreEqual(10_000, user.userCashBalance);

		Assert.AreEqual(0, user1.GetShares("AAPL"));
		Assert.AreEqual("Bob", user1.name);
		Assert.AreEqual(10_000, user1.userCashBalance);
	}

	[TestMethod]
	public void Test1Constructor() {
		Portfolio user = new Portfolio("Bob");
		user.BuyStocks("AAPL", 20);

		Assert.AreEqual(20, user.GetShares("AAPL"));
		Assert.AreEqual("Bob", user.name);
		Assert.AreEqual(user.userCashBalance, user.userCashBalance);
	}

	[TestMethod]
	public void Test2Constructors() {
		Portfolio user = new Portfolio();
		user.BuyStocks("DAL", 15);
		user.Save("TestSanitySave.txt");

		Portfolio load = new Portfolio("TestSanitySave.txt", "stk");

		Assert.AreEqual(15, load.GetShares("DAL"));
		Assert.AreEqual("Paul", load.name);
		Assert.AreEqual(load.userCashBalance, load.userCashBalance);
	}

	[TestMethod]
	public void TestBuyGetShares() {
		Portfolio user = new Portfolio();
		user.BuyStocks("AAPL", 20);
		Thread.Sleep(10000);
		user.BuyStocks("DAL", 10);
		Thread.Sleep(10000);
		user.BuyStocks("F", 20);

		Assert.AreEqual(20, user.GetShares("AAPL"));
		Assert.AreEqual(10, user.GetShares("DAL"));
		Assert.AreEqual(20, user.GetShares("F"));
		Assert.AreEqual(user.userCashBalance, user.userCashBalance);
	}

	[TestMethod]
	public void TestSellGetShares() {
		Portfolio user = new Portfolio();

		Assert.AreEqual(10_000, user.userCashBalance);

		user.BuyStocks("AAPL", 20);
		Thread.Sleep(20000);
		user.BuyStocks("DAL", 15);
		Thread.Sleep(20000);

		user.SellStocks("AAPL", 15);
		Thread.Sleep(20000);
		user.SellStocks("DAL", 15);
		Thread.Sleep(20000);

		Assert.AreEqual(0, user.GetShares("DAL"));
		Assert.AreEqual(5, user.GetShares("AAPL"));
		Assert.AreEqual(user.userCashBalance, user.userCashBalance);
	}

	[TestMethod]
	public void TestSellSmallGetShares() {
		Portfolio user = new Portfolio();

		Assert.AreEqual(10_000, user.userCashBalance);

		user.BuyStocks("AAPL", 20);
		Thread.Sleep(20000);

		user.SellStocks("AAPL", 15);
		Thread.Sleep(20000);
		user.SellStocks("AAPL", 5);
		Thread.Sleep(20000);

		Assert.AreEqual(0, user.GetShares("AAPL"));
		Assert.AreEqual(user.userCashBalance, user.userCashBalance);
	}

	[TestMethod]
	public void TestBuySharesBroker() {
		Portfolio user = new Portfolio();
		user.brokerslider();

		Thread.Sleep(10000);
		user.BuyStocks("AAPL", 10);
		Thread.Sleep(10000);
		user.BuyStocks("DAL", 50);
		Thread.Sleep(10000);

		Assert.AreEqual(10, user.GetShares("AAPL"));
		Assert.AreEqual(50, user.GetShares("DAL"));
		Assert.AreEqual(user.userCashBalance, user.userCashBalance);
	}

	[TestMethod]
	public void TestSellSharesBroker() {
		Portfolio user = new Portfolio();

		Assert.AreEqual(10_000, user.userCashBalance);

		user.brokerslider();

		user.BuyStocks("AAPL", 10);
		Thread.Sleep(10000);
		user.SellStocks("AAPL", 5);
		Thread.Sleep(10000);

		Assert.AreEqual(5, user.GetShares("AAPL"));
		Assert.AreEqual(user.userCashBalance, user.userCashBalance);
	}

	//********************** Exception Testing ************************//

	/// <summary>
	/// Testing Buying Zero Shares
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(LowStockException))]
	public void TestZeroStocks() {
		Portfolio user = new Portfolio();
		user.BuyStocks("AAPL", 0);
	}

	/// <summary>
	/// Testing Buying Negative Stocks
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(LowStockException))]
	public void TestNegativeStocks() {
		Portfolio user = new Portfolio();
		user.BuyStocks("AAPL", -5);
	}

	/// <summary>
	/// Testing Buying less than 10 Stocks
	/// </summary>
	[TestMethod]
	[ExpectedException(typeof(LowStockException))]
	public void TestLessThan10Stocks() {
		Portfolio user = new Portfolio();
		user.BuyStocks("AAPL", 5);
	}
}
