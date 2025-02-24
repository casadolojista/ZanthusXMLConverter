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
	class MerchandiseRequests {
		// Send a POST request to search for merchandises
		public static async void SearchMerchandises(string requestEndpoint, string requestXMLContentPath) {
			var appSettings = ConfigurationManager.AppSettings;

			// List with objects (and selected attributes) to be used as content in response's XML file
			List<Merchandise> merchandises = new List<Merchandise>();
			PropertyInfo[] merchandiseAttributes = {
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
			List<PropertyInfo> searchedAttributes = new List<PropertyInfo>(merchandiseAttributes);

			// Set a HTTP request
			using (var requestClient = new HttpClient()) {
				// Load the XML file from the current path and set it in a XDocument to be used as request's content
				XDocument requestXDoc = XDocument.Load(requestXMLContentPath);
				StringContent requestContent = new StringContent(requestXDoc.ToString(), null, "text/xml");

				// Send an asynchronous request and await for response (only return a valid response if was successful)
				try {
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
					foreach (XElement query in responseXDocBody.Element(appSettings.Get("responseBodyTag")).Descendants("QUERY")) {
						foreach (XElement entry in query.Descendants("CONTENT")) {
							if (entry.HasElements) {
								int storeID = int.Parse(entry.Element(appSettings.Get("storeID")).Value);
								string mercID = entry.Element(appSettings.Get("mercID")).Value.Trim('0');
								string description = Regex.Replace(entry.Element(appSettings.Get("description")).Value, " +", " ");
								int pisTaxStatus = int.Parse(entry.Element(appSettings.Get("pisTaxStatus")).Value);
								double pisAliquot = double.Parse(entry.Element(appSettings.Get("pisAliquot")).Value, CultureInfo.InvariantCulture);
								float pisSubstPerc = float.Parse(entry.Element(appSettings.Get("pisSubstPerc")).Value, CultureInfo.InvariantCulture);
								float pisReducPerc = float.Parse(entry.Element(appSettings.Get("pisReducPerc")).Value, CultureInfo.InvariantCulture);
								int cofinsTaxStatus = int.Parse(entry.Element(appSettings.Get("cofinsTaxStatus")).Value);
								double cofinsAliquot = double.Parse(entry.Element(appSettings.Get("cofinsAliquot")).Value, CultureInfo.InvariantCulture);
								float cofinsSubstPerc = float.Parse(entry.Element(appSettings.Get("cofinsSubstPerc")).Value, CultureInfo.InvariantCulture);
								float cofinsReducPerc = float.Parse(entry.Element(appSettings.Get("cofinsReducPerc")).Value, CultureInfo.InvariantCulture);
								double accountingCost = double.Parse(entry.Element(appSettings.Get("accountingCost")).Value, CultureInfo.InvariantCulture);
								int inactiveFlag = int.Parse(entry.Element(appSettings.Get("inactiveFlag")).Value);

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

					// Order list's objects
					merchandises = merchandises.OrderBy(x => x.StoreID).ThenBy(x => x.MercID.Length).ThenBy(x => x.MercID).ToList();

					// Write a XML file with the ordered list
					string responseFileBodyTag = appSettings.Get("MerchandiseFileBodyTag");
					string responseFileName = appSettings.Get("responseXMLFileName");
					string responseFilePath = appSettings.Get("responseXMLFilePath");
					FileWriter.WriteXMLFromList(merchandises, searchedAttributes, responseFileBodyTag, responseFileName, responseFilePath);

					Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
					Console.WriteLine("Retorno da requisição concluída");
					Console.WriteLine("\nRESULTADOS");
					Console.WriteLine("Qtd. de Lojas: " + merchandises.GroupBy(x => x.StoreID).Count());
					Console.WriteLine("Qtd. de Mercadorias: " + merchandises.GroupBy(x => x.MercID).Count());
				} catch (Exception ex) {
					Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
					Console.WriteLine("Falha na requisição");
					Console.WriteLine("\nERRO");
					Console.WriteLine(ex.ToString());
				}

				Console.WriteLine("\n########################################");
				Console.WriteLine(" - Pressione [Y] para enviar uma nova requisição");
				Console.WriteLine(" - Pressione [Esc] para sair");
			}
		}
	}
}
