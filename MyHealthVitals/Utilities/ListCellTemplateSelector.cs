using System;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public class ListCellTemplateSelector: Xamarin.Forms.DataTemplateSelector
	{
		public ListCellTemplateSelector()
		{
			this.listCellOneItem = new DataTemplate(typeof(ListCellOneItem));
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			return this.listCellOneItem;
			//return messageVm.IsIncoming ? this.incomingDataTemplate : this.outgoingDataTemplate;
		}

		private readonly DataTemplate listCellOneItem;
	}
}
