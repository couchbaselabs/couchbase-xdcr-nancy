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

namespace CouchbaseXdcrNancy
{
	public class XdcrModule : NancyModule
	{
		private const string XDCR_RECEIVER = "127.0.0.1";
		private const int XDCR_PORT = 8675; //Port configured in project properties
		private const string XDCR_BUCKET = "default";
		private const string UUID_POOL = "3b5211459ec34c589522f78c2284099e"; //SecureRandom.uuid.gsub("-", "")
		private const string UUID_BUCKET = "9e4d14d5a9be45cba5ec5534f42e129b";//#SecureRandom.uuid.gsub("-", "")

		private List<int[]> VBucketMap = Enumerable.Range(0, 1024).Select(i => new int[] { 0, 1 }).ToList();
		//1024.times { VBucketMap << [0,1] }

		public XdcrModule()
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
		}
	}
}