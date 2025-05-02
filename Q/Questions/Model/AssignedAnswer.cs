using NewKnowledgeAPI.A.Answers.Model;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace NewKnowledgeAPI.Q.Questions.Model
{
    public class AssignedAnswer: IDisposable
    {
        public AnswerKey AnswerKey { get; set; }
        public WhoWhen Created { get; set; }
        public WhoWhen? Modified { get; set; }

        [JsonProperty(PropertyName = "AnswerTitle", NullValueHandling = NullValueHandling.Ignore)]
        public string? AnswerTitle;

        public uint Fixed { get; set; } // num of clicks to Fixed
        public uint NotFixed { get; set; } // num of clicks to NotFixed
        public uint NotClicked { get; set; } // num of not clicked

        public AssignedAnswer()
        {
        }

  
        public AssignedAnswer(AssignedAnswerDto dto)
        {
            var (_, answerKey, _, created, modified) = dto; //, Fixed, NotFixed, NotClicked) = dto; 
            AnswerKey = answerKey;
            AnswerTitle = null;
            Created = new WhoWhen(created);
            Modified = modified != null ? new WhoWhen(modified) : null;
            Fixed = 0;
            NotFixed = 0;
            NotClicked = 0;
        }

        //public override string ToString() => 
        //    $"{PartitionKey}/{Id}, {Title} {ParentGroup} ";


        internal void Deconstruct(out AnswerKey answerKey,  out string? answerTitle, 
            out WhoWhen created, out WhoWhen? modified,
            out uint Fixed, out uint NotFixed, out uint NotClicked)
        {
            answerKey = AnswerKey;
            answerTitle = AnswerTitle;
            created = Created;
            modified = Modified ?? null;
            Fixed = this.Fixed;
            NotFixed = this.NotFixed;
            NotClicked = this.NotClicked;
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
