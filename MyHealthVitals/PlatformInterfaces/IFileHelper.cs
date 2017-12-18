using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MyHealthVitals
{
	public interface IFileHelper
	{
        void delBLEinfo();
        List<string> getBLEinfo(string devicename);
        void saveBLEinfo(string devicename, int blenum, Guid deviceid);
        Task<bool> offlineSave(Reading data, string method);
        Task<bool> offlineFileSave(FileData data, long ecgid);
        Task<bool> offlineRead();
        Task<bool> dispAlert(String Title, String message, bool tablet, String btn1, String btn2);
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
		Task<bool> SaveFromBytes(byte[] filedata, string fname);
		Task<byte[]> BytesFromFile(string fileName);
        string getFileName(string fname);
        void deleteOfflineFile();
	}

	public interface IBaseUrl { string Get(); }


}
