using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using CommunityToolkit.Maui.Storage;
using Stock;
using CommunityToolkit.Maui.Views;

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
			bool confirm = await DisplayAlert("Unsaved changes", "Would you like to save changes?", "Yes", "No");
			// User select Cancel does nothing
			if (confirm) {
				// checks if there is already a saved path, then just quickly save
				if (File.Exists(fullpath)) {
					QuickSaveClick(sender, e);
					goto CreateNew;
				}
				try {
					using var stream = new MemoryStream(Encoding.Default.GetBytes(user.Save()));
					var path = await fileSaver.SaveAsync($"{user.name}.stk", stream, default);
					if (!path.IsSuccessful) {
						await DisplayAlert("Did not Save", "Didn't save the file, proceed in creating a new Portfolio.", "OK");
					}
				} catch (Exception) {
					await DisplayAlert("Fail to Save", "Please enter a valid path to save your file, nothing is going to change.", "OK");
					return;
				}
			}
		}
		// User wants to Create a New Portfolio
		CreateNew:
		string name = await DisplayPromptAsync("Enter a Name for Portfolio", "What's your name?");
		if (name is null or "") return;
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
			bool confirm = await DisplayAlert("Unsaved changes", "Would you like to save changes?", "Yes", "No");
			// User select Cancel does nothing
			if (confirm) {
				// checks if there is already a saved path, to just quickly save
				if (File.Exists(fullpath)) {
					QuickSaveClick(sender, e);
					goto LoadFile;
				}
				try {
					// Save file that the User can choose where
					using var stream = new MemoryStream(Encoding.Default.GetBytes(user.Save()));
					var path = await fileSaver.SaveAsync($"{user.name}.stk", stream, default);
					if (!path.IsSuccessful) {
						await DisplayAlert("Did not Save", "Didn't save the file, proceed in loading a Portfolio.", "OK");
					}
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
			if (fileResult is not null) {
				if (!fileResult.FileName.EndsWith("stk")) throw new Exception();
				fullpath = fileResult.FullPath;
				user = new(fullpath, "stk");
				LabelUser.Text = $"Portfolio Name: {user.name}";
				SavePort.IsEnabled = SaveData.IsEnabled = Buy.IsEnabled = Sell.IsEnabled = Entry.IsEnabled = Broker.IsEnabled = Change.IsEnabled = true;
			} else {
				user.Changed = true;
				await DisplayAlert("Did not Open file", "Didn't select a Portfolio file, nothing is going to change.", "OK");
			}
		} catch (Exception) {
			await DisplayAlert("Error Opening File", "Please open a file ending in \".stk\"", "OK");
		}
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
			} else {
				user.Save(fullpath);
			}
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
				if (!path.IsSuccessful) {
					user.Changed = true;
					if (await DisplayAlert("Fail to Save", "Please enter a valid name to save your file, " +
					"Do you want to save again?", "Yes", "Cancel")) {
						goto SaveAgain;
					}
				}
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
		// Load HTML format for information
		string HTML = LoadHtml("About.html");
		// Add a button functions, i.e close popup
		Button button = new Button {
			Text = "Close",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			WidthRequest = 500,
			HeightRequest = 50,
			BackgroundColor = new Color(245, 245, 245)
		};

		// Display a Popup displaying "about" the Program
		Popup about = new Popup() {
			CanBeDismissedByTappingOutsideOfPopup = true,
			Size = new Size(500, 500),
			Content = new StackLayout {
				BackgroundColor = new Color(255, 255, 255),
				Children = {
					//Using  HTML format for information
					new WebView {
						HeightRequest= 450,
						Source = new HtmlWebViewSource {
							Html = HTML
						}
					},
					button, // show button
				}
			}
		};
		// Event to trigger the close of the popup
		button.Clicked += (sender, args) => about.Close();
		this.ShowPopup(about);
	}

	/// <summary>
	/// Help menu to display the Rules of the Stock Market Simulator
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private void HTPClick(object sender, EventArgs e) {
		// Load HTML format for information
		string HTML = LoadHtml("HowToPlay.html");
		// Add a button functions, i.e close popup
		Button button = new Button {
			Text = "Close",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			WidthRequest = 520,
			HeightRequest = 50,
			BackgroundColor = new Color(245, 245, 245) // Whitesmoke
		};

		// Display a Popup displaying "How to Play" the Program
		Popup HTP = new Popup() {
			CanBeDismissedByTappingOutsideOfPopup = true,
			Size = new Size(520, 700),
			Content = new StackLayout {
				BackgroundColor = new Color(255, 255, 255), // White
				Children = {
					//Using  HTML format for information
					new WebView {
						HeightRequest= 650,
						Source = new HtmlWebViewSource {
							Html = HTML
						}
					},
					button, // show button
				}
			}
		};
		button.Clicked += (sender, args) => HTP.Close();
		this.ShowPopup(HTP);
	}

	/// <summary>
	/// Change the User Portfolio name
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void ChangeClick(object sender, EventArgs e) {
		string name = await DisplayPromptAsync("Enter a Name for Portfolio", "Change Portfolio Name?");

		if (name is null or "") return;
		if (fullpath is not "") {
			string filename = await DisplayPromptAsync("Update Portfolio filename", "Want to update your filename?", placeholder: name);
			if (filename is not null or "") {
				RenameFile(fullpath, filename);
			} else {
				await DisplayAlert("Didn't Update file name", "Haven't change the file name, Porfolio name have been updated.", "OK");
			}
		}
		user.name = name;
		user.Changed = true;
		LabelUser.Text = $"Portfolio User: {user.name}";
	}

	/// <summary>
	/// Load the HTML file located in "Resources/Raw/**"
	/// Using StreamReader, locate the html file in system then read the file
	/// to save onto a string.
	/// </summary>
	/// <param name="html">Specifc file for html opening</param>
	private string LoadHtml(string html) {
		using var stream = FileSystem.OpenAppPackageFileAsync(html);
		using var reader = new StreamReader(stream.Result);

		string HTML = reader.ReadToEnd();
		return HTML;
	}

	/// <summary>
	/// Rename a specific file due to the name change from the User to their Portfolio
	/// </summary>
	/// <param name="filePath"> filepath of the User Portfolio </param>
	/// <param name="newName"> New name for the User Portfolio </param>
	private void RenameFile(string filePath, string newName) {
		string directory = Path.GetDirectoryName(filePath);
		string fileName = Path.GetFileNameWithoutExtension(filePath);
		string extension = Path.GetExtension(filePath);

		string[] files = Directory.GetFiles(directory, "*.stk");

		foreach (string file in files) {
			if (Path.GetFileNameWithoutExtension(file) == fileName) {
				string newFilePath = Path.Combine(directory, newName + extension);
				File.Move(file, newFilePath);
				fullpath = newFilePath;
				break;
			}
		}
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

