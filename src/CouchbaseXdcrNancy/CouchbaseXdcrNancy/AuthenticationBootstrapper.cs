using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.TinyIoc;
using Nancy.Bootstrapper;
using Nancy.Authentication.Basic;

namespace CouchbaseXdcrNancy
{
	public class AuthenticationBootstrapper : DefaultNancyBootstrapper
	{
		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			base.ApplicationStartup(container, pipelines);

			pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(
						container.Resolve<IUserValidator>(),
						"Couchbase XDCR"));
		}
	}
}