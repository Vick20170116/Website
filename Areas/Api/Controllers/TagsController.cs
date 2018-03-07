using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using NaimeiKnowledge.Areas.Api.Models;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.Tag;
using NaimeiKnowledge.Models;
using NaimeiKnowledge.Services;
using ZetaLib.Database;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Controllers
{
    [Produces("application/vnd.api+json")]
    [Route("[controller]")]
    [Authorize]
    public class TagsController : JsonApiController
    {
        private readonly ILogger logger;
        private readonly TagManager tagManager;
        private readonly UserManager<ApplicationUser> userManager;

        public TagsController(
            UserManager<ApplicationUser> userManager,
            TagManager tagManager,
            ILogger<TagsController> logger)
        {
            this.userManager = userManager;
            this.tagManager = tagManager;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTagAsync([FromBody]JsonApiTagDocument requestDocument)
        {
            var responseDocument = new JsonApiDocument();
            if (requestDocument is null ||
                requestDocument.Data is null ||
                requestDocument.Data.Attributes is null ||
                string.IsNullOrWhiteSpace(requestDocument.Data.Attributes.Name))
            {
                return this.BadRequest(requestDocument);
            }

            var tag = requestDocument.Data.CreateDatabaseModel();
            tag.Id = ObjectId.GenerateNewId();
            await this.tagManager.CreateAsync(tag);
            requestDocument.Data = JsonApiTagResource.Create(tag);
            return this.Ok(requestDocument);
        }

        [HttpGet]
        public async Task<IActionResult> GetTagsAsync()
        {
            var responseDocument = new JsonApiMultiResourceDocument();
            var allTags = await this.tagManager.Store.QueryableTags.ToListAsync();
            responseDocument.Data = allTags.Select(x => JsonApiTagResource.Create(x) as IJsonApiResourceIdentifier).ToList();
            return this.Ok(responseDocument);
        }
    }
}
