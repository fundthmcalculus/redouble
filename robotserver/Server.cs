using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

namespace robotserver
{
	public class Server
	{
		// Timeout for client connections.
		public const int CLIENT_TIMEOUT = 5000; // ms

		// Standard packet size
		public const int PACKET_SIZE = 16;

		// Keep-alive packet object - 16 bytes long.
		private static byte[] keepAlivePacket = null;
		public byte[] KEEPALIVE_PACKET {
			get {
				if (keepAlivePacket == null) {
					keepAlivePacket = CreateKeepAlivePacket ();
				}
				return keepAlivePacket;
			}
		}

		// Basic TCP listener thread.
		private static Thread tcpListenThread;

		// Collection of threads each handling a given client.
		private Dictionary<TcpClient,Thread> clientsAndThreads = new Dictionary<TcpClient, Thread>();

		// Actual TCP listener.
		TcpListener listener = null;

		public Server ()
		{
			// TODO - Handle any setup we need to do.
		}

		public void RunServer()
		{
			// Start tcp listener thread.
			tcpListenThread = new Thread(this.ListenForConnections);
			tcpListenThread.Start ();
		}

		public void ListenForConnections()
		{
			// Start listening for incoming TCP connections.
			listener = new TcpListener(IPAddress.Any,8080);
			listener.Start ();
			while (true) {
				Console.WriteLine ("{0}: Waiting for client to connect...",Thread.CurrentThread.ManagedThreadId);
				// Application blocks while waiting for client.
				TcpClient client = listener.AcceptTcpClient();
				// Add this client to the collection of active ones.
				AddClient(client);
			}
		}

		public void AddClient(TcpClient client) {
			// Create a new thread for this client.
			Thread clientThread = new Thread(this.HandleClient);
			clientThread.Start (client);
			// Store the pair of information.
			this.clientsAndThreads.Add(client,clientThread);
		}

		public void HandleClient(object param) {
			// Grab the current client.
			TcpClient myClient = (TcpClient)param;
			myClient.NoDelay = true;
			// Create a stream.
			NetworkStream clientStream = myClient.GetStream();

			// Set the timeouts.
			clientStream.ReadTimeout = CLIENT_TIMEOUT;
			clientStream.WriteTimeout = CLIENT_TIMEOUT;

			// Loop forever.
			while (true) {
				// Check for available data.
				if (clientStream.DataAvailable) {
					// Read the client buffer.
					byte[] buffer = new byte[PACKET_SIZE];
					int bytes = clientStream.Read(buffer,0,buffer.Length);
					// TODO - Process the packet.
				}
				// Send a keep-alive packet.
				clientStream.Write(KEEPALIVE_PACKET,0,KEEPALIVE_PACKET.Length);
			}
		}

		private byte[] CreateKeepAlivePacket() {
			// Create a new byte array.
			byte[] keepAlive = new byte[PACKET_SIZE];
			// Set each value to triangle wave values - 0,255,0,255,0,255,...
			for (int ij = 0; ij < keepAlive.Length; ij++) {
				keepAlive [ij] = byte.MaxValue * (ij % 2);
			}
			return keepAlive;
		}
	}
}

