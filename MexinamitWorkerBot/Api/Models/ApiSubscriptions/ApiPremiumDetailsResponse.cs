// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);

using System.Text.Json.Serialization;
using Newtonsoft.Json;

public class Pivot
{
    [JsonPropertyName("subscription")]
    public int Subscription { get; set; }

    [JsonPropertyName("movie_resolution")]
    public int MovieResolution { get; set; }
}
[JsonArray]
public class MovieResolution
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("created_by")]
    public int CreatedBy { get; set; }

    [JsonPropertyName("updated_by")]
    public int UpdatedBy { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("pivot")]
    public Pivot Pivot { get; set; }
}
public class Datum
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("month")]
    public int Month { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("downloads")]
    public int Downloads { get; set; }

    [JsonPropertyName("watch_on")]
    public List<string> WatchOn { get; set; }

    [JsonPropertyName("ads")]
    public int Ads { get; set; }

    [JsonPropertyName("bonus")]
    public int Bonus { get; set; }

    [JsonPropertyName("multi_level_payment")]
    public int MultiLevelPayment { get; set; }

    [JsonPropertyName("lotteries")]
    public int Lotteries { get; set; }

    [JsonPropertyName("investment")]
    public int Investment { get; set; }

    [JsonPropertyName("movie_resolutions")]
    public List<MovieResolution> MovieResolutions { get; set; }
}

public class ApiPremiumDetailsResponse
{
    [JsonPropertyName("data")]
    public List<Datum> Data { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }
}

