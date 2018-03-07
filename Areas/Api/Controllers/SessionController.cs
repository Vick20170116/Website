using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaimeiKnowledge.Areas.Api.Models;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.Session;
using NaimeiKnowledge.Models;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Controllers
{
    [Produces("application/vnd.api+json")]
    [Route("[controller]")]
    public class SessionController : JsonApiController
    {
        private readonly string controllerName = "Session";
        private readonly ILogger logger;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;

        public SessionController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SessionController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSessionAsync([FromBody]JsonApiSessionDocument requestDocument)
        {
            var responseDocument = new JsonApiDocument();
            if (requestDocument is null ||
                requestDocument.Data is null ||
                requestDocument.Data.Attributes is null)
            {
                return this.BadRequest(responseDocument);
            }

            if (string.IsNullOrWhiteSpace(requestDocument.Data.Attributes.Email) ||
                string.IsNullOrWhiteSpace(requestDocument.Data.Attributes.Password))
            {
                if (string.IsNullOrWhiteSpace(requestDocument.Data.Attributes.Provider))
                {
                    var redirectUrl = this.Url.Action(nameof(this.ExternalLoginCallback), this.controllerName);
                    var properties = this.signInManager.ConfigureExternalAuthenticationProperties(requestDocument.Data.Attributes.Provider, redirectUrl);
                    return this.Challenge(properties, requestDocument.Data.Attributes.Provider);
                }

                return this.BadRequest(responseDocument);
            }

            var user = await this.userManager.FindByEmailAsync(requestDocument.Data.Attributes.Email);
            if (user is null)
            {
                return this.BadRequest(responseDocument);
            }

            var signInResult = await this.signInManager.PasswordSignInAsync(
                user,
                requestDocument.Data.Attributes.Password,
                isPersistent: requestDocument.Data.Attributes.RememberMe,
                lockoutOnFailure: false);
            if (signInResult.Succeeded)
            {
                responseDocument.Data = JsonApiSessionResource.Create(user);
                responseDocument.Included = new List<IJsonApiResource>
                {
                    user.GetJsonApiResourceFor(user),
                };
                return this.Ok(responseDocument);
            }

            if (signInResult.RequiresTwoFactor)
            {
                return this.InternalServerError(responseDocument);
            }

            if (signInResult.IsLockedOut)
            {
                return this.InternalServerError(responseDocument);
            }

            if (signInResult.IsNotAllowed)
            {
                return this.InternalServerError(responseDocument);
            }

            return this.InternalServerError(responseDocument);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSessionAsync()
        {
            var responseDocument = new JsonApiDocument();
            await this.signInManager.SignOutAsync();
            return this.NoContent();
        }

        [HttpGet("logincallback")]
        public IActionResult ExternalLoginCallback(string provider, string remoteError = null)
        {
            var responseDocument = new JsonApiDocument();
            return this.InternalServerError(responseDocument);
        }

        [HttpGet]
        public async Task<IActionResult> GetSessionAsync()
        {
            var responseDocument = new JsonApiDocument();
            var user = await this.userManager.GetUserAsync(this.User);
            if (user is null)
            {
                return this.NotFound(responseDocument);
            }

            responseDocument.Data = JsonApiSessionResource.Create(user);
            responseDocument.Included = new List<IJsonApiResource>
            {
                user.GetJsonApiResourceFor(user),
            };
            return this.Ok(responseDocument);
        }
    }
}
