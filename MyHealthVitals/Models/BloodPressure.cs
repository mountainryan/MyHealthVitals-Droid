using System;
namespace MyHealthVitals
{
	public class BloodPressure
	{
		public int systolic { get; set;}
		public int diastolic{ get; set; }
		public DateTime date { get; set; }

		public BloodPressure(DateTime date, int sys, int dia)
		{
			this.diastolic = dia;
			this.date = date;
			this.systolic = sys;
		}
	}
}
