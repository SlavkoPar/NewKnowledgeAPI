using NewKnowledgeAPI.Model.Categories;

namespace NewKnowledgeAPI.Model.Common
{
    public class RecordDto
    {
        public WhoWhenDto? Created { get; set; }
        public WhoWhenDto? Modified { get; set; }
        public WhoWhenDto? Archived { get; set; }

        public RecordDto()
        {
        }

        public RecordDto(WhoWhenDto? Created, WhoWhenDto? Modified, WhoWhenDto? Archived)
        {
            this.Created = Created;
            this.Modified = Modified;
            this.Archived = Archived;
        }


        public RecordDto(WhoWhen Created, WhoWhen Modified, WhoWhen Archived)
        {
            this.Created = new WhoWhenDto(Created);
            this.Modified = new WhoWhenDto(Modified);
            this.Archived = new WhoWhenDto(Archived);
        }


    }
}