using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ASPMicroService.Startup
{
	public static partial class ServiceInitializer
	{
		public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
		{
			services.ConfigureCORS();
			services.ConfigureAuthentication();
			services.ConfigureDynamoDB();
			RegisterCustomDependencies(services);

			return services;
		}

		private static void ConfigureCORS(this IServiceCollection services) 
		{
			services.AddCors(options =>
			{
				options.AddPolicy("CORSPolicy",
					builder =>
					{
						builder
							.WithOrigins("https://gesib.site")
							.AllowAnyHeader()
							.AllowAnyMethod()
							.AllowCredentials();
					});
			});
		}

		private static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
		{

			var clientId = Environment.GetEnvironmentVariable("GITHUB_CLIENTID");
			var secret = Environment.GetEnvironmentVariable("GITHUB_SECRET");

			services.AddAuthentication("cookie")
				.AddCookie("cookie")
				.AddOAuth("github", options =>
				{
					options.SignInScheme = "cookie";
					options.ClientId = clientId!;
					options.ClientSecret = secret!;

					options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
					options.TokenEndpoint = "https://github.com/login/oauth/access_token";
					options.CallbackPath = new PathString("/oauth/github-cb");
					options.SaveTokens = true;
					options.UserInformationEndpoint = "https://api.github.com/user";

					options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
					options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
					options.ClaimActions.MapJsonKey("urn:github:login", "login");
					options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
					options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

					options.Events.OnCreatingTicket = async ctx =>
					{
						var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
						request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
						
						var response = await ctx.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ctx.HttpContext.RequestAborted);
						response.EnsureSuccessStatusCode();

						var jsonString = await response.Content.ReadAsStringAsync();
						var json = JsonDocument.Parse(jsonString);

						ctx.RunClaimActions(json.RootElement);
					};
				});

			return services;
		}

		private static IServiceCollection ConfigureDynamoDB(this IServiceCollection services)
		{
			var acceskey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
			var secret = Environment.GetEnvironmentVariable("AWS_SECRET");
			var credentials = new BasicAWSCredentials(acceskey, secret);
			var config = new AmazonDynamoDBConfig()
			{
				RegionEndpoint = RegionEndpoint.EUNorth1
			};

			var client = new AmazonDynamoDBClient(credentials, config);

			services.AddSingleton<IAmazonDynamoDB>(client);
			services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

			return services;
		}

		private static void RegisterCustomDependencies(IServiceCollection services)
		{
			services.AddSingleton<ChatAPIManager>();
		}
	}
}