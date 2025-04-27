using NewKnowledgeAPI.A.Answers.Model;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;

namespace NewKnowledgeAPI.Q.Questions.Model
{
    public class AssignedAnswer: IDisposable
    {
        public AnswerKey AnswerKey { get; set; }
        public WhoWhen Created { get; set; }

        public string? AnswerTitle;

        public AssignedAnswer()
        {
        }

  
        public AssignedAnswer(AssignedAnswerDto dto)
        {
            Created = new WhoWhen(dto.Created);
            AnswerKey = dto.AnswerKey;
            AnswerTitle = null;
        }

        //public override string ToString() => 
        //    $"{PartitionKey}/{Id}, {Title} {ParentGroup} ";


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
