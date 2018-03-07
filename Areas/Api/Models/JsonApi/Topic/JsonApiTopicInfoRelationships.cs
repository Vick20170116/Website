using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Topic
{
    public class JsonApiTopicInfoRelationships
    {
        public JsonApiRelationship Owner { get; set; }

        public JsonApiRelationshipMany Tags { get; set; }
    }
}
