using System.Diagnostics.Metrics;


namespace NewKnowledgeAPI.Hist.Model
{
    public class HistoryData
    {
        public string? PartitionKey { get; set; }
        public string? Id { get; set; }

        public string QuestionId { get; set; }
        public string AnswerId { get; set; }
        public short Fixed { get; set; }
        public string NickName { get; set; }

        public HistoryData() { 
        }

      
    }

}
