using MongoDB.Bson.Serialization.Attributes;
using NaimeiKnowledge.Areas.Api.Models.JsonApi.User;
using ZetaLib.AspNetCore.Identity.MongoDB;
using ZetaLib.JsonApi;

namespace NaimeiKnowledge.Models
{
    public class ApplicationUser : MongoIdentityUser
    {
        [BsonElement("displayName", Order = 4)]
        public string DisplayName { get; set; }

        public IJsonApiResource GetJsonApiResourceFor(ApplicationUser user = null)
        {
            var resource = new JsonApiUserResource
            {
                Attributes = new JsonApiUserAttributes
                {
                    DisplayName = this.DisplayName,
                },
                Id = this.Id.ToString(),
                Links = JsonApiUserResource.CreateLinks(this.Id),
            };
            if (!(user is null))
            {
                if (this.Id == user.Id)
                {
                    resource.Attributes.Email = this.Email.Value;
                    resource.Attributes.UserName = this.UserName;
                }
            }

            return resource;
        }
    }
}
