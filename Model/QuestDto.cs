using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

namespace Knowledge.Model
{
    public class QuestDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ParentCategory { get; set; }

        public QuestDto()
        {
        }

        public QuestDto(Question question)
        {
            ParentCategory = question.ParentCategory!;
            Id = question.Id;
            Title = question.Title;
        }

    }
}



