using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Reflection;

namespace ZanthusXMLConverter {
	class FileWriter {
		#region Tuples sufixes
		// Strings to build a tuple's key (to be used in searchings at appSettings's App.config)
		const string FileFolderSufix = "FileFolder"; // Sufix to point out a folder containing the requests and responses files of an object class
		const string FileBodyTagSufix = "FileBodyTag"; // Tag to mark all XML file's content (setted at file's beginning) of an object class
		const string FileItemTagSufix = "FileItemTag"; // Tag to mark items of a XML file (setted inside of body tag) of an object class
		const string FileDataSchemaPathSufix = "FileDataSchemaPath"; // Sufix to point out the data schema of a XML file
		const string FileDataSchemaNameSufix = "FileDataSchemaName"; // Sufix to point out the data schema of a XML file
		const string RequestFilePathSufix = "RequestFilePath"; // Sufix to point out the request files path of an object class
		const string RequestFileNameSufix = "RequestFileName"; // Sufix to point out the request files path of an object class
		const string ResponseFilePathSufix = "ResponseFilePath"; // Sufix to point out the response files names of an object class
		const string ResponseFileNameSufix = "ResponseFileName"; // Sufix to point out the response files names of an object class
		#endregion

		// Create a formatted name for a XML item (all caps and divided by underlines)
		private static string CreateXMLItemName(string str) {
			return Regex.Replace(str, @"((?<=[a-z])(?=[A-Z0-9])|(?<=[A-Z0-9])(?=[A-Z0-9][a-z]))", "_").ToUpper();
		}

		#region Get file paths
		// Get current request file's path of an object class
		public static string GetRequestFilePath(Type objectClass, string requestName) {
			var appSettings = ConfigurationManager.AppSettings;
			string requestFilePath = appSettings.Get(RequestFilePathSufix);
			string requestFileFolder = appSettings.Get(objectClass.Name + FileFolderSufix);
			string requestFileName = appSettings.Get(objectClass.Name + requestName + RequestFileNameSufix);

			return requestFilePath + requestFileFolder + requestFileName;
		}

		// Get current response file's path of a class
		public static string GetResponseFilePath(Type objectClass, string requestName) {
			var appSettings = ConfigurationManager.AppSettings;
			string responseFilePath = appSettings.Get(ResponseFilePathSufix);
			string responseFileFolder = appSettings.Get(objectClass.Name + FileFolderSufix);
			string responseFileName = appSettings.Get(objectClass.Name + requestName + ResponseFileNameSufix);

			return responseFilePath + responseFileFolder + responseFileName;
		}

		// Get current file data schema's path of a class
		public static string GetFileDataSchemaPath(Type objectClass, string requestName) {
			var appSettings = ConfigurationManager.AppSettings;
			string fileDataSchemaPath = appSettings.Get(FileDataSchemaPathSufix);
			string fileDataSchemaFolder = appSettings.Get(objectClass.Name + FileFolderSufix);
			string fileDataSchemaName = appSettings.Get(objectClass.Name + requestName + FileDataSchemaNameSufix);

			return fileDataSchemaPath + fileDataSchemaFolder + fileDataSchemaName;
		}
		#endregion

		#region Write XML file from XDocument
		// Write a XML file from a XDocument content
		public static void WriteXMLFromXDocument(XDocument xDoc, string filePath, string fileName) {
			using (XmlWriter xmlWri = XmlWriter.Create(filePath + fileName)) {
				xmlWri.WriteStartDocument();
				WriteXElement(xmlWri, xDoc.Elements().Single());
				xmlWri.WriteEndDocument();
				xmlWri.Flush();
				xmlWri.Close();
			}
		}

