using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaimeiKnowledge.Areas.Api.Models;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.User;
using NaimeiKnowledge.Models;
using NaimeiKnowledge.Services;
using ZetaLib.AspNetCore.Identity.MongoDB;
using ZetaLib.Database;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Areas.Api.Controllers
{
    [Produces("application/vnd.api+json")]
    [Route("[controller]")]
    [Authorize]
    public class UsersController : JsonApiController
    {
        private readonly string controllerName = "Users";
        private readonly IMailSender emailSender;
        private readonly ILogger logger;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMailSender emailSender,
            ILogger<UsersController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUserAsync([FromBody]JsonApiUserDocument requestDocument)
        {
            var responseDocument = new JsonApiDocument();
            if (requestDocument is null ||
                requestDocument.Data is null ||
                requestDocument.Data.Attributes is null ||
                string.IsNullOrWhiteSpace(requestDocument.Data.Attributes.DisplayName) ||
                string.IsNullOrWhiteSpace(requestDocument.Data.Attributes.Email) ||
                string.IsNullOrWhiteSpace(requestDocument.Data.Attributes.Password))
            {
                return this.BadRequest(responseDocument);
            }

            try
            {
                var mailAddress = new MailAddress(requestDocument.Data.Attributes.Email);
            }
            catch
            {
                return this.BadRequest(responseDocument);
            }

            var user = new ApplicationUser
            {
                DisplayName = requestDocument.Data.Attributes.DisplayName,
                Email = new MongoIdentityUserEmail(requestDocument.Data.Attributes.Email),
                UserName = requestDocument.Data.Attributes.Email,
            };
            var result = await this.userManager.CreateAsync(user, requestDocument.Data.Attributes.Password);
            if (result.Succeeded)
            {
                var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = this.Url.Action(nameof(this.VerifyUserEmailAsync), this.controllerName, new { id = user.Id, token }, this.Request.Scheme);
                await this.emailSender.SendConfirmationMailAsync(user.Email.Value, confirmationLink);
                responseDocument.Data = user.GetJsonApiResourceFor(user);
                return this.Ok(responseDocument);
            }

            var errorDetail = new StringBuilder();
            foreach (var error in result.Errors)
            {
                errorDetail.AppendLine(error.Description);
            }

            return this.BadRequest(responseDocument, "Error", errorDetail.ToString());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserAsync(string id)
        {
            var responseDocument = new JsonApiDocument();
            var requestedUser = await this.userManager.FindByIdAsync(id);
            if (requestedUser is null)
            {
                return this.NotFound(responseDocument);
            }

            var currentUser = await this.userManager.GetUserAsync(this.User);
            responseDocument.Data = requestedUser.GetJsonApiResourceFor(currentUser);
            return this.Ok(responseDocument);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsersAsync()
        {
            var responseDocument = new JsonApiMultiResourceDocument();
            var allUsers = await this.userManager.Users.ToListAsync();
            var currentUser = await this.userManager.GetUserAsync(this.User);
            responseDocument.Data = allUsers.Select(x => x.GetJsonApiResourceFor(currentUser) as IJsonApiResourceIdentifier).ToList();
            return this.Ok(responseDocument);
        }

        [HttpGet("{id}/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyUserEmailAsync(string id, string token)
        {
            var responseDocument = new JsonApiDocument();
            var requestedUser = await this.userManager.FindByIdAsync(id);
            if (requestedUser is null)
            {
                return this.NotFound(responseDocument);
            }

            var result = await this.userManager.ConfirmEmailAsync(requestedUser, token);
            if (result.Succeeded)
            {
                return this.Ok(responseDocument);
            }

            return this.BadRequest(responseDocument);
        }
    }
}
