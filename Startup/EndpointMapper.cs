using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;

namespace ASPMicroService.Startup
{
	public static class EndpointMapper
	{
		public static WebApplication ConfigureEndpoints(this WebApplication app)
		{
			app.MapGet("/login", (HttpContext ctx) => {
				return Results.Challenge(
					new AuthenticationProperties()
					{
						RedirectUri = "https://gesib.site/portfolio.html"
					},
					authenticationSchemes: new List<string>() { "github" }
				);
			});

			app.MapGet("/isAuthenticated", (HttpContext context) => {
				return context.User.Identity?.IsAuthenticated;
			});

			app.MapPost("/ask", async context =>
			{
				string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
				var chatAPIManager = context.RequestServices.GetRequiredService<ChatAPIManager>();
				var chat = chatAPIManager.Conversation;

				chat.AppendUserInput(requestBody);

				string responseData = await chat.GetResponseFromChatbotAsync();
				await context.Response.WriteAsync(responseData);
			});

			app.MapPost("/clear", async context =>
			{
				var chatAPIManager = context.RequestServices.GetRequiredService<ChatAPIManager>();
				chatAPIManager.ClearConversation();
				context.Response.StatusCode = StatusCodes.Status200OK;
				await context.Response.WriteAsync("Conversation cleared successfully");
			});


			app.MapPost("/comments", async context =>
			{
				if (!UserExtensions.TryGetUserId(context, out var userId))
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return;
				}

				string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

				if (String.IsNullOrEmpty(requestBody))
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return;
				}

				var name = context.User.Identity!.Name ?? "Anonymous";
				var id = Guid.NewGuid().ToString();
				var timeStamp = DateTime.UtcNow;

				var comment = new Comment
				{
					Pk = id,
					Sk = timeStamp.ToString(),
					UserDisplayname = name,
					Id = id,
					UserId = userId,
					Text = requestBody,
					Time = DateTime.UtcNow
				};

				var dbContext = context.RequestServices.GetRequiredService<IDynamoDBContext>();
				await dbContext!.SaveAsync(comment);

				context.Response.StatusCode = StatusCodes.Status201Created;
				context.Response.ContentType = "application/json";
				await context.Response.WriteAsync(JsonConvert.SerializeObject(comment));

			});

			app.MapPut("/editComment/{id}", async context =>
			{
				var idObj = RoutingHttpContextExtensions.GetRouteValue(context, "id");
				if (idObj == null)
				{
					context.Response.StatusCode = StatusCodes.Status400BadRequest;
					await context.Response.WriteAsync("Invalid ID");
					return;
				}

				if (!UserExtensions.TryGetUserId(context, out string? userId))
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return;
				}

				var id = idObj.ToString();
				string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
				var db = context.RequestServices.GetRequiredService<IDynamoDBContext>();
				var comments = await db.QueryAsync<Comment>(id).GetRemainingAsync();
				var comment = comments?.FirstOrDefault();

				if (comment == null)
				{
					context.Response.StatusCode = StatusCodes.Status404NotFound;
					return;
				}

				if (comment.UserId != userId || String.IsNullOrEmpty(requestBody))
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return;
				}

				comment.Text = requestBody;
				db?.SaveAsync(comment);

				context.Response.StatusCode = StatusCodes.Status200OK;
			});

			app.MapDelete("/deleteComment/{id}", async context =>
			{
				var idObj = RoutingHttpContextExtensions.GetRouteValue(context, "id");
				if (idObj == null)
				{
					context.Response.StatusCode = StatusCodes.Status400BadRequest;
					await context.Response.WriteAsync("Invalid ID");
					return;
				}

				if (!UserExtensions.TryGetUserId(context, out string? userId))
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return;
				}

				var id = idObj.ToString();

				var db = context.RequestServices.GetRequiredService<IDynamoDBContext>();
				var comments = await db.QueryAsync<Comment>(id).GetRemainingAsync();
				var comment = comments?.FirstOrDefault();

				if (comment == null)
				{
					context.Response.StatusCode = StatusCodes.Status404NotFound;
					return;
				}

				if (comment.UserId != userId)
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return;
				}

				await db.DeleteAsync(comment);

				context.Response.StatusCode = StatusCodes.Status200OK;
			});


			app.MapGet("/comments", async context =>
			{
				var dbContext = context.RequestServices.GetRequiredService<IDynamoDBContext>();
				var table = dbContext.GetTargetTable<Comment>();
				var conditions = new List<ScanCondition>();
				var comments = await dbContext!.ScanAsync<Comment>(conditions).GetRemainingAsync();
				var currentUser = UserExtensions.TryGetUserId(context, out string? userId) ? userId : null;
				var commentList = new List<KeyValuePair<Comment, bool>>();
				
				comments.Sort((commentX, commentY) => commentX.Time.CompareTo(commentY.Time));

				foreach (var comment in comments)
				{
					bool isCurrentUserAuthor = (currentUser != null && comment.UserId == currentUser);
					commentList.Add(new KeyValuePair<Comment, bool>(comment, isCurrentUserAuthor));
				}

				context.Response.StatusCode = StatusCodes.Status200OK;
				context.Response.ContentType = "application/json";
				var json = JsonConvert.SerializeObject(commentList);
				await context.Response.WriteAsync(json);
			});

			return app;
		}
	}
}