using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Q.Questions.Model
{
   

    public class QuestionDtoEx
    {
        //public QuestionDtoEx(QuestionDto? questionDto, string msg)
        //{
        //    this.questionDto = questionDto;
        //    this.msg = msg;
        //}
        public QuestionDtoEx(QuestionEx questionEx)
        {
            questionDto = questionEx.question != null ? new QuestionDto(questionEx.question!) : null;
            msg = questionEx.msg!;
        }

        public QuestionDtoEx(string msg)
        {
            questionDto = null;
            this.msg = msg;
        }



        public QuestionDto? questionDto { get; set; }
        public string msg { get; set; }
    }

}



