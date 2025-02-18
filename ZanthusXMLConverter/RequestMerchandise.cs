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

namespace ZanthusXMLConverter {
	class RequestMerchandise {
		// Send a POST request to search for merchandises
		public static async void SearchMerchandises(string requestEndpoint, string requestXMLContentPath) {
			var appSettings = ConfigurationManager.AppSettings;
			List<Merchandise> merchandises = new List<Merchandise>();
			PropertyInfo[] merchandiseProperties = {
				typeof(Merchandise).GetProperty("StoreID"),
				typeof(Merchandise).GetProperty("MercID"),
				typeof(Merchandise).GetProperty("Description"),
				typeof(Merchandise).GetProperty("PISTaxStatus"),
				typeof(Merchandise).GetProperty("PISAliquot"),
				typeof(Merchandise).GetProperty("PISSubstPerc"),
				typeof(Merchandise).GetProperty("PISReducPerc"),
				typeof(Merchandise).GetProperty("COFINSTaxStatus"),
				typeof(Merchandise).GetProperty("COFINSAliquot"),
				typeof(Merchandise).GetProperty("COFINSSubstPerc"),
				typeof(Merchandise).GetProperty("COFINSReducPerc"),
				typeof(Merchandise).GetProperty("AccountingCost"),
				typeof(Merchandise).GetProperty("InactiveFlag")
			};
			List<PropertyInfo> searchedProperties = new List<PropertyInfo>(merchandiseProperties);

			using (var requestClient = new HttpClient()) {
				// Load the XML's file from the current path and set it in a XDocument to be used in request content
				XDocument requestXDoc = XDocument.Load(requestXMLContentPath);
				StringContent requestContent = new StringContent(requestXDoc.ToString(), null, "text/xml");

				// Send an asynchronous request and await for response (only return a response if was successful)
				try {
					HttpResponseMessage responseResult = await requestClient.PostAsync(requestEndpoint, requestContent);
					responseResult.EnsureSuccessStatusCode();

					// Read and decode response as string, then set it in a XDocument
					string responseString = await responseResult.Content.ReadAsStringAsync();
					string responseDecoded = Regex.Replace(responseString, @"\t|\n|\r", "");
					XDocument responseXDoc = XDocument.Parse(@responseDecoded);

					// Get response XDocument's body content and set in another XDocument
					string responseBody = responseXDoc.Descendants("return").First().Value;
					XDocument responseXDocBody = XDocument.Parse(responseBody);

					// Parse body content's XDocument as objects
					foreach (XElement query in responseXDocBody.Element("ZMI").Descendants("QUERY")) {
						foreach (XElement entry in query.Descendants("CONTENT")) {
							if (entry.HasElements) {
								int storeID = int.Parse(entry.Element("COD_LOJA").Value);
								string mercID = entry.Element("COD_MERCADORIA").Value.Trim('0');
								string description = Regex.Replace(entry.Element("DESCRICAO").Value, " +", " ");
								int pisTaxStatus = int.Parse(entry.Element("COD_SIT_TRIB_PIS").Value);
								double pisAliquot = double.Parse(entry.Element("ALIQUOTA_PIS").Value, CultureInfo.InvariantCulture);
								float pisSubstPerc = float.Parse(entry.Element("PERC_SUBST_PIS").Value, CultureInfo.InvariantCulture);
								float pisReducPerc = float.Parse(entry.Element("PERC_REDUZ_B_CALCULO_PIS").Value, CultureInfo.InvariantCulture);
								int cofinsTaxStatus = int.Parse(entry.Element("COD_SIT_TRIB_COFINS").Value);
								double cofinsAliquot = double.Parse(entry.Element("ALIQUOTA_COFINS").Value, CultureInfo.InvariantCulture);
								float cofinsSubstPerc = float.Parse(entry.Element("PERC_SUBST_COFINS").Value, CultureInfo.InvariantCulture);
								float cofinsReducPerc = float.Parse(entry.Element("PERC_REDUZ_B_CALCULO_COFINS").Value, CultureInfo.InvariantCulture);
								double accountingCost = double.Parse(entry.Element("CUSTO_CONTABIL").Value, CultureInfo.InvariantCulture);
								int inactiveFlag = int.Parse(entry.Element("FLGINATIVO").Value);

								if (!merchandises.Exists(x => x.StoreID == storeID) ||
									!merchandises.Exists(x => x.MercID == mercID)) {
									Merchandise merchandise = new Merchandise(storeID, mercID) {
										Description = description,
										PISTaxStatus = pisTaxStatus,
										PISAliquot = pisAliquot,
										PISSubstPerc = pisSubstPerc,
										PISReducPerc = pisReducPerc,
										COFINSTaxStatus = cofinsTaxStatus,
										COFINSAliquot = cofinsAliquot,
										COFINSSubstPerc = cofinsSubstPerc,
										COFINSReducPerc = cofinsReducPerc,
										AccountingCost = accountingCost,
										InactiveFlag = inactiveFlag
									};
									merchandises.Add(merchandise);
								}
							}
						}
					}

					// Order objects in the list
					merchandises = merchandises.OrderBy(x => x.StoreID).ThenBy(x => x.MercID.Length).ThenBy(x => x.MercID).ToList();

					// Write a XML file with the ordered list
					string fileBodyName = appSettings.Get("responseBodyName");
					string responseFileName = appSettings.Get("responseXMLFileName");
					string responseFilePath = appSettings.Get("responseXMLFilePath");
					FileWriter.WriteFromList(merchandises, searchedProperties, fileBodyName, responseFileName, responseFilePath);

					Console.WriteLine("\n\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
					Console.WriteLine("Retorno da requisição concluída");
					Console.WriteLine("\nRESULTADOS");
					Console.WriteLine("Qtd. de Lojas: " + merchandises.GroupBy(x => x.StoreID).Count());
					Console.WriteLine("Qtd. de Mercadorias: " + merchandises.GroupBy(x => x.MercID).Count());
				} catch (Exception ex) {
					Console.WriteLine("\n\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
					Console.WriteLine("ERRO NA REQUISIÇÃO!");
					Console.WriteLine("\n" + ex.ToString());
				}

				Console.WriteLine("\n\n########################################");
				Console.WriteLine(" - Pressione [Y] para enviar uma nova requisição");
				Console.WriteLine(" - Pressione [Esc] para sair");
			}
		}
	}
}
