using Newtonsoft.Json;
using System.Diagnostics.Metrics;


namespace NewKnowledgeAPI.A.Answers.Model
{
    public class AnswerKey
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }

        public AnswerKey()
        {
        }
    }

}
