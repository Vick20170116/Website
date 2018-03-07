using MongoDB.Bson;
using ZetaLib.AspNetCore.Forum.MongoDB;

namespace NaimeiKnowledge.Models
{
    public class ForumComment : MongoForumComment<ObjectId>
    {
    }
}
