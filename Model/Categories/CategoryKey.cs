using Newtonsoft.Json;
using System.Diagnostics.Metrics;


namespace NewKnowledgeAPI.Model.Categories
{
    public class CategoryKey
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }

        public CategoryKey()
        {
        }
    }

}
