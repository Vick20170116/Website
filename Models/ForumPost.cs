using MongoDB.Bson;
using ZetaLib.AspNetCore.Forum.MongoDB;

namespace NaimeiKnowledge.Models
{
    public class ForumPost : MongoForumPost<ObjectId, ForumComment>
    {
    }
}
