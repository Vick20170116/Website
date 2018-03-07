using MongoDB.Bson;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.User;
using NaimeiKnowledge.Models;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Session
{
    public class JsonApiSessionResource : IJsonApiResource
    {
        public JsonApiSessionAttributes Attributes { get; set; }

        public string Id { get; set; }

        public JsonApiLinks Links { get; set; }

        public JsonApiMeta Meta { get; set; }

        public JsonApiSessionRelationships Relationships { get; set; }

        public string Type => "session";

        public static JsonApiSessionResource Create(ApplicationUser user)
        {
            return new JsonApiSessionResource
            {
                Attributes = new JsonApiSessionAttributes
                {
                    Email = user.Email.Value,
                },
                Id = ObjectId.GenerateNewId().ToString(),
                Relationships = new JsonApiSessionRelationships
                {
                    User = JsonApiUserResource.CreateRelationship(user.Id),
                },
            };
        }
    }
}
