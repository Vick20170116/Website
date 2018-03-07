using MongoDB.Bson;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.User;
using NaimeiKnowledge.Models;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Topic
{
    public class JsonApiTopicInfoResource : IJsonApiResource
    {
        public JsonApiTopicInfoAttributes Attributes { get; set; }

        public string Id { get; set; }

        public JsonApiLinks Links { get; set; }

        public JsonApiMeta Meta { get; set; }

        public JsonApiTopicInfoRelationships Relationships { get; set; }

        public string Type => "topic";

        public static JsonApiTopicInfoResource Create(ForumPost topic)
        {
            var resource = new JsonApiTopicInfoResource
            {
                Attributes = new JsonApiTopicInfoAttributes
                {
                    Created = topic.Id.CreationTime,
                    Statistics = new JsonApiTopicInfoStatisticsAttribute
                    {
                        CommentCount = topic.Statistics.CommentCount,
                        DownvoteCount = topic.Statistics.DownvoteCount,
                        PostCount = topic.Statistics.PostCount,
                        UpvoteCount = topic.Statistics.UpvoteCount
                    },
                    Title = topic.Title,
                    Type = topic.Type,
                    Updated = topic.DateLastModified?.DateTimeOffset.UtcDateTime
                },
                Id = topic.Id.ToString(),
                Links = CreateLinks(topic.Id),
                Relationships = new JsonApiTopicInfoRelationships
                {
                    Owner = JsonApiUserResource.CreateRelationship(topic.OwnerId),
                },
            };
            return resource;
        }

        public static JsonApiLinks CreateLinks(ObjectId id)
        {
            return new JsonApiLinks
            {
                Self = $"/topics/{id}",
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
            return new JsonApiResourceIdentifier("topic", id.ToString());
        }
    }
}
