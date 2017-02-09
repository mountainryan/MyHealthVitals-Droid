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
		void Handle_ItemAppearing(object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
		{
			Debug.WriteLine("here");
		}

		void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
		{
			//throw new NotImplementedException();
			//System.con
			Debug.WriteLine("herere");

			var newPage = new ParameterItemDetail();
			newPage.Title = "SpO2 Data List";

			this.Navigation.PushAsync(newPage);
		}

		public ParametersPage()
		{
			InitializeComponent();
		}

		//private Ob
		ObservableCollection<Category> categories = new ObservableCollection<Category>();
		protected async override void OnAppearing()
		{
			base.OnAppearing();

			var cats = await Category.callServiceToGetCategories();

			foreach (var cat in cats)
			{
				//cat.Name
				categories.Add(cat);
			}

			//DataTemplate template = new DataTemplate();
			////template.

			parameterListView.ItemsSource = categories;
			//parameterListView.ItemTemplate

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				layoutLoading.IsVisible = false;
				this.parameterListView.HeightRequest = this.Content.Bounds.Size.Height - this.lblLoadingMessage.Height - this.lblLoadingMessage.Margin.Top * 2;
			});
		}
	}
}
