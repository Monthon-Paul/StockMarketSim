using System.Text;
using System.Collections.ObjectModel;
using OoplesFinance.YahooFinanceAPI;
using OoplesFinance.YahooFinanceAPI.Enums;
using LiveChartsCore.Defaults;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using Stock;

namespace StockMarketSim;

/// <summary>
/// StockMarketGUI for StockMarketSim
///
/// This Applicaiton is to replicate the Stock Market Simulator,
/// it's to have the basis idea of how the Stock Market works.
/// For the program I am grabing all the Stock Data from Yahoo Finance API.
/// The intend is it to be a small application where
/// the User can Buy/Sell stocks to make a profit.
/// The User can save their Portfolio into a ".stk" file in JSON format.
///
/// Author: Monthon Paul
/// Version: May 5, 2023
/// </summary>
public partial class MainPage : ContentPage {

	// Initialize variables
	private string fullpath;
	private Portfolio user;
	private readonly IFileSaver fileSaver;
	private StockSearch FrontSTK;
	private Portfolio.StockData stock;
	private readonly YahooClient yahooClient;
	private ObservableCollection<StockSearch> Ticker { get; set; }
	private ObservableCollection<PortInfo> Port { get; set; }

	public MainPage(IFileSaver fileSaver) {
		InitializeComponent();
		Ticker = [];
		Port = [];
		yahooClient = new();
		this.fileSaver = fileSaver;
	}

