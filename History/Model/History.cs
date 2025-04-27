using NewKnowledgeAPI.Common;
using NewKnowledgeAPI.Q.Questions.Model;
using Newtonsoft.Json;

namespace NewKnowledgeAPI.Hist.Model
{
    public class History : /*Record,*/ IDisposable
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        public string QuestionId { get; set; }
        public string AnswerId { get; set; }
        public short Fixed { get; set; }
        public string NickName { get; set; }


        public static DateTime centuryBegin = new DateTime(2025, 1, 1);
        public static string GeneratedId {  
            get
            {
                long elapsedTicks = DateTime.Now.Ticks - History.centuryBegin.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                return elapsedSpan.TotalSeconds.ToString();
            }
        }  

        public History()
        {
        }

  
        public History(HistoryData historyData)
        {
            Type = "history";
            PartitionKey = historyData.PartitionKey ?? "FindOut"; ;
            Id = History.GeneratedId;
            QuestionId = historyData.QuestionId;
            AnswerId = historyData.AnswerId;
            Fixed = historyData.Fixed;
            NickName = historyData.NickName ?? "Admin";
        }

        public History(HistoryDto historyDto)
        {
            Type = "history";
            PartitionKey = historyDto.PartitionKey ?? "FindOut";
            Id = History.GeneratedId;
            QuestionId = historyDto.QuestionId;
            AnswerId = historyDto.AnswerId;
            Fixed = historyDto.Fixed;
            NickName = historyDto.NickName ?? "Admin";
        }

        //public override string ToString() => 
        //    $"{PartitionKey}/{Id}, {Title} {ParentCategory} ";

        public void Deconstruct(out string partitionKey, out string id, out string questionId, out string answerId, out short fixedValue, out string nickName)
        {
            partitionKey = PartitionKey;
            id = Id;
            questionId = QuestionId;
            answerId = AnswerId;
            fixedValue = Fixed;
            nickName = NickName;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
