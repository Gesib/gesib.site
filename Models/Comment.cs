using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("Comment")]
public class Comment
{
	[DynamoDBHashKey("pk")]
	[DynamoDBProperty("pk")] // Add this attribute
	[JsonPropertyName("pk")]
	public string Pk { get; set; } = default!;

	[DynamoDBRangeKey("sk")]
	[DynamoDBProperty("sk")] // Add this attribute
	[JsonPropertyName("sk")]
	public string Sk { get; set; } = default!;

	[JsonPropertyName("id")]
	public string Id { get; set; } = default!;
	[JsonPropertyName("userDisplayname")]
	public string UserDisplayname { get; set; } = default!;
	[JsonPropertyName("userId")]
	public string UserId { get; set; } = default!;
	[JsonPropertyName("text")]
	public string Text { get; set; } = default!;
	[JsonPropertyName("time")]
	public DateTime Time { get; set; } = default!;
}
