using System;
namespace MyHealthVitals
{

	public class SpirometerReading
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

		public string pefString { get { return ((int)Pef).ToString(); } }
		public string fev1String { get { return Math.Round(Fev1, 1).ToString(); } }
		public string dateString { get { return Date.ToString("MM/dd/yyyy hh:mm tt"); } }

		public string color
		{
			get
			{
				String color = "Green";
				// replace 500 with user defaults saved value in production
				if ((decimal)(750 * 0.6) < Pef && Pef < (decimal)(750 * 0.8))
				{
					color = "#FFA500";
				}
				if ((decimal)(750 * 0.6) > this.Pef)
				{
					color = "Red";
				}

				return color;
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
