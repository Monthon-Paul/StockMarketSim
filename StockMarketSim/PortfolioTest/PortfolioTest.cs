using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stock;
namespace PortfolioTest;

/// <summary>
/// Testing the Funcitonality of Spreadsheet
/// </summary>
///
/// Author: Monthon Paul
/// Version: September 24 2022 1.0
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

		Assert.AreEqual(0u, user.GetShares("AAPL"));
		Assert.AreEqual("Paul", user.name);
		Assert.AreEqual(10_000, user.userCashBalance);

		Assert.AreEqual(0u, user1.GetShares("AAPL"));
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
		user.BuyStocks("AAPL", 5);
		user.BuyStocks("DAL", 2);
		user.Save("TestSanitySave.txt");

		Portfolio load = new Portfolio("TestSanitySave.txt", "stk");

		Assert.AreEqual(5u, load.GetShares("AAPL"));
		Assert.AreEqual("Paul", load.name);
		Assert.AreEqual(load.userCashBalance, load.userCashBalance);
	}

	[TestMethod]
	public void TestBuyGetShares() {
		Portfolio user = new Portfolio();
		user.BuyStocks("AAPL", 10);
		user.BuyStocks("DAL", 5);
		user.BuyStocks("F", 10);

		Assert.AreEqual(10, user.GetShares("AAPL"));
		Assert.AreEqual(5, user.GetShares("DAL"));
		Assert.AreEqual(10, user.GetShares("F"));
		Assert.AreEqual(user.userCashBalance, user.userCashBalance);
	}


	[TestMethod]
	public void TestSellGetShares() {
		Portfolio user = new Portfolio();

		Assert.AreEqual(10_000, user.userCashBalance);

		user.BuyStocks("AAPL", 10);
		user.BuyStocks("DAL", 5);

		user.SellStocks("AAPL", 5);
		user.SellStocks("DAL", 5);

		Assert.AreEqual(0, user.GetShares("DAL"));
		Assert.AreEqual(5, user.GetShares("AAPL"));
		Assert.AreEqual(user.userCashBalance, user.userCashBalance);
	}
}
