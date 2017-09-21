﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			InitializeComponent();
			sycnwithCloud();
		
		}

		public async void sycnwithCloud()
		{
			layoutLoading.IsVisible = true;
			if (allReadings == null || logcalParameteritem.localhashmap.Count() > 100)
			{
				logcalParameteritem.localhashmap.Clear();
				allReadings = await Reading.GetAllReadingsFromService();
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
		
				allReadings =  await Reading.GetAllReadingsFromService();
			//	Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(5000), () =>
			//	{

					layoutLoading.IsVisible = false;

					var newPage = new ParameterItemDetailNew(((CategoryLocal)e.Item).id, allReadings);

					this.Navigation.PushAsync(newPage);
			///	   return false;
			//	});
		}
			else
			{

				var newPage = new ParameterItemDetailNew(((CategoryLocal)e.Item).id, allReadings);

				this.Navigation.PushAsync(newPage);
			}
		}
	}
}