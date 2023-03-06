# Stock Market Simulator
Authors: Monthon Paul

Current Version: 1.0

Last Updated: 03/05/2023

# Purpose: 

This Applicaiton is to replicate the Stock Market Game of using a Simulator Application, it's to have the basis idea of how 
the Stock Market works. For the program I am grabing all the Stock Data from Alpha Vantage API. The intend is it to be a small application where 
the User can buy/sell stocks to make a profit. The User can save their process into a ".stk" in JSON format, example being "stock.stk"

For more details, please look in the "About"/"How to Use" in the help tab.
 
# How the spreadsheet works:
Users will start with a balance of $1,000 where they can play the game in order to make profit or lose profit. It's a simulator so the User can have experience learning how the Stock Market works and should learn how it works.

In the file tab, there is "New", "Open", "Save", and "Save As".  New creates a New Portfolio, Open allows user to open their Data, Save saves the the User Data, and Save As lets you save to a specific location.

For more detailed explanations, take a look into the help tab for the Stock Market Simulator.

# How to Setup:

The Project was implemented in the .NET 7.0 Framwork & uses .NET MAUI for GUI, then require a compatible .NET SDK
This Program can be run in the Visual Studio IDE, or can be build/run by the Command line

#### Build 

First install .NET MAUI workload with the dotnet CLI 

```
dotnet workload install maui
```
Verify and install missing components with maui-check command line utility.
```
dotnet tool install -g redth.net.MAUI.check
maui-check
```

#### Run
Run the MAUI app either on Windows or MacOS (but first locate your directory for StockMarketSim)

For MacOS
```
cd StockMarketSim
dotnet build -t:Run -f net6.0-maccatalyst
```

For Windows
```
cd StockMarketSim
dotnet build -t:Run -f net6.0-windows
```