	/// <summary>
	/// New Button that User can click to make a New Portfolio
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void NewClick(object sender, EventArgs e) {
		// First check if there is a Portfolio, then Checks if the Portfoliio has been changed
		if (user is not null && user.Changed) {
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
					var path = await fileSaver.SaveAsync($"{user.Name}.stk", stream, default);
					if (!path.IsSuccessful)
						await DisplayAlert("Did not Save", "Didn't save the file, proceed in creating a new Portfolio.", "OK");
				} catch (Exception) {
					await DisplayAlert("Fail to Save", "Please enter a valid path to save your file, nothing is going to change.", "OK");
					return;
				}
			}
		}
		// User wants to Create a New Portfolio
		CreateNew:
		string name = await DisplayPromptAsync("Enter a Name for Portfolio", "What's your name?");
		if (string.IsNullOrEmpty(name))
			return;
		user = new(name);

		// Add Portfolio information
		Port.Clear();
		Port.Add(new PortInfo { Name = "Balance:", Balance = user.UserCashBalance.ToString() });
		foreach (var item in user.GetAllShares()) {
			var result = await user.GetTotalStockPrice(item.Key);
			Port.Add(new PortInfo { Name = $"{result.Item1}", Amount = $"{item.Value} Shares", Balance = $"${result.Item2}" });
		}
		InfoView.ItemsSource = Port;

		fullpath = "";
		LabelUser.Text = $"Portfolio Name: {user.Name}";
		SavePort.IsEnabled = SaveData.IsEnabled = Buy.IsEnabled = Sell.IsEnabled = Entry.IsEnabled = Broker.IsEnabled = Change.IsEnabled = true;
	}

	/// <summary>
	/// Loat Button that the User can load their existing Stock Portfolio
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void LoadClick(object sender, EventArgs e) {
		// First check if there is a Portfolio, then Checks if the Portfoliio has been changed
		if (user is not null && user.Changed) {
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
					var path = await fileSaver.SaveAsync($"{user.Name}.stk", stream, default);
					if (!path.IsSuccessful)
						await DisplayAlert("Did not Save", "Didn't save the file, proceed in loading a Portfolio.", "OK");
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
				if (!fileResult.FileName.EndsWith("stk"))
					throw new Exception();
				fullpath = fileResult.FullPath;
				user = new(fullpath, "stk");

				// Add Portfolio information
				Port.Clear();
				Port.Add(new PortInfo { Name = "Balance:", Balance = $"${user.UserCashBalance}" });
				foreach (var item in user.GetAllShares()) {
					var result = await user.GetTotalStockPrice(item.Key);
					Port.Add(new PortInfo { Name = $"{result.Item1}", Amount = $"{item.Value} Shares", Balance = $"${result.Item2}" });
				}
				InfoView.ItemsSource = Port;

				LabelUser.Text = $"Portfolio Name: {user.Name}";
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
		if (user is null)
			return;
		if (user.Changed) {
			if (!File.Exists(fullpath))
				SaveClick(sender, e);
			else
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
		if (user is null)
			return;
		else if (user.Changed) {
			SaveAgain:
			// checks if there is already a saved path, to just quickly save
			if (File.Exists(fullpath)) {
				QuickSaveClick(sender, e);
				return;
			}
			try {
				// Save file that the User can choose where
				using var stream = new MemoryStream(Encoding.Default.GetBytes(user.Save()));
				var path = await fileSaver.SaveAsync($"{user.Name}.stk", stream, default);
				if (!path.IsSuccessful) {
					user.Changed = true;
					if (await DisplayAlert("Fail to Save", "Please enter a valid name to save your file, " +
					"Do you want to save again?", "Yes", "Cancel"))
						goto SaveAgain;
				}
				fullpath = path.FilePath;
			} catch (Exception) {
				// Ask the User if the wanting to Save again because of in valid path
				if (await DisplayAlert("Fail to Save", "Please enter a valid path to save your file, " +
					"Do you want to save again?", "Yes", "Cancel"))
					goto SaveAgain;
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
		Button button = new() {
			Text = "Close",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			WidthRequest = 500,
			HeightRequest = 50,
			BackgroundColor = new Color(245, 245, 245)
		};

		// Display a Popup displaying "about" the Program
		Popup about = new() {
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
		Button button = new() {
			Text = "Close",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			WidthRequest = 520,
			HeightRequest = 50,
			BackgroundColor = new Color(245, 245, 245) // Whitesmoke
		};

		// Display a Popup displaying "How to Play" the Program
		Popup HTP = new() {
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

		if (string.IsNullOrEmpty(name))
			return;
		if (!string.IsNullOrEmpty(fullpath)) {
			string filename = await DisplayPromptAsync("Update Portfolio filename", "Want to update your filename?", placeholder: name);
			if (!string.IsNullOrEmpty(fullpath))
				RenameFile(fullpath, filename);
			else
				await DisplayAlert("Didn't Update file name", "Haven't change the file name, Porfolio name have been updated.", "OK");
		}
		user.Name = name;
		user.Changed = true;
		LabelUser.Text = $"Portfolio User: {user.Name}";
	}

	/// <summary>
	/// Load the HTML file located in "Resources/Raw/**"
	/// Using StreamReader, locate the html file in system then read the file
	/// to save onto a string.
	/// </summary>
	/// <param name="html">Specifc file for html opening</param>
	private static string LoadHtml(string html) {
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
	/// Buy Stock Shares according to the amount of Shares display
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void BuyClick(object sender, EventArgs e) {
		if (user is null)
			return;
		if (!string.IsNullOrEmpty(Entry.Text) && int.TryParse(Entry.Text, out int amount)) {
			user.Brokerslider(Broker.IsToggled);
			try {
				await user.BuyStocks(FrontSTK.Symbol, amount);

				// Update Portfolio information
				Port.Clear();
				Port.Add(new PortInfo { Name = "Balance:", Balance = $"${user.UserCashBalance}" });
				foreach (var item in user.GetAllShares()) {
					var result = await user.GetTotalStockPrice(item.Key);
					Port.Add(new PortInfo { Name = $"{result.Item1}", Amount = $"{item.Value} Shares", Balance = $"${result.Item2}" });
				}
				InfoView.ItemsSource = Port;
			} catch (Exception exp) {
				await DisplayAlert("Problem Purchasing Shares", exp.Message, "OK");
			}
		}
	}

	/// <summary>
	/// Sell Stock Shares according to the amount of Shares display
	/// </summary>
	/// <param name="sender">Pointer to the Button</param>
	/// <param name="e">triggle an event</param>
	private async void SellClick(object sender, EventArgs e) {
		if (user is null)
			return;
		if (!string.IsNullOrEmpty(Entry.Text) && int.TryParse(Entry.Text, out int amount)) {
			user.Brokerslider(Broker.IsToggled);
			try {
                await user.SellStocks(FrontSTK.Symbol, amount);

				// Update Portfolio information
				Port.Clear();
				Port.Add(new PortInfo { Name = "Balance:", Balance = $"${user.UserCashBalance}" });
				foreach (var item in user.GetAllShares()) {
					var result = await user.GetTotalStockPrice(item.Key);
					Port.Add(new PortInfo { Name = $"{result.Item1}", Amount = $"{item.Value} Shares", Balance = $"${result.Item2}" });
				}
				InfoView.ItemsSource = Port;
			} catch (Exception exp) {
				await DisplayAlert("Problem Selling Shares", exp.Message, "OK");
			}
		}
	}

	/// <summary>
	/// On the Search Bar, User will search either specific Ticker symbols or companies,
	/// That will allow to display the best-matching symbols and market information based on keywords of user choice.
	/// </summary>
	/// <param name="sender"> Pointer to the Search Bar</param>
	/// <param name="e"> triggle an event </param>
	private async void Search(object sender, EventArgs e) {
		SearchBar searchBar = sender as SearchBar;
		var query = searchBar.Text;
		if (!string.IsNullOrEmpty(query)) {
			// Connect ot API
			var autoCompleteList = await yahooClient.GetAutoCompleteInfoAsync(query);
			// Clear the Data Structue due to new keywords from User
			Ticker.Clear();
			foreach (var ele in autoCompleteList)
				Ticker.Add(new StockSearch { Symbol = ele.Symbol, Name = ele.Name });
			ListView.ItemsSource = Ticker;
		} else {
			// If the Search Bar has nothing, than no Data
			Ticker.Clear();
			ListView.ItemsSource = Ticker;
		}
	}

	/// <summary>
	/// Display Stock Chart UI from given ticker being clicked
	/// </summary>
	/// <param name="symbol"></param>
	/// <exception cref="NotImplementedException"></exception>
	private async void DisplayStockChart(string symbol) {
		// TODO: try to finish displaying stock into the chart
		var startDate = DateTime.Now.AddMonths(-1);
		var historicalDataList = await yahooClient.GetHistoricalDataAsync(symbol, DataFrequency.Daily, startDate);
		var viewModel = BindingContext as StockChartViewModel;
		viewModel.FinancialDataList.Clear();
		// Assuming there's a ViewModel property or way to access the StockChartViewModel instance 
		foreach (var data in historicalDataList)
			viewModel.FinancialDataList.Add(new FinancialPoint {
				Date = data.Date,
				High = data.High,
				Low = data.Low,
				Open = data.Open,
				Close = data.Close
			});
	}

	/// <summary>
	/// When User Select an item on the list, the Stock Data will be display on the Graph with it's data
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
		if (e.SelectedItem != null) {
			FrontSTK = e.SelectedItem as StockSearch;
			try {
				// Display Stock Data
				stock = await Portfolio.GetStockData(FrontSTK.Symbol);
				string[] display = DisplayStock(stock.Change, stock.Percent, stock.Date);
				StockTicker.Text = $"{stock.Symbol}:";
				StockName.Text = FrontSTK.Name;
				StockPrice.Text = $"{stock.Price:C}";
				StockChange.Text = display[0];
				StockPercent.Text = display[1];
				StockDate.Text = display[2];
				StockAsk.Text = $"Ask Size: {stock.AskSize}";
				StockBid.Text = $"Bid Size: {stock.BidSize}";

				DisplayStockChart(stock.Symbol);

				ListView.SelectedItem = null;
			} catch (Exception) {
				ListView.SelectedItem = null;
				return;
			}
		}
	}

	/// <summary>
	/// Modify or format chane, percent, and date depending on the Stock being positive or negative,
	/// output an array with the store return to each modification
	/// </summary>
	/// <param name="change"> Stock Change number</param>
	/// <param name="percent"> Stock Change Percentage</param>
	/// <param name="date"> Date of time for Stock</param>
	/// <returns> An String Array with modify and formated change, percent, date </returns>
	private string[] DisplayStock(decimal change, string percent, DateTime date) {
		string[] result = new string[3];
		// Remove the percentage sign and parse the numeric value
		double numericValue = double.Parse(percent.TrimEnd('%'));

		// Round the numeric value to two decimal places
		double roundedValue = Math.Round(Math.Abs(numericValue), 2);

		// Convert the rounded value back to a formatted percentage string
		string formatPercent = roundedValue.ToString("F2") + "%";

		// Determine modifying and formating to positive or negative
		if (change < 0.0m) {
			// Negative modification
			StockPercent.TextColor = StockChange.TextColor = StockDate.TextColor = new Color(175, 15, 15, 255);
			Background.BackgroundColor = new Color(255, 230, 230, 255);
			result[0] = change.ToString("0.####");
			result[1] = "\u2193" + formatPercent; // down arrow
		} else {
			// Positive modification
			StockPercent.TextColor = StockChange.TextColor = StockDate.TextColor = new Color(20, 175, 50, 255);
			Background.BackgroundColor = new Color(230, 255, 230, 255);
			result[0] = $"+{change:0.####}";
			result[1] = "\u2191" + formatPercent; // up arrow
		}
		result[2] = date.ToString("D");
		return result;

	}
}

