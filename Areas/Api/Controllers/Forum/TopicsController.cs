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
using NaimeiKnowledge.Areas.Api.Models.JsonApi.Topic;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.User;
using NaimeiKnowledge.Models;
using NaimeiKnowledge.Services;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Controllers.Forum
{
    [Produces("application/vnd.api+json")]
    [Route("[controller]")]
    [Authorize]
    public class TopicsController : JsonApiController
    {
        private readonly ForumManager forumManager;
        private readonly ILogger logger;
        private readonly UserManager<ApplicationUser> userManager;

        public TopicsController(
            UserManager<ApplicationUser> userManager,
            ForumManager forumManager,
            ILogger<TopicsController> logger)
        {
            this.userManager = userManager;
            this.forumManager = forumManager;
            this.logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopicsAsync()
        {
            var responseDocument = new JsonApiMultiResourceDocument();
            var topics = await this.forumManager.FindAndIncludeTopicInfoAsync();
            responseDocument.Data = topics.Select(x => JsonApiTopicInfoResource.Create(x) as IJsonApiResourceIdentifier).ToList();
            responseDocument.Links = new JsonApiLinks { Self = "/topics" };
            var includeQuery = this.Request.Query.FirstOrDefault(x => x.Key == "include");
            if (!(includeQuery.Equals(default(KeyValuePair<string, StringValues>))))
            {
                var userDictionary = new Dictionary<ObjectId, JsonApiUserResource>();
                var currentUser = await this.userManager.GetUserAsync(this.User);
                foreach (var value in includeQuery.Value)
                {
                    if (value == "owner")
                    {
                        foreach (JsonApiTopicInfoResource resource in responseDocument.Data)
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
    }
}
