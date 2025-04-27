using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Hist.Model
{
    public class HistoryDto //: RecordDto
    {
        public string? PartitionKey { get; set; }
        public string? Id { get; set; }
        public string QuestionId { get; set; }
        public string AnswerId { get; set; }
        public short Fixed { get; set; }
        public string NickName { get; set; }


        public HistoryDto()
            //: base()
        {
        }

        public HistoryDto(History history)
            //: base(history.Created, null, null)
        {
            //Console.WriteLine(JsonConvert.SerializeObject(history));
            PartitionKey = history.PartitionKey;
            Id = history.Id;
            QuestionId = history.QuestionId;
            AnswerId = history.AnswerId;
            Fixed = history.Fixed;
        }
    }
 }



