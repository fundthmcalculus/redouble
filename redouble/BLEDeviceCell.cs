using System;
using Xamarin.Forms;

namespace redouble
{
	public class BLEDeviceCell : ViewCell
	{
		Label bleNameLabel;
		Label bleIDLabel;

		public BLEDeviceCell ()
		{
			this.bleNameLabel = new Label () { Text = "Fake BLE Device", FontSize = Device.GetNamedSize (NamedSize.Medium, 
					typeof(Label)), FontAttributes = FontAttributes.Bold
			};
			this.bleNameLabel.SetBinding (Label.TextProperty, new Binding ("Name"));

			this.bleIDLabel = new Label () {Text ="BLE GUID", FontSize = Device.GetNamedSize (NamedSize.Small,
					typeof(Label)), FontAttributes = FontAttributes.None
			};
			this.bleIDLabel.SetBinding (Label.TextProperty, new Binding ("ID",BindingMode.Default, new GuidStringConvertor()));

			View = new StackLayout {
				Orientation = StackOrientation.Vertical,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Padding = new Thickness (15, 5, 5, 15),
				Children = { bleNameLabel,bleIDLabel }
			};
		}
	}

	public class GuidStringConvertor : IValueConverter
	{
		public object Convert(object value, Type targetType,object parameter, System.Globalization.CultureInfo culture)
		{
			return value.ToString ();
		}

		public object ConvertBack(object value, Type targetType,object parameter, System.Globalization.CultureInfo culture)
		{
			return new Guid((string)value);
		}
	}

}

