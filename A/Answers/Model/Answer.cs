using NewKnowledgeAPI.Common;
using Newtonsoft.Json;

namespace NewKnowledgeAPI.A.Answers.Model
{
    public class Answer : Record, IDisposable
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        public string? GroupTitle { get; set; }
        public string Title { get; set; }
        public string? ParentGroup { get; set; }
        public List<long> AssignedAnswers { get; set; }
        public int NumOfAssignedAnswers  {get; set;}
        public int Source { get; set; }
        public int Status { get; set; }

        public Answer()
            :  base()
        {
        }

  
        public Answer(AnswerData answerData)
            : base(new WhoWhen("Admin"), null, null)
        {
            string s = DateTime.Now.Ticks.ToString();
            Id = s.Substring(s.Length-10);// Guid.NewGuid().ToString();
            Type = "answer";
            PartitionKey = answerData.ParentGroup!;
            ParentGroup = answerData.ParentGroup;
            GroupTitle = null;
            Title = answerData.Title;
            AssignedAnswers = [];
            NumOfAssignedAnswers = 0;
            Source = 0;
            Status = 0;
        }

        public Answer(AnswerDto answerDto)
        : base(answerDto.Created, answerDto.Modified, answerDto.Archived)
        {
            string s = DateTime.Now.Ticks.ToString();
            Id = s.Substring(s.Length - 10);// Guid.NewGuid().ToString();
            Type = "answer";
            PartitionKey = answerDto.PartitionKey!;
            ParentGroup = answerDto.ParentGroup;
            GroupTitle = null;
            Title = answerDto.Title;
            AssignedAnswers = answerDto.AssignedAnswers!;
            NumOfAssignedAnswers = answerDto.NumOfAssignedAnswers;
            Source = answerDto.Source;
            Status = answerDto.Status;    
        }

        //public override string ToString() => 
        //    $"{PartitionKey}/{Id}, {Title} {ParentGroup} ";

        public void Deconstruct(out string partitionKey, out string id, out string title, out string? parentGroup,
                                out string type, out int source, out int status, out List<long> assignedAnswers)
        {
            partitionKey = PartitionKey;
            id = Id;
            title = Title;
            parentGroup = ParentGroup;
            type = Type;
            source = Source;
            status = Status;
            assignedAnswers = AssignedAnswers;
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
