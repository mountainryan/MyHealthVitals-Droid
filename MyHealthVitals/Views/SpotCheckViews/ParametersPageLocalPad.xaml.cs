using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Diagnostics;


namespace MyHealthVitals
{
	public partial class ParametersPageLocalPad : ContentPage
	{
		class CategoryLocal
		{
			public int id;
			public string Name { get; set; }
			public CategoryLocal(int id, string name)
			{
				this.id = id;
				this.Name = name;
			}
		}
		ObservableCollection<CategoryLocal> categories_local = new ObservableCollection<CategoryLocal>();
		private  Reading[] allReadings;
		public ParametersPageLocalPad()
		{
            NavigationPage.SetHasNavigationBar(this, false);
			/*
            cont.HeightRequest *= Screensize.heightfactor;
            label.FontSize *= Screensize.heightfactor;
            label.HeightRequest *= Screensize.heightfactor;
            arrow.HeightRequest *= Screensize.heightfactor;
            arrow.WidthRequest *= Screensize.widthfactor;
            */
			
			InitializeComponent();

			FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
			titlebtn.FontSize = 30 * Screensize.heightfactor;
            backbtn.FontSize = 30 * Screensize.heightfactor;

			this.allReadings = ParametersPageLocal.allReadings;
			sycnwithCloud();

		}

		public async void sycnwithCloud()
		{
			layoutLoading.IsVisible = true;
			if (allReadings == null || logcalParameteritem.localhashmap.Count() > 100)
			{
				logcalParameteritem.localhashmap.Clear();
				//allReadings = await Reading.GetAllReadingsFromService();
			}
			Debug.WriteLine("allReadings == " + allReadings);

			categories_local.Add(new CategoryLocal(1, "Blood Pressure"));
			categories_local.Add(new CategoryLocal(2, "SpO2"));
			categories_local.Add(new CategoryLocal(3, "Heart Rate"));
			categories_local.Add(new CategoryLocal(4, "Temperature"));
			categories_local.Add(new CategoryLocal(5, "Weight/BMI"));
			categories_local.Add(new CategoryLocal(8, "Glucose"));
			categories_local.Add(new CategoryLocal(10, "ECG"));

			parameterListView.ItemsSource = categories_local;
			layoutLoading.IsVisible = false;
            //set the height of these elements on screen


		}
		void btnPrevClicked(object sender, System.EventArgs e)
		{
			Navigation.PopAsync();
		}
		async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
		{
			//	var newPage = new ParameterItemDetail(((CategoryLocal)e.Item).id);
			if (allReadings == null)
			{
				layoutLoading.IsVisible = true;
			}


			await Task.Delay(1).ContinueWith(_ =>
			{
				//PushData(e);
				if (allReadings == null)
				{
					int index = Task.WaitAny(Task_vars.tasks);
				}

			});

			layoutLoading.IsVisible = false;

			if (Device.Idiom == TargetIdiom.Tablet)
			{
				var newPage = new ParameterItemDetailNew(((CategoryLocal)e.Item).id, allReadings);
				await this.Navigation.PushAsync(newPage);
			}
			else
			{
				var newPage = new ParameterItemDetailNew(((CategoryLocal)e.Item).id, allReadings);
				await this.Navigation.PushAsync(newPage);
			}

		}

	}
}
