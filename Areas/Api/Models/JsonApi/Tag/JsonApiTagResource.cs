using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using NaimeiKnowledge.Models;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Tag
{
    public class JsonApiTagResource : IJsonApiResource
    {
        public JsonApiTagAttributes Attributes { get; set; }

        public string Id { get; set; }

        public JsonApiLinks Links { get; set; }

        public JsonApiMeta Meta { get; set; }

        public string Type { get; } = "tag";

        public static JsonApiTagResource Create(ForumTag tag)
        {
            return new JsonApiTagResource
            {
                Attributes = new JsonApiTagAttributes
                {
                    Name = tag.Name
                },
                Id = tag.Id.ToString(),
            };
        }

        public static JsonApiLinks CreateLinks(ObjectId id)
        {
            return new JsonApiLinks
            {
                Self = $"/tags/{id}",
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

        public static JsonApiRelationshipMany CreateRelationshipMany(IEnumerable<ObjectId> ids)
        {
            return new JsonApiRelationshipMany
            {
                Data = ids.Select(x => CreateResourceIdentifier(x)).ToList(),
            };
        }

        public static JsonApiResourceIdentifier CreateResourceIdentifier(ObjectId id)
        {
            return new JsonApiResourceIdentifier("tag", id.ToString());
        }

        public ForumTag CreateDatabaseModel()
        {
            var tag = new ForumTag
            {
                Description = this.Attributes.Description,
                Name = this.Attributes.Name
            };
            if (this.Id != null)
            {
                tag.Id = ObjectId.Parse(this.Id);
            }

            return tag;
        }
    }
}
