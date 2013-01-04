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
using Nancy.TinyIoc;
using Nancy.Bootstrapper;
using Nancy.Authentication.Basic;
using Nancy.Diagnostics;
using Nancy.ErrorHandling;
using System.IO;
using System.Text;

namespace CouchbaseXdcrNancy
{
	public class ApplicationBootstrapper : DefaultNancyBootstrapper
	{
		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			base.ApplicationStartup(container, pipelines);

			pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(
						container.Resolve<IUserValidator>(),
						"Couchbase XDCR"));

			StaticConfiguration.EnableRequestTracing = true;

			NancyInternalConfiguration.WithOverrides(c =>
				{
					c.StatusCodeHandlers.Clear();
					c.StatusCodeHandlers.Add(typeof(StatusCodeHandler));
				}
			);

			container.Register<IReplicationHandler>(new XmlReplicator());
		}

		protected override DiagnosticsConfiguration DiagnosticsConfiguration
		{
			get { return new DiagnosticsConfiguration { Password = @"A2mVtH/XRT\p,B" }; }
		}

	}
}