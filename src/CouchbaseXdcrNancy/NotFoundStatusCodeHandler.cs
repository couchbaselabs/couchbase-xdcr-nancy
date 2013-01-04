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
using Nancy.ErrorHandling;
using Nancy;
using System.IO;
using System.Text;

namespace CouchbaseXdcrNancy
{
	public class StatusCodeHandler : IStatusCodeHandler
	{
		private DefaultStatusCodeHandler _defaultHandler = new DefaultStatusCodeHandler();

		public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
		{
			return _defaultHandler.HandlesStatusCode(statusCode, context);
		}

		public void Handle(HttpStatusCode statusCode, NancyContext context)
		{
			if (statusCode == HttpStatusCode.NotFound)
			{
				if (context.Response == null)
				{
					context.Response = new Response() { StatusCode = statusCode };
				}

				context.Response.ContentType = "application/json";
				context.Response.Contents = s =>
				{
					using (var writer = new StreamWriter(s, Encoding.UTF8))
					{
						var json = "{{ \"error\" : \"Resource Not Found\", \"Requested\" : \"{0}\", \"Method\" : \"{1}\"}}";
						writer.Write(json, context.Request.Url, context.Request.Method);
					}
				};

				return;
			}

			_defaultHandler.Handle(statusCode, context);
		}
	}
}
