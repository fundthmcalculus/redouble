using System;
using System.Linq;

using Xamarin.Forms;


namespace redouble
{
	public class RedoubleApp : Application
	{
		public TabbedPageContainer m_myTabbedContainer = new TabbedPageContainer();
		public RedoubleApp ()
		{
			MainPage = m_myTabbedContainer;
		}



		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

