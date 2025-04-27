using Newtonsoft.Json;
using System.Diagnostics.Metrics;


namespace NewKnowledgeAPI.A.Answers.Model
{
    public class AnswerTitle
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public AnswerTitle(string Id, string Title)
        {
            this.Id = Id;
            this.Title = Title;
        }

        public AnswerTitle()
        {
        }

    }

}
