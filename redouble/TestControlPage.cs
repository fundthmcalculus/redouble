using System;
using System.Threading.Tasks;
using System.Threading;

using Xamarin.Forms;
using BLE = Robotics.Mobile.Core.Bluetooth.LE;

namespace redouble
{
	public class TestControlPage : ContentPage
	{
		private double m_leftRightSpeed = 0;

		public double LeftRightSpeed {
			get {
				return this.m_leftRightSpeed;
			}
			set {
				double d = value;
				if (Math.Abs (d) < zero_band)
					d = 0;
				this.m_leftRightSpeed = d;
			}
		}

		private double m_upDownSpeed = 0;

		public double UpDownSpeed {
			get {
				return this.m_upDownSpeed;
			}
			set {
				// Clamp.
				double d = value;
				if (Math.Abs (d) < zero_band)
					d = 0;
				this.m_upDownSpeed = d;
			}
		}

		// Clamping region about zero.
		private double zero_band = 5;


		public Slider m_leftRightSlider = null;
		public Slider m_upDownSlider = null;

		private BluetoothSearchPage m_searchPage = null;

		public BluetoothSearchPage SearchPage {
			get {
				return this.m_searchPage;
			}
			set {
				this.m_searchPage = value;
			}
		}


		private Task m_communicationTask = null;
		private CancellationTokenSource m_cancellationSource = new CancellationTokenSource ();

		public TestControlPage (BluetoothSearchPage bSearchPage = null)
		{
			this.Title = "Control";
			// Create the slider objects.
			this.m_leftRightSlider = new Slider (-100, 100, this.m_leftRightSpeed);
			this.m_upDownSlider = new Slider (-100, 100, this.m_upDownSpeed);

			// Store the search page with all the appropriate information.
			this.SearchPage = bSearchPage;

			// Add event handlers.
			m_leftRightSlider.ValueChanged += LeftRightSlider_ValueChanged;
			m_upDownSlider.ValueChanged += M_upDownSlider_ValueChanged;

			Content = new StackLayout { 
				Children = {
					new Label { Text = "Left/Right Speed" },
					m_leftRightSlider,
					new Label { Text = "Up/Down Speed" },
					m_upDownSlider

				},
				Padding = Constants.GetPadding ()
			};
			// Start the background thread that keeps sending stuff.
			this.m_communicationTask = new Task (CommunicateWithArduino);
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			this.m_communicationTask.Start ();
		}

		void M_upDownSlider_ValueChanged (object sender, ValueChangedEventArgs e)
		{
			// Update the value being sent.
			this.UpDownSpeed = m_upDownSlider.Value;
		}

		void LeftRightSlider_ValueChanged (object sender, ValueChangedEventArgs e)
		{
			// Update the value being sent.
			this.LeftRightSpeed = m_leftRightSlider.Value;
		}

		public void CommunicateWithArduino ()
		{
			// Loop forever.
			while (true) {
				// Pause for 1/10 of a second.
				Thread.Sleep (100);
				// Send a packet with the data
				this.SearchPage.WriteSpeed ((sbyte)this.LeftRightSpeed, (sbyte)this.UpDownSpeed);
			}
		}

		protected override void OnDisappearing ()
		{
			// Stop the communication task.
			this.m_cancellationSource.Cancel ();
			base.OnDisappearing ();
		}
	}
}


