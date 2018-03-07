using MongoDB.Bson;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.User
{
    public class JsonApiUserResource : IJsonApiResource
    {
        public JsonApiUserAttributes Attributes { get; set; }

        public string Id { get; set; }

        public JsonApiLinks Links { get; set; }

        public JsonApiMeta Meta { get; set; }

        public JsonApiRelationships Relationships { get; set; }

        public string Type => "user";

        public static JsonApiLinks CreateLinks(ObjectId id)
        {
            return new JsonApiLinks
            {
                Self = $"/users/{id}",
            };
        }

        public static JsonApiRelationship CreateRelationship(ObjectId id)
        {
            return new JsonApiRelationship
            {
                Data = CreateResourceIdentifier(id),
                Links = CreateLinks(id),
            };
        }

        public static JsonApiResourceIdentifier CreateResourceIdentifier(ObjectId id)
        {
            return new JsonApiResourceIdentifier("user", id.ToString());
        }
    }
}
