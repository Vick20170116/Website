using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.User
{
    public class JsonApiUserVerificationResource : IJsonApiResource
    {
        public JsonApiUserVerificationAttributes Attributes { get; set; }

        public string Id { get; set; }

        public JsonApiLinks Links { get; set; }

        public JsonApiMeta Meta { get; set; }

        public JsonApiRelationships Relationships { get; set; }

        public string Type => "verification";
    }
}
