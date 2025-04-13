using NewKnowledgeAPI.Model.Questions;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NewKnowledgeAPI.Model.Common
{

    public class WhoWhenDto
    {
        public WhoWhenDto() 
        {
        } 

        public WhoWhenDto(string NickName)
        {
            this.Time = DateTime.Now;
            this.NickName = NickName;
        }

        public WhoWhenDto(WhoWhen? whoWhen)
        {
            if (whoWhen == null)
            {
                this.Time = DateTime.Now;
                this.NickName = "NN";
            }
            else
            {
                Time = whoWhen.Time;
                NickName = whoWhen.NickName;
            }
        }

        public DateTime Time { get; set; }
        public string NickName { get; set; }
    }
}

