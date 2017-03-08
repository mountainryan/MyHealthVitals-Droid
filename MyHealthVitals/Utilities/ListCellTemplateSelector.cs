using System;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public class ListCellTemplateSelector: Xamarin.Forms.DataTemplateSelector
	{
		private readonly DataTemplate listCellOneItem;
		private readonly DataTemplate listCellTwoItem;
		public ListCellTemplateSelector()
		{
			this.listCellOneItem = new DataTemplate(typeof(ListCellOneItem));
			this.listCellTwoItem = new DataTemplate(typeof(ListCellTwoItem));
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (((ParameterDetailItem)item).categoryId == 1 || ((ParameterDetailItem)item).categoryId == 2 || ((ParameterDetailItem)item).categoryId == 5)
			{
				return this.listCellTwoItem;
			}
			else {
				return this.listCellOneItem;
			}
		}
	}
}
