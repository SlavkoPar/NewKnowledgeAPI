﻿using Microsoft.AspNetCore.Http.HttpResults;
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
        public WhoWhenDto? Modified { get; set; }

        public string? AnswerTitle { get; set; }
        //public uint Fixed { get; set; } // num of clicks to Fixed
        //public uint NotFixed { get; set; } // num of clicks to NotFixed
        //public uint NotClicked { get; set; } // num of not clicked

        public AssignedAnswerDto()
        {
        }

        //public AssignedAnswerDto(AssignedAnswer assignedAnswer)
        //{
        //    var (answerKey, created, answerTitle, Fixed, NotFixed, NotClicked) = assignedAnswer;
        //    AnswerKey = answerKey;
        //    Created = new WhoWhenDto(created);
        //    AnswerTitle = answerTitle;
        //    this.Fixed = Fixed;
        //    this.NotFixed = NotFixed;
        //    this.NotClicked = NotClicked;
        //}

        public AssignedAnswerDto(QuestionKey questionKey, AssignedAnswer assignedAnswer)
        {
            QuestionKey = questionKey;
            var (answerKey, answerTitle, created, modified, Fixed, NotFixed, NotClicked) = assignedAnswer;
            AnswerKey = answerKey;
            AnswerTitle = answerTitle;
            Created = new WhoWhenDto(created);
            Modified = new WhoWhenDto(modified);
        }

        internal void Deconstruct(out QuestionKey? questionKey, 
            out AnswerKey answerKey, out string? answerTitle, out WhoWhenDto created, out WhoWhenDto? modified)
            //out uint Fixed, out uint NotFixed, out uint NotClicked)
        {
            questionKey = QuestionKey;
            answerKey = AnswerKey;
            answerTitle = AnswerTitle;
            created = Created;
            modified = Modified;
            //Fixed = this.Fixed;
            //NotFixed= this.NotFixed;
            //NotClicked = this.NotClicked;
        }
    }

    
}
