using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;

namespace StockMarketSim;

public partial class MainPage : ContentPage {

	private const string apiKey = "CN0WTTYL7GCVQ5E3";
	private const int SearchDelay = 300;

	private ObservableCollection<AlphaVantageSearch> Ticker { get; set; }

	public MainPage() {
		InitializeComponent();
		Ticker = new();
		//BindingContext = this;
	}

	/// <summary>
	/// About menu to display the details about the Pogram
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void NewClick(object sender, EventArgs e) {
		DisplayAlert("About",
	  "Snake Game solution\n it's a full Online Multiplayer Game.\n" +
	  "Users are allow to connect to a Snake Server \n" + "to play on a Network with other players.\n" +
	  "Implementation by Monthon Paul\n", "OK");
	}

	/// <summary>
	/// About menu to display the details about the Pogram
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void LoadButton(object sender, EventArgs e) {
		DisplayAlert("About",
	  "Snake Game solution\n it's a full Online Multiplayer Game.\n" +
	  "Users are allow to connect to a Snake Server \n" + "to play on a Network with other players.\n" +
	  "Implementation by Monthon Paul\n", "OK");
	}

	/// <summary>
	/// About menu to display the details about the Pogram
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void SaveButton(object sender, EventArgs e) {
		DisplayAlert("About",
	  "Snake Game solution\n it's a full Online Multiplayer Game.\n" +
	  "Users are allow to connect to a Snake Server \n" + "to play on a Network with other players.\n" +
	  "Implementation by Monthon Paul\n", "OK");
	}

	/// <summary>
	/// About menu to display the details about the Pogram
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void SaveAsButton(object sender, EventArgs e) {
		DisplayAlert("About",
	  "Snake Game solution\n it's a full Online Multiplayer Game.\n" +
	  "Users are allow to connect to a Snake Server \n" + "to play on a Network with other players.\n" +
	  "Implementation by Monthon Paul\n", "OK");
	}

	/// <summary>
	/// About menu to display the details about the Pogram
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void AboutClicked(object sender, EventArgs e) {
		DisplayAlert("About",
	  "Snake Game solution\n it's a full Online Multiplayer Game.\n" +
	  "Users are allow to connect to a Snake Server \n" + "to play on a Network with other players.\n" +
	  "Implementation by Monthon Paul\n", "OK");
	}

	/// <summary>
	/// Help button to display the User how to move the Snake
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void HTPClicked(object sender, EventArgs e) {
		DisplayAlert("Controls",
					 "W:\t Move up\n" +
					 "A:\t Move left\n" +
					 "S:\t Move down\n" +
					 "D:\t Move right\n",
					 "OK");
	}

	private async void OnTextChanged(object sender, EventArgs e) {
		SearchBar searchBar = (SearchBar) sender;
		var query = searchBar.Text;

		string searchUrl = $"https://www.alphavantage.co/query?function=SYMBOL_SEARCH&keywords={query}&apikey={apiKey}";

		await Task.Delay(SearchDelay);
		if (!string.IsNullOrEmpty(query)) {
			using (HttpClient client = new HttpClient()) {
				HttpResponseMessage response = await client.GetAsync(searchUrl);
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				Ticker.Clear();
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
			Ticker.Clear();
		}
	}

	private class AlphaVantageSearch {
		public string Symbol { get; set; }
		public string Name { get; set; }
	}

	private void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
		if (e.SelectedItem != null) {
			var symbol = e.SelectedItem as string;
			// Do something with the selected symbol, such as navigating to a details page.
			ListView.SelectedItem = null;
		}
	}
}

