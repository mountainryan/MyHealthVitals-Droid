using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MyHealthVitals;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class ParameterItemDetailNew : ContentPage
	{
		Reading[] allReadings;
		int categoryId;
		public ObservableCollection<ParameterDetailItem> data = new ObservableCollection<ParameterDetailItem>();
		public ParameterItemDetailNew()
		{
            NavigationPage.SetHasNavigationBar(this, false);
			InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet) 
			{
                
				label.FontSize = 45 * Screensize.heightfactor;
				label.Margin = new Thickness(20*Screensize.widthfactor);
                //titlebtn.Text = Xamarin.Forms.Page.
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
                
				label.FontSize *= Screensize.heightfactor;
				label.Margin = new Thickness(10 * Screensize.widthfactor);
            }
		}

		void btnPrevClicked(object sender, System.EventArgs e)
		{
			Navigation.PopAsync();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			//Debug.WriteLine("ParameterItemDetailNew OnAppearing");
			// callApi();
			ListCellTwoItem.fileName = null;
			itemList.ItemsSource = null;

			foreach (var e in data)
			{
				if (e.date.Equals(ListCellTwoItem.itemDate))
				{
					DateTime iDate = Convert.ToDateTime(e.date);
					String date_nosec = iDate.ToString("MM/dd/yyyy hh:mm tt");
					//Debug.WriteLine(e.date);
					string fn = Regex.Replace(date_nosec, @"\s+", "");//dateTime.Trim(' ')
					fn = Regex.Replace(fn, @"[/:]+", "");
					bool ret = DependencyService.Get<IFileHelper>().checkFileExist(fn + ".txt");
					if (ret)
					{
						e.secondItem = "No Report";
					}
					else if (DependencyService.Get<IFileHelper>().checkFileExist(fn + "ECG.pdf"))
					{
						//count++;
						e.secondItem = "Saved";
					}
					else
					{
                        //Debug.WriteLine("filename:" + fn + " e");
						e.secondItem = "Emailed";
					}
					//Debug.WriteLine("e.secondItem ="+ e.secondItem);
					break;
				}
			}
			itemList.ItemsSource = data;
            Debug.WriteLine("font size of Most Recent Readings = "+label.FontSize);
		}

		public ParameterItemDetailNew(int id, Reading[] allReadings)
		{
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
			FakeToolbar.Children.Add(
			backarrow,
			// Adds the Button on the top left corner, with 10% of the navbar's width and 100% height
			new Rectangle(0, 0.5, 0.1, 1),
			// The proportional flags tell the layout to scale the value using [0, 1] -> [0%, 100%]
			AbsoluteLayoutFlags.HeightProportional | AbsoluteLayoutFlags.WidthProportional
			);

			FakeToolbar.Children.Add(
				backbtn,
				// Using 0.5 will center it and the layout takes the size of the element into account
				// 0.5 will center, 1 will right align
				// Adds in the center, with 90% of the navbar's width and 100% of height
				new Rectangle(0.1, 0.5, 0.15, 1),
				AbsoluteLayoutFlags.All
			);
			FakeToolbar.Children.Add(
				titlebtn,
				// Using 0.5 will center it and the layout takes the size of the element into account
				// 0.5 will center, 1 will right align
				// Adds in the center, with 90% of the navbar's width and 100% of height
				new Rectangle(0.5, 0.5, 0.5, 1),
				AbsoluteLayoutFlags.All
			);
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
				titlebtn.FontSize = 30 * Screensize.heightfactor;
                backbtn.FontSize = 30 * Screensize.heightfactor;

				label.FontSize = 45 * Screensize.heightfactor;
				label.Margin = new Thickness(20 * Screensize.widthfactor);
				//titlebtn.Text = Xamarin.Forms.Page.
			}
			else if (Device.Idiom == TargetIdiom.Phone)
			{
				FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
				titlebtn.FontSize = 24 * Screensize.heightfactor;
                backbtn.FontSize = 24 * Screensize.heightfactor;

				label.FontSize *= Screensize.heightfactor;
				label.Margin = new Thickness(10 * Screensize.widthfactor);
			}
           	this.categoryId = id;
			this.allReadings = allReadings;
			setTitleAndData(id);
            titlebtn.Text = this.Title;
			callApi();
		}

		private void setTitleAndData(int categoryId){ 
			switch (categoryId)
			{
				case 1:
				{
					this.Title = "Blood Pressure Data List";
					headerContainer.Children.Add(new headerWithTwoTitle("SYS", "DIA"));
					break;
				}
				case 2:
				{
					this.Title = "SpO2 Data List";
					headerContainer.Children.Add(new headerWithTwoTitle("SpO2", "Pulse"));
					break;
				}
				case 3:
				{
					this.Title = "Heart Rate Data List";
					headerContainer.Children.Add(new headerWithOneTitle("Heart Rate (Pulse)"));
					break;
				}

				case 4:
				{
					this.Title = "Temperature Data List";
					headerContainer.Children.Add(new headerWithOneTitle("Temperature °F/°C"));
					break;
				}
				case 5:
				{
					this.Title = "Weight/BMI Data List";
					headerContainer.Children.Add(new headerWithTwoTitle("Weight lbs/kg", "BMI"));
					break;
				}
				case 8:
				{
					this.Title = "Glucose Data List";
					headerContainer.Children.Add(new headerWithOneTitle("Glucose (MG/DL)"));
					break;
				}
				case 10:
				{
					this.Title = "ECG Data List";
					headerContainer.Children.Add(new headerWithTwoTitle("ECG Result", "Report Status"));
					break;
				}
			}
		}

		public async void callApi()
		{
			try
			{
				//Debug.WriteLine("LIST");

				var allCategoryReading = from reading in allReadings
										 where reading.CategoryId == categoryId
										 select reading;

				//allCategoryReading = allCategoryReading.GroupBy(s => s.Date);
				//Debug.WriteLine("categoryID = " + categoryId);

				switch (categoryId)
				{
					// Blood pressure
					case 1:
						//	case 10:
						{
							if (logcalParameteritem.localhashmap.Count() > 0 && logcalParameteritem.localhashmap.ContainsKey(1) )
							{
								foreach (var val in logcalParameteritem.localhashmap[1])
								{
									data.Add(val);
								};
							}
							//		categoryId = 1
							//Debug.WriteLine("BP START");
							var bpReadings = from spSet in
							   (from reading in allCategoryReading
								group reading by reading.Date)
											 orderby spSet.Key descending
											 let dia = spSet.FirstOrDefault(x => (x.ValueType == "DIA" || x.ValueType == "Diastolic"))
											 let sys = spSet.FirstOrDefault(x => (x.ValueType == "SYS" || x.ValueType == "Systolic"))

											 where sys != null && dia != null
											 select new
											 {
												 Date = spSet.Key,
												 sys = sys,
												 dia = dia,
											 };

							var newBpReadings = (bpReadings.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();

							foreach (var reading in newBpReadings)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = ((int)reading.sys.EnglishValue).ToString();
								item.secondItem = ((int)reading.dia.EnglishValue).ToString();
								item.categoryId = reading.sys.CategoryId;

								data.Add(item);
							}

							itemList.ItemsSource = data;
							Debug.WriteLine("END data= " + data.Count());

							break;
						}
					case 2:
						{
							if (logcalParameteritem.localhashmap.Count() > 0 && logcalParameteritem.localhashmap.ContainsKey(2))
							{
								foreach (var val in logcalParameteritem.localhashmap[2])
								{
									data.Add(val);
								};
							}
							var allCategoryReading2 = from reading in allReadings
													  where reading.CategoryId == 2 || reading.CategoryId == 3
													  select reading;

							var Spo2Readings = from spSet in
							   (from reading in allCategoryReading2
								group reading by reading.Date)
											   orderby spSet.Key descending
											   let SpO2 = spSet.FirstOrDefault(x => x.CategoryId == 2)
											   let Pulse = spSet.FirstOrDefault(x => (x.CategoryId == 3))
											   where SpO2 != null && Pulse != null
											   select new
											   {
												   Date = spSet.Key,
												   SpO2 = SpO2,
												   Pulse = Pulse,
											   };

							//Spo2Readings.GroupBy
							var newSpo2Readings = (Spo2Readings.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();

							foreach (var reading in newSpo2Readings)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = ((int)reading.SpO2.EnglishValue).ToString();
								item.secondItem = ((int)reading.Pulse.EnglishValue).ToString();
								item.categoryId = reading.SpO2.CategoryId;
								data.Add(item);
							}
							itemList.ItemsSource = data;
							break;
						}
					case 3:
						{
							//var sortedHeartRates = allCategoryReading.Reverse();
							if (logcalParameteritem.localhashmap.Count() > 0 && logcalParameteritem.localhashmap.ContainsKey(3))
							{
								foreach (var val in logcalParameteritem.localhashmap[3])
								{
									data.Add(val);
								};
							}
							foreach (var reading in allCategoryReading)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = ((int)reading.EnglishValue).ToString();
								item.categoryId = reading.CategoryId;
								data.Add(item);
							}

							itemList.ItemsSource = data;
							break;
						}
					// temperature
					case 4:
						{
							//var sortedTemps = allCategoryReading.Reverse();
							if (logcalParameteritem.localhashmap.Count() > 0 && logcalParameteritem.localhashmap.ContainsKey(4))
							{
								foreach (var val in logcalParameteritem.localhashmap[4])
								{
									data.Add(val);
								};
							}
							foreach (var reading in allCategoryReading)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = Math.Round((decimal)reading.EnglishValue, 1) + "/" + Math.Round((decimal)reading.MetricValue, 1);
								item.categoryId = reading.CategoryId;
								data.Add(item);
							}

							itemList.ItemsSource = data;
							break;
						}
					case 5:
						{
							if (logcalParameteritem.localhashmap.Count() > 0 && logcalParameteritem.localhashmap.ContainsKey(5))
							{
								foreach (var val in logcalParameteritem.localhashmap[5])
								{
									Debug.WriteLine("val.BMI"+val.secondItem +" WEIGHT "+ val.firstItem);
									data.Add(val);
								};
							}
							var allCategoryReading5 = from reading in allReadings
													  where reading.CategoryId == 5 || reading.CategoryId == 7
													  select reading;

							var weightBmiReading = (from spSet in
							   (from reading in allCategoryReading5
								group reading by reading.Date)
													orderby spSet.Key descending
													let weight = spSet.FirstOrDefault(x => x.CategoryId == 5)
													let bmi = spSet.FirstOrDefault(x => x.CategoryId == 7)
													where weight != null
													select new
													{
														Date = spSet.Key,
														weight = weight,
														bmi = bmi,
													});//.Take(1);

							//where weight != null && bmi != null

							var newWeightBmiReading = (weightBmiReading.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();
							//Debug.WriteLine("newWeightBmiReading = " + newWeightBmiReading);
							foreach (var reading in newWeightBmiReading)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = Math.Round((decimal)reading.weight.EnglishValue, 1) + "/" + Math.Round((decimal)reading.weight.MetricValue, 1);
							//	Debug.WriteLine("firstItem = " + item.firstItem + "SECONDItem = " + item.secondItem);
								if (reading.bmi != null)
								{
									item.secondItem = Math.Round((decimal)reading.bmi.EnglishValue, 1).ToString();
								}
								else
								{
									item.secondItem = null;
								}
								item.categoryId = reading.weight.CategoryId;
								data.Add(item);
							}
							itemList.ItemsSource = data;
							break;
						}

					case 8:
						{
							// Glucose data list
							//var sortedTemps = allCategoryReading.Reverse();
							if (logcalParameteritem.localhashmap.Count() > 0 && logcalParameteritem.localhashmap.ContainsKey(8))
							{
								foreach (var val in logcalParameteritem.localhashmap[8])
								{
									data.Add(val);
								};
							}
							foreach (var reading in allCategoryReading)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = Math.Round((decimal)reading.EnglishValue, 1).ToString();
								item.categoryId = reading.CategoryId;
								data.Add(item);
							}
							itemList.ItemsSource = data;
							break;
						}

					case 10:
						int count = 0;
						if (logcalParameteritem.localhashmap.Count() > 0 && logcalParameteritem.localhashmap.ContainsKey(10))
						{
							foreach (var val in logcalParameteritem.localhashmap[10])
							{

								//	var item = new ParameterDetailItem();
								//	item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");

								//if (count < 30)
								//	{   
							    DateTime iDate = Convert.ToDateTime(val.date);
							    String date_nosec = iDate.ToString("MM/dd/yyyy hh:mm tt");
								//Debug.WriteLine("date_nosec = "+date_nosec);	
                                var fileName = Regex.Replace(date_nosec, @"\s+", "");
                                fileName = Regex.Replace(fileName, @"[/:]+", "");
								//Debug.WriteLine("fileName = " + fileName);	
                                bool ret = DependencyService.Get<IFileHelper>().checkFileExist(fileName + ".txt");
									if (ret)
									{
										val.secondItem = "No Report";
									}
									else if (DependencyService.Get<IFileHelper>().checkFileExist(fileName + "ECG.pdf"))
									{
										//count++;
										val.secondItem = "Saved";
									}
									else
									{
                                        Debug.WriteLine("filename:" + fileName+ " val");
										val.secondItem = "Emailed";
									}
									count++;
							//	}

								val.firstItem = val.firstItem == "0" ? "Normal" : "Abnormal";
								data.Add(val);
							}
						};

						foreach (var reading in allCategoryReading)
						{
                            
							var item = new ParameterDetailItem();
                            item.date = reading.Date.ToString("MM/dd/yyyy hh:mm:ss tt");
                            item.date_nosec = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
                            //Debug.WriteLine("item.date_nosec  = " + item.date_nosec);
						//	if (count < 30)
						//	{
							var fileName = Regex.Replace(item.date_nosec, @"\s+", "");
							fileName = Regex.Replace(fileName, @"[/:]+", "");
                        //Debug.WriteLine("fileName = " + fileName);
							bool ret = DependencyService.Get<IFileHelper>().checkFileExist(fileName + ".txt");
                        //Debug.WriteLine("file exists? "+ret);
							if (ret)
							{
								item.secondItem = "No Report";
							}
							else if (DependencyService.Get<IFileHelper>().checkFileExist(fileName + "ECG.pdf"))
							{
								//count++;
								item.secondItem = "Saved";
							}
							else
							{
                                //Debug.WriteLine("filename:" + fileName + " item");
								item.secondItem = "Emailed";
							}
							count++;
						//	}

							item.firstItem = reading.EnglishValue == 0 ? "Normal" : "Abnormal";
							item.categoryId = reading.CategoryId;
							data.Add(item);
						}

					//	itemList.ItemsSource = data;
						break;
					default:
						break;
				}

			//	itemList.ItemsSource = data;

			}
			catch (Exception)
			{
				Debug.WriteLine("error in calling server or parsing");
			}
			finally
			{
				//layoutLoading.IsVisible = false;
			}

			this.itemList.HeightRequest = this.Content.Bounds.Size.Height - 110;
		}
	}
}
