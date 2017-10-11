using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MyHealthVitals.Droid;
using OxyPlot;

[assembly: Xamarin.Forms.Dependency(typeof(BaseUrl_Android))]

[assembly: Xamarin.Forms.Dependency(typeof(FileHelperAndroid))]

namespace MyHealthVitals.Droid
{


	public class FileHelperAndroid : IFileHelper
	{
		public bool checkFileExist(string fileName)
		{
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string filePath = Path.Combine(documentsPath, fileName);
			System.Diagnostics.Debug.WriteLine("IF EXIST filePath == " +filePath);
			System.Diagnostics.Debug.WriteLine("File.Exists(filePath); == " + File.Exists(filePath));

			return File.Exists(filePath);
		}

		public List<string> readFromTxt(string fileName)
		{
			throw new NotImplementedException();
		}

		public byte[] saveToPdf(PlotModel ecgModel, string fileName, string name)
		{
			throw new NotImplementedException();
		}

		public void saveTotxt(List<int> ecgModel, string title, string subtitle, string fileName)
		{
			throw new NotImplementedException();
		}

		public Task<bool> sentToEmail(string fileName)
		{
			throw new NotImplementedException();
		}

		public void setEcgInof(string Patient, string DOB, string Finding, string Recorded, string FindingDetails, string HeartRate, string TestDuration = "30s")
		{
			throw new NotImplementedException();
		}

		public Task<bool> setEmailClient()
		{
			throw new NotImplementedException();
		}
	}

	public class BaseUrl_Android : IBaseUrl
	{
		public string Get()
		{
			return "file:///android_asset/";
		}
	}

}
