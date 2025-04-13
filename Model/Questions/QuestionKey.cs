using Newtonsoft.Json;
using System.Diagnostics.Metrics;


namespace NewKnowledgeAPI.Model.Questions
{
    public class QuestionKey
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }

        public QuestionKey()
        {
        }
    }

}
