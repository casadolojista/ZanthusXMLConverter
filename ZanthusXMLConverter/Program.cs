using System;
using System.Configuration;

namespace ZanthusXMLConverter {
	class Program {
		static void Main() {
			var appSettings = ConfigurationManager.AppSettings;
			ConsoleKeyInfo pressedKey;

			Console.WriteLine(" - Pressione [Y] para enviar uma requisição");
			Console.WriteLine(" - Pressione [Esc] para sair");

			do {
				pressedKey = Console.ReadKey();

				switch (pressedKey.Key) {
					default:
						break;
					case ConsoleKey.Y:
						Console.WriteLine("\n\n----------------------------------------");
						Console.WriteLine("Enviando requisição...\n\n");
						RequestMerchandise.SearchMerchandises(
							(appSettings.Get("requestEndpoint") + appSettings.Get("requestMethod")),
							(appSettings.Get("requestXMLFilePath") + appSettings.Get("requestXMLFileName"))
						);
						break;
				}
			} while (pressedKey.Key != ConsoleKey.Escape);
		}
	}
}