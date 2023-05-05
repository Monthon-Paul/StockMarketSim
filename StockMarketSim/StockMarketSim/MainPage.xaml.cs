using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using CommunityToolkit.Maui.Storage;
using Stock;

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
/// Version: May 5, 2023
/// </summary>
public partial class MainPage : ContentPage {

	// Initialize variables
	private const int SearchDelay = 300;
	private string fullpath;
	private Portfolio user;
	private IFileSaver fileSaver;

	private ObservableCollection<AlphaVantageSearch> Ticker { get; set; }

	public MainPage(IFileSaver fileSaver) {
		InitializeComponent();
		Ticker = new();
		this.fileSaver = fileSaver;
	}

	/// <summary>
	/// New Button that User can click to make a New Portfolio
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void NewClick(object sender, EventArgs e) {
		// First check if there is a Portfolio, then Checks if the Portfoliio has been changed
		if (user is null) {
			goto CreateNew;
		} else if (user.Changed) {
			//Asks the user if they want to save or not, cancel feature so the it doesn't change user
			string confirm = await DisplayActionSheet("Unsaved changes, Would you like to save changes?", "Cancel", null, "Yes", "No");
			// User select Cancel does nothing
			if (confirm is "Cancel") {
				return;
			} else if (confirm is "Yes") {
				// checks if there is already a saved path, to just quickly save
				if (File.Exists(fullpath)) {
					QuickSaveClick(sender, e);
					goto CreateNew;
				}
				try {
					using var stream = new MemoryStream(Encoding.Default.GetBytes(user.Save()));
					await fileSaver.SaveAsync($"{user.name}.stk", stream, default);
				} catch (Exception) {
					await DisplayAlert("Fail to Save", "Please enter a valid path to save your file, nothing is going to change.", "OK");
					return;
				}
			}
		}
		// User wants to Create a New Portfolio
		CreateNew:
		string name = await DisplayPromptAsync("Enter a Name for Portfolio", "What's your name?");
		if (name is null) return;
		user = new(name);
		fullpath = "";
		LabelUser.Text = $"Portfolio Name: {user.name}";
		SavePort.IsEnabled = SaveData.IsEnabled = Buy.IsEnabled = Sell.IsEnabled = Entry.IsEnabled = Broker.IsEnabled = Change.IsEnabled = true;
	}

	/// <summary>
	/// Loat Button that the User can load their existing Stock Portfolio
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void LoadClick(object sender, EventArgs e) {
		// First check if there is a Portfolio, then Checks if the Portfoliio has been changed
		if (user is null) {
			goto LoadFile;
		} else if (user.Changed) {
			//Asks the user if they want to save or not, cancel feature so the it doesn't change user
			string confirm = await DisplayActionSheet("Unsaved changes, Would you like to save changes?", "Cancel", null, "Yes", "No");
			// User select Cancel does nothing
			if (confirm is "Cancel") {
				return;
			} else if (confirm is "Yes") {
				// checks if there is already a saved path, to just quickly save
				if (File.Exists(fullpath)) {
					QuickSaveClick(sender, e);
					goto LoadFile;
				}
				try {
					// Save file that the User can choose where
					using var stream = new MemoryStream(Encoding.Default.GetBytes(user.Save()));
					await fileSaver.SaveAsync($"{user.name}.stk", stream, default);
				} catch (Exception) {
					await DisplayAlert("Fail to Save", "Please enter a valid path to save your file, nothing is going to change.", "OK");
					return;
				}
			}
		}
		// Open Loaded Portfolio file
		LoadFile:
		try {
			//Grabs the name of the file
			var fileResult = await FilePicker.PickAsync();
			// Makes sure that there is a name for the file or that the file open ends with ".stk"
			// Otherwise don't change Portfolio
			if (fileResult != null) {
				if (!fileResult.FileName.EndsWith("stk")) throw new Exception();
				fullpath = fileResult.FullPath;
				user = new(fullpath, "stk");
			} else {
				//If the User dosen't select any file
				Console.WriteLine("No file selected.");
			}
		} catch (Exception ex) {
			Console.WriteLine("Error opening file:");
			await DisplayAlert("Error Opening File", "Please open a file ending in \".stk\"", "OK");
			Console.WriteLine(ex);
		}
		LabelUser.Text = $"Portfolio Name: {user.name}";
		SavePort.IsEnabled = SaveData.IsEnabled = Buy.IsEnabled = Sell.IsEnabled = Entry.IsEnabled = Broker.IsEnabled = Change.IsEnabled = true;
	}

	/// <summary>
	/// Quick Save User Portfolio Progress
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void QuickSaveClick(object sender, EventArgs e) {
		//Takes the already save path and update the Portfolio
		if (user is null) return;
		if (user.Changed) {
			if (!File.Exists(fullpath)) {
				SaveClick(sender, e);
				return;
			}
			user.Save(fullpath);
		}
	}

	/// <summary>
	/// Save User Portfolio Progress at a specific location
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void SaveClick(object sender, EventArgs e) {
		// First check if there is a Portfolio, then Checks if the Portfoliio has been changed
		if (user is null) {
			return;
		} else if (user.Changed) {
			SaveAgain:
			// checks if there is already a saved path, to just quickly save
			if (File.Exists(fullpath)) {
				QuickSaveClick(sender, e);
				return;
			}
			try {
				// Save file that the User can choose where
				using var stream = new MemoryStream(Encoding.Default.GetBytes(user.Save()));
				var path = await fileSaver.SaveAsync($"{user.name}.stk", stream, default);

				fullpath = path.FilePath;
			} catch (Exception) {
				// Ask the User if the wanting to Save again because of in valid path
				if (await DisplayAlert("Fail to Save", "Please enter a valid path to save your file, " +
					"Do you want to save again?", "Yes", "Cancel")) {
					goto SaveAgain;
				}
			}
		}
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
	/// Change the User Portfolio name
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void ChangeClick(object sender, EventArgs e) {
		string name = await DisplayPromptAsync("Enter a Name for Portfolio", "Change Portfolio Name?");
		if (name is null) return;
		user.name = name;
		LabelUser.Text = $"Portfolio User: {user.name}";
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
		string apiKey = GetAPIKey();
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

