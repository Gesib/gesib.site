using Microsoft.Extensions.FileProviders;

namespace ASPMicroService.Startup
{
	public static class MiddlewareInitializer
	{
		public static WebApplication ConfigureMiddleware(this WebApplication app, IWebHostEnvironment env)
		{
			app.UseCors("CORSPolicy");

			var rootPath = env.WebRootPath ??= Directory.GetCurrentDirectory();
			app.UseFileServer(new FileServerOptions()
			{
				FileProvider = new PhysicalFileProvider(
					Path.Combine(rootPath, "StaticFiles")
				),
				RequestPath = new PathString("")
			});

			app.UseAuthentication();

			return app;
		}
	}
}
