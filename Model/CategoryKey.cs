using Newtonsoft.Json;
using System.Diagnostics.Metrics;


namespace Knowledge.Model
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
