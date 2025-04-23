using Newtonsoft.Json;
using System.Diagnostics.Metrics;


namespace NewKnowledgeAPI.Q.Questions.Model
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
