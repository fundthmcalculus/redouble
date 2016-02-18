using System;
using Xamarin.Forms;

namespace redouble
{
	public static class Constants
	{
		public static Thickness iOSPadding = new Thickness(0,20,0,0);

		public static Thickness GetPadding()
		{
			// Support other platforms.
			return iOSPadding;
		}
	}
}

