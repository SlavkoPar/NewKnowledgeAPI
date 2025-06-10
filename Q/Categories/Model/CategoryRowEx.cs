namespace NewKnowledgeAPI.Q.Categories.Model
{
 
    public class CategoryRowEx

    {
        public CategoryRowEx(CategoryRow? categoryRow, string msg)
        {
            this.categoryRow = categoryRow;
            this.msg = msg;
        }

        public void Deconstruct(out CategoryRow? categoryRow, out string msg)
        {
            categoryRow = this.categoryRow;
            msg = this.msg;
        }

        public CategoryRow? categoryRow { get; set; }
        public string msg { get; set; }    
    }
}
