using Newtonsoft.Json;
using System.Diagnostics.Metrics;


namespace NewKnowledgeAPI.Categories.Model
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
