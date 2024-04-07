using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace StockMarketSim;

/// <summary>
/// Stock Data shown in Candle Stick graph
/// </summary>
public partial class StockChartViewModel {

	public ObservableCollection<FinancialPoint> FinancialDataList { get; set; }

	public StockChartViewModel() {
		FinancialDataList = [];
		Series = [ new CandlesticksSeries<FinancialPoint> { Values = FinancialDataList } ];
	}
	
	public Axis[] XAxes { get; set; } = [
		new Axis {
			LabelsRotation = 15,
			Labeler = value => new DateTime((long)value).ToString("MMM dd"),
			LabelsPaint = Application.Current.RequestedTheme == AppTheme.Dark ? new SolidColorPaint(SKColors.White) : new SolidColorPaint(SKColors.Black),
            // set the unit width of the axis to "days"
            // since our X axis is of type date time and 
            // the interval between our points is in days
            UnitWidth = TimeSpan.FromDays(1).Ticks,
		}
	];

	public ISeries[] Series { get; set; }
}