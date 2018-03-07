using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using NaimeiKnowledge.Areas.Api.Models;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.Link;
using NaimeiKnowledge.Models;
using ZetaLib.Database;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Controllers
{
    [Produces("application/vnd.api+json")]
    [Route("[controller]")]
    [Authorize]
    public class LinksController : JsonApiController
    {
        private readonly ILogger logger;
        private readonly IRepository<TaggedLink> repository;
        private readonly UserManager<ApplicationUser> userManager;

        public LinksController(
            UserManager<ApplicationUser> userManager,
            IRepositoryProvider repositoryProvider,
            ILogger<LinksController> logger)
        {
            this.repository = repositoryProvider.GetRepository<TaggedLink>("links");
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLinkAsync([FromBody]JsonApiLinkDocument requestDocument)
        {
            var responseDocument = new JsonApiDocument();
            if (requestDocument is null ||
                requestDocument.Data is null ||
                requestDocument.Data.Attributes is null ||
                requestDocument.Data.Attributes.Url is null)
            {
                return this.BadRequest(responseDocument);
            }

            Uri.TryCreate(requestDocument.Data.Attributes.Url, UriKind.Absolute, out var url);
            if (url is null || (url.Scheme != "http" && url.Scheme != "https"))
            {
                return this.BadRequest(responseDocument);
            }

            TaggedLink link;
            if (string.IsNullOrWhiteSpace(requestDocument.Data.Attributes.Title))
            {
                link = await this.AnalyzeLinkAsync(url, false);
            }
            else
            {
                link = await this.AnalyzeLinkAsync(url);
                link.Title = requestDocument.Data.Attributes.Title;
            }

            if (!(requestDocument.Data.Attributes.Tags is null))
            {
                link.Tags = requestDocument.Data.Attributes.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }

            var result = await this.repository.InsertAsync(link);
            if (result.Succeeded)
            {
                responseDocument.Data = JsonApiLinkResource.Create(link);
                return this.Ok(responseDocument);
            }

            return this.BadRequest(responseDocument);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLinkAsync(string id)
        {
            var responseDocument = new JsonApiDocument();
            if (!(ObjectId.TryParse(id, out var linkId)))
            {
                return this.NotFound(responseDocument);
            }

            var result = await this.repository.DeleteAsync(linkId);
            if (result.Succeeded)
            {
                return this.NoContent();
            }

            return this.NotFound(responseDocument);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLinkAsync(string id)
        {
            var responseDocument = new JsonApiDocument();
            if (!(ObjectId.TryParse(id, out var linkId)))
            {
                return this.NotFound(responseDocument);
            }

            var result = await this.repository.FindAsync(linkId);
            if (result.Data.Count() == 0)
            {
                return this.NotFound(responseDocument);
            }

            responseDocument.Data = JsonApiLinkResource.Create(result.Data.First());
            return this.Ok(responseDocument);
        }

        [HttpGet]
        public async Task<IActionResult> GetLinksAsync()
        {
            var responseDocument = new JsonApiMultiResourceDocument();
            var result = await this.repository.FindAsync(x => true);
            responseDocument.Data = result.Data.Select(x => JsonApiLinkResource.Create(x) as IJsonApiResourceIdentifier).ToList();
            return this.Ok(responseDocument);
        }

        [HttpGet("{id}/reanalyze")]
        public async Task<IActionResult> ReanalyzeLinkAsync(string id)
        {
            var responseDocument = new JsonApiDocument();
            if (!(ObjectId.TryParse(id, out var linkId)))
            {
                return this.NotFound(responseDocument);
            }

            var result = await this.repository.FindAsync(linkId);
            if (result.Data.Count() == 0)
            {
                return this.NotFound(responseDocument);
            }

            var oldLink = result.Data.First();
            var newLink = await AnalyzeLinkAsync(result.Data.First().Url, false);
            newLink.Id = oldLink.Id;
            newLink.Tags = oldLink.Tags;
            result = await this.repository.CreateOrReplaceAsync(newLink);
            if (!(result.Succeeded))
            {
                return this.InternalServerError(responseDocument);
            }

            responseDocument.Data = JsonApiLinkResource.Create(result.Data.First());
            return this.Ok(responseDocument);
        }

        private async Task<TaggedLink> AnalyzeLinkAsync(Uri url, bool onlyHeaders = true)
        {
            var link = new TaggedLink();
            using (var client = new HttpClient())
            {
                var responseMessage = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                link.Url = responseMessage.RequestMessage.RequestUri;
                if (!(onlyHeaders) && responseMessage.Content.Headers.ContentType.MediaType.StartsWith("text/html"))
                {
                    string html;
                    using (var stream = await responseMessage.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream, true))
                    {
                        html = await reader.ReadToEndAsync();
                    }

                    var regex = new Regex(@"(?<=<title.*?>)(.*?)(?=</title>)", RegexOptions.IgnoreCase);
                    link.Title = regex.Match(html).Value.Trim();
                }
            }

            return link;
        }
    }
}
