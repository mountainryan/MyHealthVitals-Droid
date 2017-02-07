using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

using Xamarin.Forms;

namespace MyHealthVitals
{

	public class Employee
	{
		public string DisplayName { get; set; }
	}

	public partial class ParameterListPage : ContentPage
	{
		void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
		{
			Category category = (Category)e.Item;
			Debug.WriteLine(category.Name);
		}

		ObservableCollection<Category> categories = new ObservableCollection<Category>();

		public ParameterListPage()
		{
			InitializeComponent();



			this.layoutLoading.IsVisible = true;
		}

		protected async override void OnAppearing()
		{
			base.OnAppearing();

			var cats = await Category.callServiceToGetCategories();

			foreach (var cat in cats)
			{
				//cat.Name
				categories.Add(cat);
			}
			parameterListView.ItemsSource = categories;

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				layoutLoading.IsVisible = false;

				//var windowSize = this.Content.Bounds.Size.Height;

				this.parameterListView.HeightRequest = this.Content.Bounds.Size.Height - this.lblLoadingMessage.Height - this.lblLoadingMessage.Margin.Top*2;

			});
		}

		//public void OnCellClicked(object sender, EventArgs e)
		//{
		//	var b = (Button)sender;
		//	var t = b.CommandParameter;
		//	((ContentPage)((ListView)((StackLayout)b.Parent).Parent).Parent).DisplayAlert("Clicked", t + " button was clicked", "OK");
		//	Debug.WriteLine("clicked" + t);
		//}
	}
}
