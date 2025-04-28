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
            var (questionKey, answerKey, created, answerTitle) = dto; 
            Created = new WhoWhen(created);
            AnswerKey = answerKey;
            AnswerTitle = answerTitle;
        }

        //public override string ToString() => 
        //    $"{PartitionKey}/{Id}, {Title} {ParentGroup} ";


        internal void Deconstruct(out AnswerKey answerKey, out WhoWhen created, out string? answerTitle )
        {
            answerKey = AnswerKey;
            created = Created;
            answerTitle = AnswerTitle;
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
