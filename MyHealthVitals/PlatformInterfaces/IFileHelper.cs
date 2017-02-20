using System;
namespace MyHealthVitals
{
	public interface IFileHelper
	{
		void saveToPdf(OxyPlot.PlotModel ecgModel,String fileName);
	}
}
