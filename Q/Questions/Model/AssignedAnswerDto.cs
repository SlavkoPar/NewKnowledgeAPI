using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.A.Answers.Model;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Q.Questions.Model
{
    public class AssignedAnswerDto
    {
        public QuestionKey QuestionKey { get; set; }
        public AnswerKey AnswerKey { get; set; }
        public WhoWhenDto Created { get; set; }

        public string? AnswerTitle { get; set; } 

        public AssignedAnswerDto()
        {
        }

        public AssignedAnswerDto(QuestionKey questionKey, AssignedAnswer assignedAnswer)
        {
            //Console.WriteLine(JsonConvert.SerializeObject(answer));
            QuestionKey = questionKey;
            AnswerKey = assignedAnswer.AnswerKey;
            Created = new WhoWhenDto(assignedAnswer.Created);
        }

        internal void Deconstruct(out QuestionKey questionKey, out AnswerKey answerKey, out WhoWhenDto created)
        {
            questionKey = QuestionKey;
            answerKey = AnswerKey;
            created = Created;
        }
    }

    
}
