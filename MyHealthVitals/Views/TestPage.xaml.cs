using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class TestPage : ContentPage
	{
		void btnConectCLicked(object sender, System.EventArgs e)
		{
			//bleManager = new BleManager();
			//bleManager.connect(this);
		}

		void btnCancelThisPageClicked(object sender, System.EventArgs e)
		{
			this.Navigation.PopModalAsync(true);
		}

		public TestPage()
		{
			InitializeComponent();

			//var htmlsource = new HtmlWebViewSource();
			//htmlsource.BaseUrl = DependencyService.Get<IBaseUrl>().Get();
			////htmlsource.Html = @"<html><body>
			////							<img src=""https://media.giphy.com/media/xTk9ZvMnbIiIew7IpW/giphy.gif"" alt=""Mountain View"" style=""width:304px;height:228px;"">
			////							</body></html>";

			////htmlsource.Html = 


			
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			//this.bleManager.disconnectDevice();
		}

		// BluetoothCallBackUpdatable interface methods
		public void ShowMessageOnUI(String message)
		{
			Debug.WriteLine(message);
		}

		public void SYS_DIA_BPM_updated()
		{
			Debug.WriteLine("update UI showoing readings");
		}
	}
}
