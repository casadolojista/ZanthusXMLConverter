﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace ZanthusXMLConverter {
	class FileWriter {
		#region Write XML file from XDocument
		// Write a XML file from a XDocument content
		public static void WriteXMLFromXDocument(XDocument xDoc, string fileName, string filePath) {
			using (XmlWriter xmlWri = XmlWriter.Create(filePath + fileName)) {
				xmlWri.WriteStartDocument();
				WriteXElement(xmlWri, xDoc.Elements().First());
				xmlWri.WriteEndDocument();
				xmlWri.Flush();
				xmlWri.Close();
			}
		}

		// Write the content of a XElement on a XML file
		private static void WriteXElement(XmlWriter xmlWri, XElement xEle) {
			xmlWri.WriteStartElement(xEle.Name.LocalName);

			if (xEle.HasAttributes) {
				foreach (XAttribute xAtt in xEle.Attributes()) {
					xmlWri.WriteAttributeString(xAtt.Name.LocalName, xAtt.Value);
				}
			}

			if (xEle.HasElements) {
				foreach (XElement descendant in xEle.Descendants()) {
					WriteXElement(xmlWri, descendant);
					xmlWri.WriteEndElement();
				}
			} else {
				xmlWri.WriteString(xEle.Value);
			}
		}
		#endregion

		#region Write XML file from object list
		// Write a XML file from the content of an object list
		public static void WriteXMLFromList<T>(List<T> list, List<PropertyInfo> searchedAttributes) {
			var appSettings = ConfigurationManager.AppSettings;
			string responseFileBodyTag = appSettings.Get(typeof(Merchandise).Name + "FileBodyTag");
			string responseFileItemTag = appSettings.Get(typeof(Merchandise).Name + "FileItemTag");

			using (XmlWriter xmlWri = XmlWriter.Create(Program.GetResponseFilePath(typeof(Merchandise).Name))) {
				xmlWri.WriteStartDocument();
				xmlWri.WriteStartElement(responseFileBodyTag);

				// Data Schema
				xmlWri.WriteAttributeString("xmlns", "xsi", null, @"http://www.w3.org/2001/XMLSchema-instance");
				xmlWri.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, appSettings.Get("MerchandiseFileDataSchema")); ;

				// Write every item of XML body content
				foreach (T item in list) {
					xmlWri.WriteStartElement(responseFileItemTag);

					// Write every searched attribute of current item
					foreach (PropertyInfo attribute in searchedAttributes) {
						string formattedAttribute = CreateXMLItemName(appSettings.Get(attribute.Name));
						var attributeValue = attribute.GetValue(item);
						string formattedAttributeValue;

						if (attribute.PropertyType == typeof(string)) {
							formattedAttributeValue = (attributeValue != null) ? attributeValue.ToString() : "";
						} else if (attribute.PropertyType == typeof(int)) {
							formattedAttributeValue = (attributeValue != null) ? attributeValue.ToString() : "0";
						} else {
							if ((attributeValue != null) && (attributeValue.ToString() != "0")) {
								formattedAttributeValue = float.Parse(attributeValue.ToString(), CultureInfo.InvariantCulture).ToString("0.00");
							} else {
								formattedAttributeValue = float.Parse("0", CultureInfo.InvariantCulture).ToString("0.00");
							}
						}

						xmlWri.WriteElementString(formattedAttribute, formattedAttributeValue);
					}

					xmlWri.WriteEndElement();
				}

				xmlWri.WriteEndElement();
				xmlWri.WriteEndDocument();
				xmlWri.Flush();
				xmlWri.Close();
			}
		}

		// Create a formatted name for a XML item
		private static string CreateXMLItemName(string str) {
			return Regex.Replace(str, @"((?<=[a-z])(?=[A-Z0-9])|(?<=[A-Z0-9])(?=[A-Z0-9][a-z]))", "_").ToUpper();
		}
		#endregion
	}
}
