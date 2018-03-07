using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Post
{
    public class JsonApiPostRelationships
    {
        public JsonApiRelationship Owner { get; set; }

        public JsonApiRelationship Parent { get; set; }

        public JsonApiRelationshipMany Tags { get; set; }
    }
}
