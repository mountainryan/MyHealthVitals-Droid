using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MyHealthVitals
{
	public interface IFileHelper
	{
		void setEcgInof(String Patient, String DOB, String Finding, String Recorded,
						String FindingDetails, String HeartRate, String TestDuration = "30s");
		List<string> readFromTxt(String fileName);
		void saveTotxt(List<int> ecgModel, string title, string subtitle, String fileName);
		byte[] saveToPdf(OxyPlot.PlotModel ecgModel, String fileName, string name);
		Task<bool> sentToEmail(string fileName);
		bool checkFileExist(string fileName);
		Task<bool> setEmailClient();
		string retGif();
		void copyAsset();
	}

	public interface IBaseUrl { string Get(); }


}
