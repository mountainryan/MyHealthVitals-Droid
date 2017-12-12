using System;
using System.Collections.Generic;

namespace MyHealthVitals
{
	public class ParameterDetailItem
	{
		public string date { get; set; }
		public string firstItem { get; set;}
		public string secondItem { get; set; }
		public long categoryId;
        public string date_nosec { get; set; }
        public long getID { get; set; }
	}
	public class logcalParameteritem { 
		static public Dictionary<int, List<ParameterDetailItem>> localhashmap = new Dictionary<int , List<ParameterDetailItem>>();
		static public List<SpirometerReading> localspirometerList = new List<SpirometerReading>();

	}

}
