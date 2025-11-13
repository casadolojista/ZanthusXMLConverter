using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace ZanthusXMLConverter
{
	class MercadoriaRequests
	{
		// envia um POST request buscando as mercadorias e retorna na forma de um arquivo XML
		public static async void ProcurarMercadorias(string requestEndpoint, string requestFilePath)
		{
			var appSettings = ConfigurationManager.AppSettings;

			List<Mercadoria> mercadorias = new List<Mercadoria>();
			PropertyInfo[] atributos = {
				typeof(Mercadoria).GetProperty("CodLoja"),
				typeof(Mercadoria).GetProperty("CodMercadoria"),
				typeof(Mercadoria).GetProperty("Descricao"),
				typeof(Mercadoria).GetProperty("PrecoUnitario"),
				typeof(Mercadoria).GetProperty("CodSitTrib"),
				typeof(Mercadoria).GetProperty("AliqInternaICMS"),
				typeof(Mercadoria).GetProperty("CodSitTribPIS"),
				typeof(Mercadoria).GetProperty("AliqPIS"),
				typeof(Mercadoria).GetProperty("CodSitTribCOFINS"),
				typeof(Mercadoria).GetProperty("AliqCOFINS"),
				typeof(Mercadoria).GetProperty("CFOP"),
				typeof(Mercadoria).GetProperty("CEST"),
				typeof(Mercadoria).GetProperty("NCM"),
				typeof(Mercadoria).GetProperty("CSTOrigemInteg"),
				typeof(Mercadoria).GetProperty("CBNEF"),
				typeof(Mercadoria).GetProperty("FlagInativo")
			};
			List<PropertyInfo> mercadoriaAtributos = new List<PropertyInfo>(atributos);

			using (var requestClient = new HttpClient())
			{

				
				// Arquivo XML
				XDocument requestXDoc = XDocument.Load(requestFilePath);
				StringContent requestContent = new StringContent(requestXDoc.ToString(), null, "text/xml");

				// Send an asynchronous request and await for response (only return a valid response if was successful)
				try
				{
					HttpResponseMessage responseResult = await requestClient.PostAsync(requestEndpoint, requestContent);
					responseResult.EnsureSuccessStatusCode();

					// Read and decode response as string, then set it in a XDocument
					string responseString = await responseResult.Content.ReadAsStringAsync();
					string responseDecoded = Regex.Replace(responseString, @"\t|\n|\r", "");
					XDocument responseXDoc = XDocument.Parse(@responseDecoded);

					// Get response's body content and set it in another new XDocument
					string responseBody = responseXDoc.Descendants("return").First().Value;
					XDocument responseXDocBody = XDocument.Parse(responseBody);

					// Parse new XDocument's body content as objects
					foreach (XElement query in responseXDocBody.Element("ZMI").Descendants("QUERY"))
					{
						foreach (XElement entry in query.Descendants("CONTENT"))
						{
							if (entry.HasElements)
							{
								int storeID = int.Parse(entry.Element(appSettings.Get("StoreID" + "Tag")).Value);
								string mercID = entry.Element(appSettings.Get("MercID" + "Tag")).Value.Trim('0');
								string description = Regex.Replace(entry.Element(appSettings.Get("Description" + "Tag")).Value, " +", " ");
								int pisTaxStatus = int.Parse(entry.Element(appSettings.Get("PISTaxStatus" + "Tag")).Value);
								double pisAliquot = double.Parse(entry.Element(appSettings.Get("PISAliquot" + "Tag")).Value);
								float pisSubstPerc = float.Parse(entry.Element(appSettings.Get("PISSubstPerc" + "Tag")).Value);
								float pisReducPerc = float.Parse(entry.Element(appSettings.Get("PISReducPerc" + "Tag")).Value);
								int cofinsTaxStatus = int.Parse(entry.Element(appSettings.Get("COFINSTaxStatus" + "Tag")).Value);
								double cofinsAliquot = double.Parse(entry.Element(appSettings.Get("COFINSAliquot" + "Tag")).Value);
								float cofinsSubstPerc = float.Parse(entry.Element(appSettings.Get("COFINSSubstPerc" + "Tag")).Value);
								float cofinsReducPerc = float.Parse(entry.Element(appSettings.Get("COFINSReducPerc" + "Tag")).Value);
								double accountingCost = double.Parse(entry.Element(appSettings.Get("AccountingCost" + "Tag")).Value);
								int inactiveFlag = int.Parse(entry.Element(appSettings.Get("InactiveFlag" + "Tag")).Value);

								if (!mercadorias.Exists(x => x.CodLoja == storeID) ||
									!mercadorias.Exists(x => x.CodMercadoria == mercID))
								{
									Mercadoria mercadoria = new Mercadoria(storeID, mercID)
									{
										Descricao = description,
										CodSitTribPIS = pisTaxStatus,
										AliqPIS = pisAliquot,
										CodSitTribCOFINS = cofinsTaxStatus,
										AliqCOFINS = cofinsAliquot,
										FlagInativo = inactiveFlag
									};
									mercadorias.Add(mercadoria);
								}
							}
						}
					}

					// Order list's objects and write the content into a XML file
					mercadorias = mercadorias.OrderBy(x => x.CodLoja).ThenBy(x => x.CodMercadoria.Length).ThenBy(x => x.CodMercadoria).ToList();
					FileWriter.WriteXMLFromList(mercadorias, mercadoriaAtributos, "Search");

					Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
					Console.WriteLine("Retorno da requisição concluída");
					Console.WriteLine("\nRESULTADOS");
					Console.WriteLine("Qtd. de Lojas: " + mercadorias.GroupBy(x => x.CodLoja).Count());
					Console.WriteLine("Qtd. de Mercadorias: " + mercadorias.GroupBy(x => x.CodMercadoria).Count());
				}
				catch (Exception ex)
				{
					Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
					Console.WriteLine("Falha na requisição");
					Console.WriteLine("\nERRO");
					Console.WriteLine(ex.ToString());
				}

				Console.WriteLine("\n########################################");
				Console.WriteLine(" - Pressione [M] para enviar uma nova requisição");
				Console.WriteLine(" - Pressione [Esc] para sair");
			}
		}
	}
}