		// Write the content of current XElement on a XML file
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
				}
			} else {
				xmlWri.WriteString(xEle.Value);
			}

			xmlWri.WriteEndElement();
		}
		#endregion

		#region Write XML file from object list
		// Write a XML file from the content of an object list
		public static void WriteXMLFromList<T>(List<T> list, List<PropertyInfo> requestedAttributes, string requestName) {
			// Get application settings from configuration (related to the current list's object class)
			var appSettings = ConfigurationManager.AppSettings;
			string responseFilePath = GetResponseFilePath(typeof(T), requestName);
			string responseFileDataSchemaPath = GetFileDataSchemaPath(typeof(T), requestName);
			string responseFileBodyTag = appSettings.Get(typeof(T).Name + FileBodyTagSufix);
			string responseFileItemTag = appSettings.Get(typeof(T).Name + FileItemTagSufix);

			// Write a XML file using object list's content
			using (XmlWriter xmlWri = XmlWriter.Create(responseFilePath)) {
				xmlWri.WriteStartDocument();
				xmlWri.WriteStartElement(responseFileBodyTag);

				// Set a data schema to current XML file (to able reading in Excel)
				xmlWri.WriteAttributeString("xmlns", "xsi", null, @"http://www.w3.org/2001/XMLSchema-instance");
				xmlWri.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, responseFileDataSchemaPath);

				// Write every item of the list into XML file's body content
				foreach (T item in list) {
					xmlWri.WriteStartElement(responseFileItemTag);

					// Write every searched attribute of current item
					foreach (PropertyInfo attribute in requestedAttributes) {
						string formattedAttribute = CreateXMLItemName(appSettings.Get(attribute.Name + "Tag"));
						var attributeValue = attribute.GetValue(item);
						string formattedAttributeValue;

						if (attribute.PropertyType == typeof(string)) {
							formattedAttributeValue = (attributeValue != null) ? attributeValue.ToString() : "";
						} else if (attribute.PropertyType == typeof(int)) {
							formattedAttributeValue = (attributeValue != null) ? attributeValue.ToString() : "0";
						} else {
							if ((attributeValue != null) && (attributeValue.ToString() != "0")) {
								//formattedAttributeValue = float.Parse(attributeValue.ToString(), CultureInfo.InvariantCulture).ToString("0.00");
								formattedAttributeValue = float.Parse(attributeValue.ToString(), NumberStyles.Float).ToString("0.00");
							} else {
								//formattedAttributeValue = float.Parse("0", CultureInfo.InvariantCulture).ToString("0.00");
								formattedAttributeValue = float.Parse("0", NumberStyles.Float).ToString("0.00");
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

			// Write data schema of the current generated XML file
			WriteXSDFromAttributeList(typeof(T), requestedAttributes, requestName);
		}
		#endregion

		#region Write XSD data schema from an attribute (PropertyInfo) list
		// Write a XSD data schema from an attribute list of a object class
		private static void WriteXSDFromAttributeList(Type objectClass, List<PropertyInfo> requestedAttributes, string requestName) {
			var appSettings = ConfigurationManager.AppSettings;
			string fileDataSchemaPath = GetFileDataSchemaPath(objectClass, requestName);
			string fileDataSchemaBodyTag = appSettings.Get(objectClass.Name + FileBodyTagSufix);
			string fileDataSchemaItemTag = appSettings.Get(objectClass.Name + FileItemTagSufix);

			// Write a data schema for XML file's of an object class
			using (XmlWriter xmlWri = XmlWriter.Create(fileDataSchemaPath)) {
				XmlSchema xsdSchema = new XmlSchema();

				// Set rules for XML's body content
				XmlSchemaElement xsdBody = new XmlSchemaElement();
				XmlSchemaComplexType xsdBodyType = new XmlSchemaComplexType();
				XmlSchemaSequence xsdBodyTypeSequence = new XmlSchemaSequence();
				xsdBody.Name = fileDataSchemaBodyTag;
				xsdBody.SchemaType = xsdBodyType;
				xsdBodyType.Particle = xsdBodyTypeSequence;
				xsdSchema.Items.Add(xsdBody);

				// Set rules for XML's items
				XmlSchemaElement xsdItem = new XmlSchemaElement();
				XmlSchemaComplexType xsdItemType = new XmlSchemaComplexType();
				XmlSchemaSequence xsdItemTypeSequence = new XmlSchemaSequence();
				xsdItem.Name = fileDataSchemaItemTag;
				xsdItem.MaxOccursString = "unbounded";
				xsdItem.SchemaType = xsdItemType;
				xsdItemType.Particle = xsdItemTypeSequence;
				xsdBodyTypeSequence.Items.Add(xsdItem);

				// Write rules for every request attribute of current object class
				foreach (PropertyInfo attribute in requestedAttributes) {
					XmlSchemaElement xsdAttribute = new XmlSchemaElement {
						Name = CreateXMLItemName(appSettings.Get(attribute.Name + "Tag"))
					};

					if (attribute.PropertyType == typeof(string)) {
						xsdAttribute.SchemaTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
					} else if (attribute.PropertyType == typeof(int)) {
						xsdAttribute.SchemaTypeName = new XmlQualifiedName("integer", "http://www.w3.org/2001/XMLSchema");
					} else {
						xsdAttribute.SchemaTypeName = new XmlQualifiedName("decimal", "http://www.w3.org/2001/XMLSchema");
					}

					xsdItemTypeSequence.Items.Add(xsdAttribute);
				}

				xsdSchema.Write(xmlWri);
			}
		}
		#endregion
	}
}
