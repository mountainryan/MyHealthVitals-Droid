using System;
using System.IO;
using OxyPlot;
using MyHealthVitals.iOS;
using Foundation;

[assembly: Xamarin.Forms.Dependency(typeof(FileHelperIos))]

[assembly: Xamarin.Forms.Dependency(typeof(BaseUrl_iOS))]

namespace MyHealthVitals.iOS
{
	public class FileHelperIos : IFileHelper
	{
		public void saveToPdf(PlotModel ecgModel, String fileName)
		{

			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var filePath = Path.Combine(documentsPath, fileName);

			System.Diagnostics.Debug.WriteLine(filePath);

			using (var stream = File.Create(filePath))
			{
				var pdfExporter = new PdfExporter();
				pdfExporter.Height = 150;
				pdfExporter.Width = 320;
				//pdfExporter.
				//pdfExporter.
				pdfExporter.Export(ecgModel, stream);
			}
		}
	}

	public class BaseUrl_iOS : IBaseUrl
	{
		public string Get()
		{
			return NSBundle.MainBundle.BundlePath;
		}
	}
}
