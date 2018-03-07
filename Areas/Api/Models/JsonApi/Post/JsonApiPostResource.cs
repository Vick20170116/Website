using System.Linq;
using MongoDB.Bson;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.Tag;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.Topic;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.User;
using NaimeiKnowledge.Models;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Post
{
    public class JsonApiPostResource : IJsonApiResource
    {
        public JsonApiPostAttributes Attributes { get; set; }

        public string Id { get; set; }

        public JsonApiLinks Links { get; set; }

        public JsonApiMeta Meta { get; set; }

        public JsonApiPostRelationships Relationships { get; set; }

        public string Type => "post";

        public static JsonApiPostResource Create(ForumPost post)
        {
            var resource = new JsonApiPostResource
            {
                Attributes = new JsonApiPostAttributes
                {
                    Body = post.Body,
                    Created = post.Id.CreationTime,
                    Statistics = new JsonApiPostStatisticsAttribute
                    {
                        CommentCount = post.Statistics.CommentCount,
                        DownvoteCount = post.Statistics.DownvoteCount,
                        UpvoteCount = post.Statistics.UpvoteCount
                    },
                    Title = post.Title,
                    Type = post.Type,
                    Updated = post.DateLastModified?.DateTimeOffset.UtcDateTime,
                },
                Id = post.Id.ToString(),
                Links = CreateLinks(post.Id),
                Relationships = new JsonApiPostRelationships
                {
                    Owner = JsonApiUserResource.CreateRelationship(post.OwnerId),
                },
            };
            if (post.ParentId != default)
            {
                resource.Relationships.Parent = JsonApiTopicInfoResource.CreateRelationship(post.ParentId);
            }

            if (post.Tags != null && post.Tags.Count > 0)
            {
                resource.Relationships.Tags = JsonApiTagResource.CreateRelationshipMany(post.Tags);
            }

            return resource;
        }

        public static JsonApiLinks CreateLinks(ObjectId id)
        {
            return new JsonApiLinks
            {
                Self = $"/posts/{id}"
            };
        }

        public ForumPost CreateDatabaseModel()
        {
            var post = new ForumPost
            {
                Body = this.Attributes.Body.ToList(),
                Title = this.Attributes.Title,
                Type = this.Attributes.Type,
            };
            if (this.Id != null)
            {
                post.Id = ObjectId.Parse(this.Id);
            }

            if (this.Relationships != null)
            {
                if (this.Relationships.Owner != null)
                {
                    post.OwnerId = ObjectId.Parse(this.Relationships.Owner.Data.Id);
                }

                if (this.Relationships.Parent != null)
                {
                    post.ParentId = ObjectId.Parse(this.Relationships.Parent.Data.Id);
                }

                if (this.Relationships.Tags != null)
                {
                    post.Tags = this.Relationships.Tags.Data.Select(x => ObjectId.Parse(x.Id)).ToList();
                }
            }

            return post;
        }
    }
}
