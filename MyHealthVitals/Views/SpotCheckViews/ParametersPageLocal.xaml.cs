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
	public partial class ParametersPageLocal : ContentPage
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
		public static Reading[] allReadings;
		public ParametersPageLocal()
		{
            NavigationPage.SetHasNavigationBar(this, false);
            //label.FontSize *= Screensize.heightfactor;
			InitializeComponent();
			FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
			titlebtn.FontSize = 24 * Screensize.heightfactor;
            backbtn.FontSize = 24 * Screensize.heightfactor;
            //sync data with cl
			sycnwithCloud();
		
		}

		void btnPrevClicked(object sender, System.EventArgs e)
		{
			Navigation.PopAsync();
		}

        public void setLandscape()
        { 
            Debug.WriteLine("Called ParamPageLocal setLandscape()");
        }
        public void setPortrait()
        {
            Debug.WriteLine("Called ParamPageLocal setPortrait()");
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
