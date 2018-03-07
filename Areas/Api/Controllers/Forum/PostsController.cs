using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using NaimeiKnowledge.Areas.Api.Models;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.Post;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.User;
using NaimeiKnowledge.Models;
using NaimeiKnowledge.Services;
using ZetaLib.Database;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Controllers.Forum
{
    [Produces("application/vnd.api+json")]
    [Route("[controller]")]
    [Authorize]
    public class PostsController : JsonApiController
    {
        private readonly ForumManager forumManager;
        private readonly ILogger logger;
        private readonly UserManager<ApplicationUser> userManager;

        public PostsController(
            UserManager<ApplicationUser> userManager,
            ForumManager forumManager,
            ILogger<PostsController> logger)
        {
            this.userManager = userManager;
            this.forumManager = forumManager;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePostAsync([FromBody]JsonApiPostDocument requestDocument)
        {
            var responseDocument = new JsonApiDocument();
            if (requestDocument is null ||
                requestDocument.Data is null ||
                requestDocument.Data.Attributes is null)
            {
                return this.BadRequest(responseDocument);
            }

            var userId = ObjectId.Parse(this.userManager.GetUserId(this.User));
            var post = requestDocument.Data.CreateDatabaseModel();
            if (post.Body.Any(x => x.Type == "text" && string.IsNullOrEmpty(x.Text)))
            {
                return this.BadRequest(responseDocument);
            }

            post.OwnerId = userId;
            post.Id = ObjectId.GenerateNewId();
            await this.forumManager.CreateAsync(post);
            responseDocument.Links = JsonApiPostResource.CreateLinks(post.Id);
            return this.Accepted(responseDocument);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostAsync(string id)
        {
            var responseDocument = new JsonApiPostDocument();
            if (!(ObjectId.TryParse(id, out var postId)))
            {
                return this.NotFound(responseDocument);
            }

            var post = await this.forumManager.FindByIdAsync(postId);
            if (post is null)
            {
                return this.NotFound(responseDocument);
            }

            responseDocument.Data = JsonApiPostResource.Create(post);
            foreach (var query in this.Request.Query)
            {
                switch (query.Key)
                {
                    case "include":
                        {
                            var userDictionary = new Dictionary<ObjectId, JsonApiUserResource>();
                            foreach (var value in query.Value)
                            {
                                switch (value)
                                {
                                    case "owner":
                                        {
                                            var resource = responseDocument.Data;
                                            if (!(resource.Relationships is null || resource.Relationships.Owner is null))
                                            {
                                                var userId = ObjectId.Parse(resource.Relationships.Owner.Data.Id);
                                                if (!(userDictionary.ContainsKey(userId)))
                                                {
                                                    var user = await this.userManager.FindByIdAsync(userId.ToString());
                                                    userDictionary[userId] = user.GetJsonApiResourceFor(user) as JsonApiUserResource;
                                                }
                                            }
                                        }

                                        break;
                                }
                            }

                            if (userDictionary.Count > 0)
                            {
                                responseDocument.Included = new List<IJsonApiResource>();
                                foreach (var value in userDictionary.Values)
                                {
                                    responseDocument.Included.Add(value);
                                }
                            }
                        }

                        break;
                }
            }

            responseDocument.Links = JsonApiPostResource.CreateLinks(post.Id);
            return this.Ok(responseDocument);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostsAsync()
        {
            var responseDocument = new JsonApiMultiResourceDocument();
            var topicIdQuery = this.Request.Query.FirstOrDefault(x => x.Key.ToLowerInvariant() == "topicId".ToLowerInvariant());
            if (!(topicIdQuery.Equals(default(KeyValuePair<string, StringValues>))))
            {
                var topicIdString = topicIdQuery.Value.FirstOrDefault();
                if (!(ObjectId.TryParse(topicIdString, out var topicId)))
                {
                    responseDocument.Data = new List<IJsonApiResourceIdentifier>();
                    responseDocument.Links = new JsonApiLinks { Self = $"/posts?topicId={topicId}" };
                    return this.Ok(responseDocument);
                }

                var posts = await this.forumManager.FindAsync(x => x.Id == topicId || x.ParentId == topicId);
                responseDocument.Data = posts.Select(x => JsonApiPostResource.Create(x) as IJsonApiResourceIdentifier).ToList();
                responseDocument.Links = new JsonApiLinks { Self = $"/posts?topicId={topicId}" };
            }
            else
            {
                var posts = await this.forumManager.ForumStore.QueryablePosts.ToListAsync();
                responseDocument.Data = posts.Select(x => JsonApiPostResource.Create(x) as IJsonApiResourceIdentifier).ToList();
                responseDocument.Links = new JsonApiLinks { Self = "/posts" };
            }

            var includeQuery = this.Request.Query.FirstOrDefault(x => x.Key == "include");
            if (!(includeQuery.Equals(default(KeyValuePair<string, StringValues>))))
            {
                var userDictionary = new Dictionary<ObjectId, JsonApiUserResource>();
                var currentUser = await this.userManager.GetUserAsync(this.User);
                foreach (var value in includeQuery.Value)
                {
                    if (value == "owner")
                    {
                        foreach (JsonApiPostResource resource in responseDocument.Data)
                        {
                            if (resource.Relationships != null && resource.Relationships.Owner != null)
                            {
                                var userId = ObjectId.Parse(resource.Relationships.Owner.Data.Id);
                                if (!(userDictionary.ContainsKey(userId)))
                                {
                                    var user = await this.userManager.FindByIdAsync(userId.ToString());
                                    userDictionary[userId] = user.GetJsonApiResourceFor(currentUser) as JsonApiUserResource;
                                }
                            }
                        }
                    }
                }

                if (userDictionary.Count > 0)
                {
                    if (responseDocument.Included is null)
                    {
                        responseDocument.Included = new List<IJsonApiResource>();
                    }

                    foreach (var value in userDictionary.Values)
                    {
                        responseDocument.Included.Add(value);
                    }
                }
            }

            return this.Ok(responseDocument);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePostAsync(string id, [FromBody]JsonApiPostDocument requestDocument)
        {
            var responseDocument = new JsonApiPostDocument();
            if (requestDocument is null ||
                requestDocument.Data is null ||
                requestDocument.Data.Attributes is null)
            {
                return this.BadRequest(responseDocument);
            }

            if (!(ObjectId.TryParse(id, out var postId)))
            {
                return this.NotFound(responseDocument);
            }

            var postBasicInfo = await this.forumManager.FindAndIncludeBasicInfoByIdAsync(postId);
            if (postBasicInfo is null)
            {
                return this.NotFound(responseDocument);
            }

            var currentUserId = ObjectId.Parse(this.userManager.GetUserId(this.User));
            if (postBasicInfo.OwnerId != currentUserId)
            {
                return this.Forbidden(responseDocument);
            }

            var post = requestDocument.Data.CreateDatabaseModel();
            post.OwnerId = currentUserId;
            post.ParentId = postBasicInfo.ParentId;
            await this.forumManager.UpdateAsync(post);
            return this.Accepted(responseDocument);
        }
    }
}
