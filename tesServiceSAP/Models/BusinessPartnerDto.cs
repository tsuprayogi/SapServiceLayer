//BusinessPartnerDto.cs
using Newtonsoft.Json;
public class BusinessPartnerDto
{
    [JsonProperty("CardCode")]
    public string CardCode { get; set; }

    [JsonProperty("CardName")]
    public string CardName { get; set; }

    [JsonProperty("CardType")]
    public string CardType { get; set; }


}

public class ItemResult
{
    [JsonProperty("ItemCode")]
    public string ItemCode { get; set; }
    
    [JsonProperty("ItemName")]
    public string ItemName { get; set; }
    
    [JsonProperty("ItmsGrpCod")]
    public int ItmsGrpCod { get; set; }

}

//SapResponse.cs
public class SapResponse<T>
{
    
[JsonProperty("value")]
    public List<T> Value { get; set; } = new List<T>();


}
//LoginResponse.cs
public class LoginResponse
{
    [JsonProperty("AccessToken")]
    public string AccessToken { get; set; }

    [JsonProperty("SessionId")]
    public string SessionId { get; set; }

    [JsonProperty("RouteId")]
    public string RouteId { get; set; }

    [JsonProperty("Expiration")]
    public DateTime Expiration { get; set; }


}
public class ODataResponse<T>
    {
        [JsonProperty("value")]
    public List<T> Value { get; set; } = new List<T>();

    [JsonProperty("@odata.count")]
    public int? Count { get; set; }


    }

public class QueryResultDto
{
    [JsonProperty("value")]
    public List<ItemResult> Value { get; set; }
}
