using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace ZanthusXMLConverter {
	class FileWriter {
		#region XDOCUMENT
		// Write a XML file from a XDocument content
		public static void WriteFromXDocument(XDocument xDoc, string fileName, string saveLocation) {
			using (XmlWriter xmlWri = XmlWriter.Create(saveLocation + fileName)) {
				xmlWri.WriteStartDocument();
				WriteFileXElement(xmlWri, xDoc.Elements().First());
				xmlWri.WriteEndDocument();
				xmlWri.Flush();
				xmlWri.Close();
			}
		}

		// Write the content of a XElement on a XML file
		private static void WriteFileXElement(XmlWriter xmlWri, XElement xEle) {
			xmlWri.WriteStartElement(xEle.Name.LocalName);

			if (xEle.HasAttributes) {
				foreach (XAttribute xAtt in xEle.Attributes()) {
					xmlWri.WriteAttributeString(xAtt.Name.LocalName, xAtt.Value);
				}
			}

			if (xEle.HasElements) {
				foreach (XElement descendant in xEle.Descendants()) {
					WriteFileXElement(xmlWri, descendant);
					xmlWri.WriteEndElement();
				}
			} else {
				xmlWri.WriteString(xEle.Value);
			}
		}
		#endregion

		#region OBJECT LIST
		// Write the content of an object list on a XML file
		public static void WriteFromList<T>(List<T> list, List<PropertyInfo> searchedProperties, string bodyName, string fileName, string saveLocation) {
			using (XmlWriter xmlWri = XmlWriter.Create(saveLocation + fileName)) {
				xmlWri.WriteStartDocument();
				xmlWri.WriteStartElement(bodyName);

				// Write every item of XML body content
				foreach (T item in list) {
					string formattedItemName = CreateXMLName(item.GetType().Name);
					xmlWri.WriteStartElement(formattedItemName);

					// Write every searched property of current item
					foreach (PropertyInfo property in searchedProperties) {
						string formattedPropertyName = CreateXMLName(property.Name);
						var propertyValue = property.GetValue(item);
						string formattedPropertyValue;

						if (property.PropertyType == typeof(string)) {
							formattedPropertyValue = (propertyValue != null) ? propertyValue.ToString() : "";
						} else if (property.PropertyType == typeof(int)) {
							formattedPropertyValue = (propertyValue != null) ? propertyValue.ToString() : "0";
						} else {
							if ((propertyValue != null) && (propertyValue.ToString() != "0")) {
								formattedPropertyValue = float.Parse(propertyValue.ToString(), CultureInfo.InvariantCulture).ToString("0.00");
							} else {
								formattedPropertyValue = float.Parse("0", CultureInfo.InvariantCulture).ToString("0.00");
							}
						}

						xmlWri.WriteElementString(formattedPropertyName, formattedPropertyValue);
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
		private static string CreateXMLName(string str) {
			return Regex.Replace(str, @"((?<=[a-z])(?=[A-Z0-9])|(?<=[A-Z0-9])(?=[A-Z0-9][a-z]))", "_").ToUpper();
		}
		#endregion
	}
}
