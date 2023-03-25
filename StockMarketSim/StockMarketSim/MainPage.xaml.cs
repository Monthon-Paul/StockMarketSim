using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Stock;
using Newtonsoft.Json;

namespace StockMarketSim;

/// <summary>
/// StockMarketGUI for StockMarketSim
///
/// This Applicaiton is to replicate the Stock Market Simulator,
/// it's to have the basis idea of how the Stock Market works.
/// For the program I am grabing all the Stock Data from Alpha Vantage API.
/// The intend is it to be a small application where
/// the User can Buy/Sell stocks to make a profit.
/// The User can save their Portfolio into a ".stk" file in JSON format.
///
/// Author: Monthon Paul
/// Version: March 24, 2022
/// </summary>
public partial class MainPage : ContentPage {

	// Initialize variables
	private const string apiKey = "CN0WTTYL7GCVQ5E3";
	private const int SearchDelay = 300;
	private string fullpath;
	Portfolio user;

	private ObservableCollection<AlphaVantageSearch> Ticker { get; set; }

	public MainPage() {
		InitializeComponent();
		Ticker = new();
	}

	/// <summary>
	/// New Button that User can click to make a New Portfolio
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void NewClick(object sender, EventArgs e) {
		if (user.Changed) {
			//Asks the user if they want to save or not, cancel feature so that it doesn't clear
			string confirm = await DisplayActionSheet("Unsaved changes, Would you like to save changes?", "Cancel", null, "Yes", "No");
			// User select Cancel does nothing
			if (confirm is "Cancel") {
				return;
			} else if (confirm is "Yes") {
				//TODO: make Logic
			}
			// User wants to Create a New Portfolio
			string result = await DisplayPromptAsync("Enter a Name for Portfolio", "What's your name?");
			if (result is null) return;
			user = new(result);
		}
	}

	/// <summary>
	/// Loat Button that the User can load their existing Stock Portfolio
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void LoadClick(object sender, EventArgs e) {
		//TODO: Complete Logic, don't return 
		return;
	}

	/// <summary>
	/// Save User Portfolio Progress
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void SaveClick(object sender, EventArgs e) {
		//TODO: Complete Logic, don't return 
		return;
	}

	/// <summary>
	/// Save User Portfolio Progress at a specific location
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void SaveDataClick(object sender, EventArgs e) {
		//TODO: Complete Logic, don't return 
		return;
	}

	/// <summary>
	/// About menu to display the details about the Stock Market Simulator
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void AboutClick(object sender, EventArgs e) {
		//TODO: Complete Logic, don't return 
		return;
	}

	/// <summary>
	/// Help menu to display the Rules of the Stock Market Simulator
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void HTPClick(object sender, EventArgs e) {
		//TODO: Complete Logic, don't return 
		return;
	}

	/// <summary>
	/// On the Search Bar, User will search either specific Ticker symbols or companies,
	/// That will allow to display the best-matching symbols and market information based on keywords of user choice.
	/// </summary>
	/// <param name="sender"> Pointer to the Search Bar</param>
	/// <param name="e"> triggle an event </param>
	private async void OnTextChanged(object sender, EventArgs e) {
		SearchBar searchBar = (SearchBar) sender;
		var query = searchBar.Text;

		// Connect ot API
		string searchUrl = $"https://www.alphavantage.co/query?function=SYMBOL_SEARCH&keywords={query}&apikey={apiKey}";

		// a Delay in Search
		await Task.Delay(SearchDelay);
		if (!string.IsNullOrEmpty(query)) {
			using (HttpClient client = new HttpClient()) {
				HttpResponseMessage response = await client.GetAsync(searchUrl);
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				// Clear the Data Structue due to new keywords from User
				Ticker.Clear();
				// Parse JSON Doc in order to add info to the Data Structure
				using (JsonDocument json = JsonDocument.Parse(content)) {
					JsonElement root = json.RootElement;
					JsonElement bestMatches = root.GetProperty("bestMatches");
					foreach (var ele in bestMatches.EnumerateArray()) {
						AlphaVantageSearch result = new AlphaVantageSearch {
							Symbol = ele.GetProperty("1. symbol").ToString(),
							Name = ele.GetProperty("2. name").ToString()
						};
						Ticker.Add(result);
					}
					ListView.ItemsSource = Ticker;
				}
			}
		} else {
			// If the Search Bar has nothing, than no Data
			Ticker.Clear();
		}
	}

	// Class for Stock Data information
	private class AlphaVantageSearch {
		public string Symbol { get; set; }
		public string Name { get; set; }
	}

	/// <summary>
	/// When User Select an item on the list, the Stock Data will be display on the Graph with it's data
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
		if (e.SelectedItem != null) {
			var symbol = e.SelectedItem as AlphaVantageSearch;
			// Do something with the selected symbol, such as navigating to a details page.
			//TODO: Complete Logic
			ListView.SelectedItem = null;
		}
	}
}

