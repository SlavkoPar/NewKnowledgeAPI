namespace NewKnowledgeAPI.Common
{
    public class Record
    {
        public WhoWhen? Created { get; set; }
        public WhoWhen? Modified { get; set; }
        public WhoWhen? Archived { get; set; }

        public Record(WhoWhen? Created, WhoWhen? Modified, WhoWhen? Archived) {
            this.Created = Created;
            this.Modified = Modified;
            this.Archived = Archived;
        }

        public Record(WhoWhenDto Created, WhoWhenDto Modified, WhoWhenDto Archived)
        {
            this.Created = Created != null ? new WhoWhen(Created) : null;
            this.Modified = Modified != null ? new WhoWhen(Modified) : null;
            this.Archived = Archived != null ? new WhoWhen(Archived) : null;
        }

        public Record()
        {
        }
    }
}
