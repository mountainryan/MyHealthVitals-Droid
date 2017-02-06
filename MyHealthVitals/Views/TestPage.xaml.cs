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

		//private BleManager bleManager;

		protected async override void OnAppearing()
		{
			base.OnAppearing();
		}
	}
}
