using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.A.Answers.Model;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Q.Questions.Model
{
    public class RelatedFilterDto
    {
        public QuestionKey QuestionKey { get; set; }
        public string Filter { get; set; }
        public WhoWhenDto Created { get; set; }
        public WhoWhenDto? Used { get; set; }  // we consider creation when Used is null 

        public RelatedFilterDto()
        {
        }

        internal void Deconstruct(out QuestionKey questionKey, out string filter,
           out WhoWhenDto created, out WhoWhenDto? used)
        {
            questionKey = QuestionKey;
            filter = Filter;
            created = Created;
            used = Used;
        }
    }
}
