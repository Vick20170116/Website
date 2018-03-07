using System;
using MongoDB.Bson;
using NaimeiKnowledge.Models;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Link
{
    public class JsonApiLinkResource : IJsonApiResource
    {
        public JsonApiLinkAttributes Attributes { get; set; }

        public string Id { get; set; }

        public JsonApiLinks Links { get; set; }

        public JsonApiMeta Meta { get; set; }

        public string Type => "link";

        public static JsonApiLinkResource Create(TaggedLink link)
        {
            return new JsonApiLinkResource
            {
                Attributes = new JsonApiLinkAttributes
                {
                    Tags = string.Join(',', link.Tags),
                    Title = link.Title,
                    Url = link.Url.OriginalString,
                },
                Id = link.Id.ToString(),
                Links = CreateLinks(link.Id.ToString()),
            };
        }

        public static JsonApiLinks CreateLinks(string id)
        {
            return new JsonApiLinks
            {
                Self = $"/links/{id}",
            };
        }

        public TaggedLink CreateDatabaseModel()
        {
            return new TaggedLink
            {
                Id = ObjectId.Parse(this.Id),
                Tags = this.Attributes.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries),
                Title = this.Attributes.Title,
                Url = new Uri(this.Attributes.Url)
            };
        }
    }
}
