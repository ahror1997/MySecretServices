using Newtonsoft.Json;

namespace MySecretServices.Responses
{
	public class BaseResponse
	{
        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("success")]
        public bool Success { get; set; } = false;

        [JsonProperty("errors")]
        public object Errors { get; set; }
	}
}
