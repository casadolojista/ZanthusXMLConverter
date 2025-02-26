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
						Console.WriteLine("Enviando requisição...");
						MerchandiseRequests.SearchMerchandises(appSettings.Get("RequestEndpoint") + appSettings.Get("RequestMethod"), GetRequestFilePath(typeof(Merchandise).Name));
						break;
				}
			} while (pressedKey.Key != ConsoleKey.Escape);
		}

		// Get the request file's path of current object class
		public static string GetRequestFilePath(string className) {
			var appSettings = ConfigurationManager.AppSettings;

			string requestFilePath = appSettings.Get("RequestFilePath");
			string requestFileFolder = appSettings.Get(className + "FileFolder");
			string requestFileName = appSettings.Get(className + "RequestFileName");

			return requestFilePath + requestFileFolder + requestFileName;
		}

		// Get path to write the a response file
		public static string GetResponseFilePath(string className) {
			var appSettings = ConfigurationManager.AppSettings;

			string requestFilePath = appSettings.Get("ResponseFilePath");
			string requestFileFolder = appSettings.Get(className + "FileFolder");
			string requestFileName = appSettings.Get(className + "ResponseFileName");

			return requestFilePath + requestFileFolder + requestFileName;
		}
	}
}