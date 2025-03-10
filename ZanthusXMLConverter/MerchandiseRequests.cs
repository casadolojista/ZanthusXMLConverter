﻿using System;
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
		// Send a POST request to search for merchandises and return response as a XML file
		public static async void SearchMerchandises(string requestEndpoint, string requestFilePath) {
			var appSettings = ConfigurationManager.AppSettings;

			// List with objects (and selected attributes) to be used as content in response's file
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
			List<PropertyInfo> requestedAttributes = new List<PropertyInfo>(merchandiseAttributes);

			// Set a HTTP request
			using (var requestClient = new HttpClient()) {
				// Load the XML file from the current path and set it into a XDocument to be used as request's content
				XDocument requestXDoc = XDocument.Load(requestFilePath);
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
					foreach (XElement query in responseXDocBody.Element("ZMI").Descendants("QUERY")) {
						foreach (XElement entry in query.Descendants("CONTENT")) {
							if (entry.HasElements) {
								int storeID = int.Parse(entry.Element(appSettings.Get("StoreID" + "Tag")).Value);
								string mercID = entry.Element(appSettings.Get("MercID" + "Tag")).Value.Trim('0');
								string description = Regex.Replace(entry.Element(appSettings.Get("Description" + "Tag")).Value, " +", " ");
								int pisTaxStatus = int.Parse(entry.Element(appSettings.Get("PISTaxStatus" + "Tag")).Value, CultureInfo.InvariantCulture);
								double pisAliquot = double.Parse(entry.Element(appSettings.Get("PISAliquot" + "Tag")).Value, CultureInfo.InvariantCulture);
								float pisSubstPerc = float.Parse(entry.Element(appSettings.Get("PISSubstPerc" + "Tag")).Value, CultureInfo.InvariantCulture);
								float pisReducPerc = float.Parse(entry.Element(appSettings.Get("PISReducPerc" + "Tag")).Value, CultureInfo.InvariantCulture);
								int cofinsTaxStatus = int.Parse(entry.Element(appSettings.Get("COFINSTaxStatus" + "Tag")).Value, CultureInfo.InvariantCulture);
								double cofinsAliquot = double.Parse(entry.Element(appSettings.Get("COFINSAliquot" + "Tag")).Value);
								float cofinsSubstPerc = float.Parse(entry.Element(appSettings.Get("COFINSSubstPerc" + "Tag")).Value, CultureInfo.InvariantCulture);
								float cofinsReducPerc = float.Parse(entry.Element(appSettings.Get("COFINSReducPerc" + "Tag")).Value, CultureInfo.InvariantCulture);
								double accountingCost = double.Parse(entry.Element(appSettings.Get("AccountingCost" + "Tag")).Value, CultureInfo.InvariantCulture);
								int inactiveFlag = int.Parse(entry.Element(appSettings.Get("InactiveFlag" + "Tag")).Value);

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

					// Order list's objects and write the content into a XML file
					merchandises = merchandises.OrderBy(x => x.StoreID).ThenBy(x => x.MercID.Length).ThenBy(x => x.MercID).ToList();
					FileWriter.WriteXMLFromList(merchandises, requestedAttributes, "Search");

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
