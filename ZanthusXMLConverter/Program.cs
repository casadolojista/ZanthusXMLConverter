using System;
using System.Configuration;

namespace ZanthusXMLConverter {
	class Program {
		static void Main() {
			var appSettings = ConfigurationManager.AppSettings;
			ConsoleKeyInfo pressedKey;

			Console.WriteLine("CONSOLE APPLICATION INICIADA");
			Console.WriteLine("\n########################################");
			Console.WriteLine(" - Pressione [Y] para enviar uma requisição");
			Console.WriteLine(" - Pressione [Esc] para sair");

			// Read current pressed key to call requisitions
			do {
				pressedKey = Console.ReadKey();

				switch (pressedKey.Key) {
					default:
						break;
					case ConsoleKey.Y:
						Console.WriteLine("\n----------------------------------------");
						Console.WriteLine("Enviando requisição...");
						MerchandiseRequests.SearchMerchandises(
							appSettings.Get("RequestEndpoint") + appSettings.Get("RequestMethod"),
							FileWriter.GetRequestFilePath(typeof(Merchandise), "Search")
						);
						break;
					//case ConsoleKey.M:
					//	Console.WriteLine("\n\n----------------------------------------");
					//	Console.WriteLine("Método de requisição está em desenvolvimento");
					//	break;
				}
			} while (pressedKey.Key != ConsoleKey.Escape);
		}

	}
}