using System.Collections.Generic;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NewKnowledgeAPI.Questions.Model
{
    public class QuestionsMore
    {
        public List<Question> questions { get; set; }
        public bool hasMoreQuestions { get; set; }
        public QuestionsMore(List<Question> questions, bool hasMore)
        {
            this.questions = questions;
            hasMoreQuestions = hasMore;
        }
    }
}

