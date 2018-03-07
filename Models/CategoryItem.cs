using System.Collections.Generic;
using MongoDB.Bson;

namespace NaimeiKnowledge.Models
{
    public class CategoryItem
    {
        public IList<CategoryItem> Children { get; set; }

        public ObjectId TagId { get; set; }
    }
}
