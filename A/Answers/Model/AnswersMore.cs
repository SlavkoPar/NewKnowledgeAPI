using System.Collections.Generic;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NewKnowledgeAPI.A.Answers.Model
{
    public class AnswersMore
    {
        public List<Answer> answers { get; set; }
        public bool hasMoreAnswers { get; set; }
        public AnswersMore(List<Answer> answers, bool hasMore)
        {
            this.answers = answers;
            hasMoreAnswers = hasMore;
        }
    }
}

