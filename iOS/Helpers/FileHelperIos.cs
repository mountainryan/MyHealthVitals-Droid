using System;
using System.IO;
using OxyPlot;
using MyHealthVitals.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(FileHelperIos))]

namespace MyHealthVitals.iOS
{
	public class FileHelperIos : IFileHelper
	{
		public void saveToPdf(PlotModel ecgModel,String fileName) {

			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var filePath = Path.Combine(documentsPath, fileName);

			System.Diagnostics.Debug.WriteLine(filePath);

			using (var stream = File.Create(filePath))
			{
				var pdfExporter = new PdfExporter();
				pdfExporter.Height = 300;
				pdfExporter.Width = 320;
				//pdfExporter.
				//pdfExporter.
				pdfExporter.Export(ecgModel, stream);
			}
		}
	}
}
