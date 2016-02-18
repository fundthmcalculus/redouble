using System;
using System.Linq;

using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using BLE = Robotics.Mobile.Core.Bluetooth.LE;


namespace redouble
{
	public class BluetoothSearchPage : ContentPage
	{
		public const byte EOS_SENTINEL = 111;

		public const string RED_BEAR_LABS_BISCUIT_SERVICE = "713d0000-503e-4c75-ba94-3148f18d941e";
		public const string RED_BEAR_LABS_BISCUIT_WRITE_WITHOUT_RESPONSE = "713d0003-503e-4c75-ba94-3148f18d941e";
		public const string RED_BEAR_LABS_BISCUIT_NOTIFY = "713d0002-503e-4c75-ba94-3148f18d941e";

		private BLE.Characteristic m_notifyCharacteristic = null;

		public BLE.Characteristic NotifyCharacteristic {
			get {
				return this.m_notifyCharacteristic;
			}
			set {
				this.m_notifyCharacteristic = value;
			}
		}

		private BLE.Characteristic m_writeCharacteristic = null;

		public BLE.Characteristic WriteCharacteristic {
			get {
				return this.m_writeCharacteristic;
			}
			set {
				this.m_writeCharacteristic = value;
			}
		}

		private BLE.Device m_connectedDevice = null;

		/// <summary>
		/// Gets or sets the connected device. It is used during the actual connection phase.
		/// </summary>
		/// <value>The connected device.</value>
		public BLE.Device ConnectedDevice {
			get {
				return this.m_connectedDevice;
			}
			set {
				this.m_connectedDevice = value;
			}
		}

		private BLE.Service m_redBearLabsService = null;

		/// <summary>
		/// Gets or sets the red bear labs Bluetooth service.
		/// </summary>
		/// <value>The red bear labs service.</value>
		public BLE.Service RedBearLabsService {
			get {
				return this.m_redBearLabsService;
			}
			set {
				this.m_redBearLabsService = value;
			}
		}

		Button connectButton = new Button {
			Text = "Connect to Arduino!",
		};

		ListView bleDeviceListView = new ListView ();

		public BluetoothSearchPage ()
		{
			this.Title = "Search";
			// Set up bluetooth callbacks.
			BLE.Adapter.Current.DeviceDiscovered += Adapter_Current_DeviceDiscovered;
			BLE.Adapter.Current.ScanTimeoutElapsed += Robotics_Mobile_Core_Bluetooth_LE_Adapter_Current_ScanTimeoutElapsed;
			BLE.Adapter.Current.DeviceConnected += BLE_Adapter_Current_DeviceConnected;
			BLE.Adapter.Current.DeviceFailedToConnect += BLE_Adapter_Current_DeviceFailedToConnect;

			// Set up GUI callbacks.
			connectButton.Clicked += connectButtonClicked;

			bleDeviceListView.IsPullToRefreshEnabled = true;
			bleDeviceListView.Refreshing += BleDeviceListView_Refreshing;
			bleDeviceListView.ItemTemplate = new DataTemplate (typeof(BLEDeviceCell));
			bleDeviceListView.ItemsSource = this.DiscoveredDevices;
			bleDeviceListView.HasUnevenRows = true;
			bleDeviceListView.ItemSelected += BleDeviceListView_ItemSelected;

			// The root page of your application
			this.Content = new StackLayout {
				VerticalOptions = LayoutOptions.Center,
				Children = {
					connectButton,
					bleDeviceListView
				},
				Padding = Constants.GetPadding ()
			};
		}

		void BLE_Adapter_Current_DeviceFailedToConnect (object sender, BLE.DeviceConnectionEventArgs e)
		{
			// Handle device failed to connect.
			this.DisplayAlert ("Connection Failed", "Device `" + e.Device.Name + "` failed to connect.", "Cancel");
		}

		void BLE_Adapter_Current_DeviceConnected (object sender, BLE.DeviceConnectionEventArgs e)
		{
			// Check what services are available.
			this.ConnectedDevice = (BLE.Device)e.Device;
			this.ConnectedDevice.ServicesDiscovered += ConnectedDevice_ServicesDiscovered;
			this.ConnectedDevice.DiscoverServices ();
		}

		void ConnectedDevice_ServicesDiscovered (object sender, EventArgs e)
		{
			// Find the Red Bear Labs service.
			this.RedBearLabsService = (from cService in this.ConnectedDevice.Services
			                           where cService.ID.ToString ().Equals (
				                               RED_BEAR_LABS_BISCUIT_SERVICE, StringComparison.OrdinalIgnoreCase)
			                           select (BLE.Service)cService).ToList () [0];
			// Make sure it is not null.
			if (this.RedBearLabsService == null) {
				this.DisplayAlert ("Missing Bluetooth Service", "Red Bear Labs Biscuit Service not available!", "Cancel");
				return;
			}
			// Figure out what characteristics are available.
			this.RedBearLabsService.CharacteristicsDiscovered += RblService_CharacteristicsDiscovered;
			this.RedBearLabsService.DiscoverCharacteristics ();
		}

