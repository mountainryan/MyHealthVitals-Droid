using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace MyHealthVitals
{

	public partial class ParametersPage : ContentPage
	{
		ObservableCollection<Category> categories = new ObservableCollection<Category>();

		public ParametersPage()
		{
			InitializeComponent();
			CallAPi();
		}

		private async void CallAPi() {
			layoutLoading.IsVisible = true;
			var cats = await Category.callServiceToGetCategories();

			foreach (var cat in cats)
			{
				//cat.Name
				categories.Add(cat);
			}

			//DataTemplate template = new DataTemplate();
			////template.

			parameterListView.ItemsSource = categories;

			layoutLoading.IsVisible = false;
			this.parameterListView.HeightRequest = this.Content.Bounds.Size.Height - this.lblLoadingMessage.Height - this.lblLoadingMessage.Margin.Top * 2;
			//parameterListView.ItemTemplate

			//Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			//{
				
			//});
		}

		// listview Delegates
		void Handle_ItemAppearing(object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
		{
			Debug.WriteLine("Handle_ItemAppearing:");
		}

		void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
		{
			var newPage = new ParameterItemDetail(((Category)e.Item).Id);
			newPage.Title = "SpO2 Data List";

			this.Navigation.PushAsync(newPage);
		}
	}
}
