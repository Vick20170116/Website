using Newtonsoft.Json;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Session
{
    public class JsonApiSessionAttributes
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string Provider { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool RememberMe { get; set; }
    }
}
