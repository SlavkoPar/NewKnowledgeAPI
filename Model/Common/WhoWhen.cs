using NewKnowledgeAPI.Model.Questions;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NewKnowledgeAPI.Model.Common
{
   
    public class WhoWhen
    {
        public WhoWhen()
        {
        }

        public WhoWhen(string NickName)
        {
            this.Time = DateTime.Now;
            this.NickName = NickName;
        }

        public WhoWhen(WhoWhenDto whoWhenDto)
        {
            this.Time = whoWhenDto.Time;
            this.NickName = whoWhenDto.NickName;
        }

      
        public DateTime Time { get; set; }
        public string NickName { get; set; }
    }
 
}

