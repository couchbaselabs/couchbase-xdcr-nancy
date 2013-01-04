#region [ License information          ]
/* ************************************************************
 * 
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2012 Couchbase, Inc.
 *    
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *        http://www.apache.org/licenses/LICENSE-2.0
 *    
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *    
 * ************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.IO;
using System.Text;

namespace CouchbaseXdcrNancy
{
	public class XmlReplicator : IReplicationHandler
	{
		private readonly string _path;
		private XDocument doc = new XDocument();

		public XmlReplicator(string path = @"C:\temp\replication.xml")
		{
			_path = path;
			if (! File.Exists(_path))
			{
				//var xml = new XElement("documents");
				File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"utf-8\"?><documents></documents>", Encoding.UTF8);
			}
		}

		public bool IsMissing(string key, string rev)
		{
			var xml = XDocument.Load(_path);
			var documents = xml.Document.Root.Elements("document");
			var document = documents.Where(d => d.Element("meta").Element("rev").Value == rev && d.Element("meta").Element("id").Value == key);
			return document.Count() == 0;
		}

		public void CreateDocument(Document document)
		{
			var xml = XDocument.Load(_path);
			
			var docElement = new XElement("document", 
				new XElement("meta", 
					new XElement("id", document.Id),
					new XElement("rev", document.Revision),
					new XElement("expiration", document.Expiration),
					new XElement("flags", document.Flags)
				),
				new XElement("value", new XCData(document.Value))
			);

			xml.Document.Root.Add(docElement);
			xml.Save(_path);
		}
	}
}