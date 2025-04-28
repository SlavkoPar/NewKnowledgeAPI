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
        public QuestionKey? QuestionKey { get; set; }
        public AnswerKey AnswerKey { get; set; }
        public WhoWhenDto Created { get; set; }

        public string? AnswerTitle { get; set; }

        public AssignedAnswerDto()
        {
        }

        public AssignedAnswerDto(AssignedAnswer assignedAnswer)
        {
            // Explicitly access properties instead of deconstruction
            AnswerKey = assignedAnswer.AnswerKey;
            Created = new WhoWhenDto(assignedAnswer.Created);
            AnswerTitle = assignedAnswer.AnswerTitle;
        }

        public AssignedAnswerDto(QuestionKey questionKey, AssignedAnswer assignedAnswer)
        {
            QuestionKey = questionKey;
            AnswerKey = assignedAnswer.AnswerKey;
            Created = new WhoWhenDto(assignedAnswer.Created);
        }

        internal void Deconstruct(out QuestionKey? questionKey, out AnswerKey answerKey, out WhoWhenDto created, out string? answerTitle)
        {
            questionKey = QuestionKey;
            answerKey = AnswerKey;
            created = Created;
            answerTitle = AnswerTitle;
        }
    }

    
}
