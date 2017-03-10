using System;
using System.ComponentModel;

namespace MyHealthVitals
{

	public class SpirometerReading: INotifyPropertyChanged
	{
		public DateTime Date { get; set; }
		public decimal Pef { get; set; }
		public decimal Fev1 { get; set; }

		public SpirometerReading(DateTime newDate, decimal pef, decimal fev1)
		{
			this.Date = newDate;
			this.Pef = pef;
			this.Fev1 = fev1;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public int _index;

		public int index { 
			get {
				return _index;
			}
			set {
				if (value == _index) return;

				_index = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(index)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(indexText)));
				//PropertyChanged(this, new PropertyChangedEventArgs
			} 
		}

		//private string _value;

		//public string Value
		//{
		//	get { return _value; }
		//	set
		//	{
		//		if (value == _value) return;
		//		_value = value;
		//		OnPropertyChanged();
		//	}
		//}

		public string indexText {
			get {

				var retValue = "";

				switch (index) { 
					case 0:{
							retValue = "1st reading";
							break;
						}
						case 1:
						{
							retValue = "2nd reading";
							break;
						}
						case 2:
						{
							retValue = "3rd reading";
							break;
						}
					default:
						{
							retValue = index + "th reading";
							break;
						}
				}

				return retValue;
			} 
		}

		//public string dateString1 { get; set; }

		public string pefString { get { return ((int)Pef).ToString(); } }
		public string fev1String { get { return Math.Round(Fev1, 2).ToString(); } }
		public string dateString { get { return Date.ToString("MM/dd/yyyy hh:mm tt"); }  }

		public string color
		{
			get
			{
				String color1 = "#11FF00";

				try
				{
					double normalPefAtThatTime = (double)Demographics.sharedInstance.getNormalPefForDate(this.Date);

					if (!(normalPefAtThatTime > 0))
						return color1;

					if (normalPefAtThatTime * 0.6 < (double)Pef && (double)Pef < normalPefAtThatTime * 0.8)
					{
						color1 = "#FFFF00";
					}
					if (normalPefAtThatTime * 0.6 > (double)Pef)
					{
						//#ff0000
						color1 = "Red";
					}
				}
				catch {
					System.Diagnostics.Debug.WriteLine("no established calibrated value.");
				}

				return color1;
			}
		}

		//public int getColor()
		//{
		//	// green color

		//}

		//private string name;
		//public string Name
		//{
		//	get
		//	{
		//		return this.name;
		//	}
		//	set
		//	{
		//		this.name = value;
		//	}
		//}

		//public void saveSpirometerToLocal()
		//{
		//var calbratedReadingDefaults = NSUserDefaults.StandardUserDefaults;

		//calbratedReadingDefaults.SetString(this.Date.ToString(), "MyNormalDate");
		//calbratedReadingDefaults.SetDouble(this.Pef, "MyNormalPEF");
		//calbratedReadingDefaults.SetDouble(this.Fev1, "MyNormalFEV1");

		//calbratedReadingDefaults.Synchronize();

		//System.IO.Con

		//System.Console.wriet
		//}

		//public static SpirometerReading getCalibratedReading()
		//{
		//	var calbratedReading = NSUserDefaults.StandardUserDefaults;

		//	SpirometerReading calReading = new SpirometerReading();

		//	String mydate = calbratedReading.StringForKey("MyNormalDate") ?? "1950/01/01 12:00";
		//	calReading.Date = DateTime.Parse(mydate);
		//	calReading.Fev1 = calbratedReading.DoubleForKey("MyNormalFEV1");
		//	calReading.Pef = calbratedReading.DoubleForKey("MyNormalPEF");

		//	return calReading;
		//}

		//public async void saveReadingIntoServer()
		//{
		//	Reading fevReading = new Reading { CategoryId = 9, Date = this.Date, Source = "Device", EnglishValue = Convert.ToDecimal(this.Fev1), ValueType = "FEV1" };
		//	Reading pefReading = new Reading { CategoryId = 9, Date = this.Date, Source = "Device", EnglishValue = Convert.ToDecimal(this.Pef), ValueType = "PEF" };

		//	await fevReading.PostAsync(LoginController.credential);
		//	await pefReading.PostAsync(LoginController.credential);
		//}
	}
}
