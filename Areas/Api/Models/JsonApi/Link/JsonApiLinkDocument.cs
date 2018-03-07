using System.Collections.Generic;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Link
{
    public class JsonApiLinkDocument : IJsonApiDocument
    {
        public JsonApiLinkResource Data { get; set; }

        public IList<JsonApiError> Errors { get; set; }

        public IList<IJsonApiResource> Included { get; set; }

        public JsonApiJsonapi Jsonapi { get; } = new JsonApiJsonapi();

        public JsonApiLinks Links { get; set; }

        public JsonApiMeta Meta { get; set; }
    }
}
