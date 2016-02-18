using System;
using System.Collections.Generic;
using System.Threading;

namespace robotserver
{
	class MainClass
	{
		// Command strings.
		public static string EXIT = "EXIT";

		// Lists of synonyms for each of the command strings.
		public static List<string> EXIT_synonyms = new List<string>(new string [] {"QUIT","Q","EXIT", "CLOSE"});

		// The server object which we are controlling.
		private static Server roboServer;

		public static void Main (string[] args)
		{
			roboServer = new Server ();
			roboServer.RunServer ();
			// Have the ability to issue commands at the console.
			ProcessCommands ();
		}

		static void ProcessCommands ()
		{
			while (true) {
				// Get the next command - block.
				Console.Write ("> ");
				string cmd = Console.ReadLine ();
				// Clean it.
				cmd = CleanCommand (cmd);
				// Now process it.
				if (cmd.Equals (EXIT)) {
					// TODO - Support exiting the program.
					break;
				}
			}
		}

		private static string CleanCommand(string cmd)
		{
			// Convert to UPPER case.
			cmd = cmd.ToUpperInvariant();
			// Now remove all synonyms.
			if (EXIT_synonyms.Contains(cmd)) cmd = EXIT;

			// Return the processed command.
			return cmd;
		}
	}
}