		void RblService_CharacteristicsDiscovered (object sender, EventArgs e)
		{
			// Get the notify characteristic.
			BLE.Characteristic notifyC = (from cCharacteristic in this.RedBearLabsService.Characteristics
			                              where cCharacteristic.ID.ToString ().Equals (RED_BEAR_LABS_BISCUIT_NOTIFY, StringComparison.OrdinalIgnoreCase)
			                              select (BLE.Characteristic)cCharacteristic).ToList () [0];
			if (notifyC == null) {
				this.DisplayAlert ("Missing Characteristic", "Notify Characteristic is not available.", "Cancel");
				return;
			}

			// Get the write characteristic.
			BLE.Characteristic writeC = (from cCharacteristic in this.RedBearLabsService.Characteristics
			                             where cCharacteristic.ID.ToString ().Equals (RED_BEAR_LABS_BISCUIT_WRITE_WITHOUT_RESPONSE, StringComparison.OrdinalIgnoreCase)
			                             select (BLE.Characteristic)cCharacteristic).ToList () [0];
			if (writeC == null) {
				this.DisplayAlert ("Missing Characteristic", "Write Characteristic is not available.", "Cancel");
				return;
			}

			// Store the characteristics.
			this.NotifyCharacteristic = notifyC;
			this.WriteCharacteristic = writeC;

			// Set up the characteristic callbacks.
			this.SetupCharacteristics ();

			this.DisplayAlert ("Connection Success", "Device `" + this.ConnectedDevice.Name +
			"` succesfully connected!", "Cancel");

			RedoubleApp rdApp = ((RedoubleApp)RedoubleApp.Current);
			rdApp.m_myTabbedContainer.CurrentPage = rdApp.m_myTabbedContainer.Children [1];
		}

		void BleDeviceListView_ItemSelected (object sender, SelectedItemChangedEventArgs e)
		{
			// Attempt to connect to this device.
			BLE.Device selectedDevice = (BLE.Device)e.SelectedItem;
			BLE.Adapter.Current.ConnectToDevice (selectedDevice);
		}

		void BleDeviceListView_Refreshing (object sender, EventArgs e)
		{
			// Rescan.
			this.ScanForDevices ();
		}

		void connectButtonClicked (object sender, EventArgs e)
		{
			connectButton.IsEnabled = false;
			// Attempt to scan.
			this.ScanForDevices ();
		}

		protected void SetupCharacteristics ()
		{
			// Set up the notify characteristic.
			this.NotifyCharacteristic.ValueUpdated += NotifyCharacteristic_ValueUpdated;
			this.NotifyCharacteristic.StartUpdates ();
		}

		void WriteCharacteristic_ValueUpdated (object sender, BLE.CharacteristicReadEventArgs e)
		{

		}

		void NotifyCharacteristic_ValueUpdated (object sender, BLE.CharacteristicReadEventArgs e)
		{
			// TODO - Show information.
			this.DisplayAlert ("Incoming Data", this.NotifyCharacteristic.StringValue, "OK");
		}


		private ObservableCollection<Robotics.Mobile.Core.Bluetooth.LE.Device> m_discoveredDevices = null;

		public ObservableCollection<Robotics.Mobile.Core.Bluetooth.LE.Device> DiscoveredDevices {
			get {
				if (m_discoveredDevices == null) {
					m_discoveredDevices = new ObservableCollection<Robotics.Mobile.Core.Bluetooth.LE.Device> ();
				}
				return m_discoveredDevices;
			}
		}

		public void ScanForDevices ()
		{
			// Flag the list view.
			this.bleDeviceListView.IsRefreshing = true;
			// Remove everything explicitly.
			for (int ij = this.DiscoveredDevices.Count - 1; ij >= 0; ij--) {
				this.DiscoveredDevices.RemoveAt (ij);
			}
			// Scan for everything.
			BLE.Adapter.Current.StartScanningForDevices (Guid.Empty);
		}

		void Robotics_Mobile_Core_Bluetooth_LE_Adapter_Current_ScanTimeoutElapsed (object sender, EventArgs e)
		{
			// Re-enable the button.
			connectButton.IsEnabled = true;
			// deflag the list view.
			this.bleDeviceListView.IsRefreshing = false;
		}

		private void Adapter_Current_DeviceDiscovered (object sender, BLE.DeviceDiscoveredEventArgs e)
		{
			BLE.Device newDevice = (BLE.Device)e.Device;
			// Don't add it if it doesn't have a name.
			if (string.IsNullOrEmpty (newDevice.Name))
				return;
			// Make sure it hasn't already been added, based upon the ID.
			foreach (BLE.Device item in this.DiscoveredDevices) {
				if (item.ID.Equals (newDevice.ID))
					return;
			}
			this.DiscoveredDevices.Add (newDevice);
		}

		public void WriteSpeed (sbyte lrSpeed, sbyte udSpeed)
		{
			byte lrS, udS;
			// Preserve the bit information.
			unchecked {
				lrS = (byte)lrSpeed;
				udS = (byte)udSpeed;
			}
			this.WriteCharacteristic.Write (new byte[] { lrS, udS, EOS_SENTINEL });
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			// Now start scanning.
			this.ScanForDevices();
		}
	}
}


