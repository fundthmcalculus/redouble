using System;
using Xamarin.Forms;

namespace redouble
{
	public class TabbedPageContainer : TabbedPage
	{
		public BluetoothSearchPage m_searchPage = new BluetoothSearchPage();
		public TestControlPage m_testPage =null;

		public TabbedPageContainer ()
		{
			this.m_testPage = new TestControlPage (m_searchPage);

			this.Title = "Redouble";

			this.Children.Add(m_searchPage);
			this.Children.Add(m_testPage);
		}
	}
}

