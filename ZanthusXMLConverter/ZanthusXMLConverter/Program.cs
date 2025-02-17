using System;

namespace IntegracaoRequester {
    class Program {
        static readonly string requestEndpoint = "http://mamaepresentes.zanthusonline.com.br/manager/manager_integracao.php5?wsdl=";
        static readonly string requestContentPath = @"C:\Users\Daniel\source\repos\XMLConverter\XMLConverter\Requests\merc_search_request_redux.xml";

        static void Main() {
            Console.WriteLine(" - Pressione [Y] para enviar uma requisição");
            Console.WriteLine(" - Pressione [Esc] para sair");

            ConsoleKeyInfo pressedKey;

            do {
                pressedKey = Console.ReadKey();

                switch (pressedKey.Key) {
                    default:
                        break;
                    case ConsoleKey.Y:
                        Console.WriteLine("\n\n----------------------------------------");
                        Console.WriteLine("Enviando requisição...\n\n");
                        RequestMerchandise.SearchForMerchandises(requestEndpoint, requestContentPath);
                        break;
                }
            } while (pressedKey.Key != ConsoleKey.Escape);
        }
    }
}