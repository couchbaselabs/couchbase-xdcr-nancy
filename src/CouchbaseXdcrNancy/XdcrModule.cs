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
using System.Web;
using Nancy;
using Nancy.Security;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CouchbaseXdcrNancy
{
	public class XdcrModule : NancyModule
	{
		private const string XDCR_RECEIVER = "127.0.0.1";
		private const int XDCR_PORT = 8675; //Port configured in Visual Studio project properties
		private const string XDCR_BUCKET = "default";
		private const string UUID_POOL = "3b5211459ec34c589522f78c2284099e";
		private const string UUID_BUCKET = "9e4d14d5a9be45cba5ec5534f42e129b";

		private const string REGEX_VBUCKET =        @"/(?<bucket>[\w]{1,})/(?<vbucket>[\d]{1,})(;|%3b)(?<uuid>[\w]{1,})";
		private const string REGEX_MASTER_VBUCKET = @"/(?<bucket>[\w]{1,})/(?<master>[\w]{1,})(;|%3b)(?<uuid>[\w]{1,})";
		private const string REGEX_MASTER_VBUCKET_LOCAL = @"/(?<bucket>[\w]{1,})/(?<master>[\w]{1,})(;|%3b)(?<uuid>[\w]{1,})/_local/(?<vbucket>[\d]{1,4})-(?<rev>[\w]{1,})/(?<bucket>[\w]{1,})/(?<bucket>[\w]{1,})";

		private const string REGEX_POST_ROOT = @"/(?<a>[\w]{1,})/(?<b>[\w]{1,})(;|%3b)(?<c>[\w]{1,})/";
		private const string REGEX_REVS_DIFF = REGEX_POST_ROOT + "_revs_diff";
		private const string REGEX_FULL_COMMIT = REGEX_POST_ROOT + "_ensure_full_commit";
		private const string REGEX_BULK_DOCS = REGEX_POST_ROOT + "_bulk_docs";

		private List<int[]> VBucketMap = Enumerable.Range(0, 1024).Select(i => new int[] { 0, 1 }).ToList();
		
		public XdcrModule(IReplicationHandler handler)
		{
			this.RequiresAuthentication();

			Get["/pools"] = x =>
				{
					var output = new
					{
						pools = new object[] 
						{
							new { name = "default", uri = "/pools/default?uuid=" + UUID_POOL }
						},
						uuid = UUID_POOL
					};

					return Response.AsJson(output);
				};

			Get["pools/default"] = x =>
				{
					var output = new
					{
						buckets = new { uri = "/pools/default/buckets?uuid=" + UUID_POOL },
						nodes = new object[] 
						{	
							new 
							{ 
								ports = new { direct = XDCR_PORT } ,
								couchApiBase = string.Concat("http://", XDCR_RECEIVER, ":", XDCR_PORT, "/"),
								hostname = string.Concat(XDCR_RECEIVER, ":", XDCR_PORT) 
							}
						}
					};

					return Response.AsJson(output);
				};

			Get["pools/default/buckets"] = x =>
			{
				var output = new object[]
				{
					new 
					{
						bucketCapabilities = new string[] { "couchapi" },
						bucketType = "membase",
						nodes = new object[] 
						{	
							new 
							{ 
								ports = new { direct = XDCR_PORT } ,
								couchApiBase = string.Concat("http://", XDCR_RECEIVER, ":", XDCR_PORT, "/default"),
								hostname = string.Concat(XDCR_RECEIVER, ":", XDCR_PORT) 
							}
						},
						name = XDCR_BUCKET,
						vBucketServerMap = new
						{
							serverList = new string[] { string.Concat(XDCR_RECEIVER, ":", XDCR_PORT) },
							vBucketMap = VBucketMap
						},
						uuid = UUID_BUCKET,
						uri = string.Concat("/pools/default/buckets/", XDCR_BUCKET, "?bucket_uuid=", UUID_BUCKET)
					}
				};

				return Response.AsJson(output);
			};

			Get["pools/default/buckets/{bucket}"] = x =>
			{
				var output = new object[]
				{
					new 
					{
						bucketCapabilities = new string[] { "couchapi" },
						bucketType = "membase",
						nodes = new object[] 
						{	
							new 
							{ 
								ports = new { direct = XDCR_PORT } ,
								couchApiBase = string.Concat("http://", XDCR_RECEIVER, ":", XDCR_PORT, "/default"),
								hostname = string.Concat(XDCR_RECEIVER, ":", XDCR_PORT) 
							}
						},
						name = XDCR_BUCKET,
						vBucketServerMap = new
						{
							serverList = new string[] { string.Concat(XDCR_RECEIVER, ":", XDCR_PORT) },
							vBucketMap = VBucketMap
						},
						uuid = UUID_BUCKET,
						uri = string.Concat("/pools/default/buckets/", XDCR_BUCKET, "?bucket_uuid=", UUID_BUCKET)
					}
				};

				return Response.AsJson(output);
			};

			Get[REGEX_VBUCKET] = x =>
			{
				var status = getBucketExistsStatusCode(x.bucket);

				if (Request.Method == "HEAD")
				{
					return status;
				}

				var result = new { db_name = XDCR_BUCKET };

				return status == HttpStatusCode.OK ? Response.AsJson(result) : null;
			};

			Get[REGEX_MASTER_VBUCKET] = x =>
			{
				var status = getBucketExistsStatusCode(x.bucket);

				if (Request.Method == "HEAD")
				{
					return status;
				}

				var result = new { db_name = XDCR_BUCKET };

				return status == HttpStatusCode.OK ? Response.AsJson(result) : null;
			};

			//TODO: figure out a regex for both this pattern and master_vbucket - Sinatra version works like that
			Get[REGEX_MASTER_VBUCKET_LOCAL] = x =>
			{
				var status = getBucketExistsStatusCode(x.bucket);

				if (Request.Method == "HEAD")
				{
					return status;
				}

				var result = new { db_name = XDCR_BUCKET };

				return status == HttpStatusCode.OK ? Response.AsJson(result) : null;
			};

			Post[REGEX_REVS_DIFF] = x =>
			{
				var body = "";
				Context.Request.Body.Position = 0;
				using (var sr = new StreamReader(Context.Request.Body))
				{
					body = sr.ReadToEnd();
				}
				var jobj = JObject.Parse(body);

				var outDict = new Dictionary<string, object>();
				foreach (var item in jobj)
				{
					var key = item.Key;
					var rev = item.Value.ToString();
					if (handler.IsMissing(key, rev)) 
					{
						outDict[key] = new { missing = rev };
					}
				}

				return Response.AsJson(outDict);
			};

			Post[REGEX_FULL_COMMIT] = x =>
			{
				return Response.AsJson(new { ok = true }, HttpStatusCode.Created);
			};

			Post[REGEX_BULK_DOCS] = x =>
			{
				var body = "";
				Context.Request.Body.Position = 0;
				using (var sr = new StreamReader(Context.Request.Body))
				{
					body = sr.ReadToEnd();
				}
				var jobj = JObject.Parse(body);

				var newEdits = jobj.Value<bool>("new_edits");
				var docs = jobj.Value<JArray>("docs");
				foreach (var doc in docs)
				{
					var originalDoc = Encoding.UTF8.GetString(Convert.FromBase64String(doc.Value<string>("base64")));
					var meta = doc["meta"] as JObject;

					var document = new Document
					{
						Id = meta.Value<string>("id"),
						Revision = meta.Value<string>("rev"),
						Expiration = meta.Value<int>("expiration"),
						Flags = meta.Value<int>("flags"),
						Value = originalDoc
					};
					
					handler.CreateDocument(document);
				}

				return HttpStatusCode.Created;
			};
		}

		private HttpStatusCode getBucketExistsStatusCode(string database)
		{
			return database == XDCR_BUCKET ? HttpStatusCode.OK : HttpStatusCode.NotFound;
		}
	}
}