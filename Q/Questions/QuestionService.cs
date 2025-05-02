using System.Net;
using Knowledge.Services;
using Microsoft.Azure.Cosmos;
using NewKnowledgeAPI.A.Answers;
using NewKnowledgeAPI.Common;
using NewKnowledgeAPI.Q.Categories.Model;
using NewKnowledgeAPI.Q.Categories;
using NewKnowledgeAPI.Q.Questions.Model;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.HttpResults;


namespace NewKnowledgeAPI.Q.Questions
{
    public class QuestionService : IDisposable
    {
        public DbService? Db { get; set; } = null;

        private readonly string containerId = "Questions";
        private Container? _container = null;

        public async Task<Container> container()
        {
            _container ??= await Db!.GetContainer(containerId);
            return _container;
        }


        public string? PartitionKey { get; set; } = null;
        public QuestionService()
        {
        }

        public QuestionService(DbService Db)
        {
            this.Db = Db;
        }
                 
        public async Task<HttpStatusCode> CheckDuplicate(string? Title, string? Id = null)
        {

            var sqlQuery = Title != null
                ? $"SELECT * FROM c WHERE c.Type = 'question' AND c.Title = '{Title}' AND IS_NULL(c.Archived)"
                : $"SELECT * FROM c WHERE c.Type = 'question' AND c.Id = '{Id}' AND IS_NULL(c.Archived)";
            QueryDefinition queryDefinition = new(sqlQuery);
            FeedIterator<Question> queryResultSetIterator =
                _container!.GetItemQueryIterator<Question>(queryDefinition);
            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                if (currentResultSet.Count == 0)
                {
                    throw new CosmosException("Question Title already exists", HttpStatusCode.NotFound, 0, "0", 0);
                }
            }
            return HttpStatusCode.Found;
        }

        public async Task<QuestionEx?> AddQuestion(QuestionData questionData)
        {
            var myContainer = await container();
            //Console.WriteLine(JsonConvert.SerializeObject(questionData));
            string msg = string.Empty;
            try
            {
                Question question = new(questionData);
                Console.WriteLine("----->>>>> " + JsonConvert.SerializeObject(question));
                // Read the item to see if it exists.  
                await CheckDuplicate(questionData.Title);
                msg = $":::::: Item in database with Title: {questionData.Title} already exists";
                Console.WriteLine(msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Question q = new(questionData);
                QuestionEx questionEx = await AddNewQuestion(q);
                return questionEx;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                msg = ex.Message;
                Console.WriteLine(msg);
            }
            return new QuestionEx(null, msg);
        }


        public async Task<QuestionEx> AddNewQuestion(Question question)
        {
            var (PartitionKey, Id, Title, ParentCategory, Kind, Level, Variations, Questions) = question;
            var myContainer = await container();
            string msg = string.Empty;
            try
            {
                // Check if the id already exists
                ItemResponse<Question> aResponse =
                    await myContainer!.ReadItemAsync<Question>(
                        Id,
                        new PartitionKey(PartitionKey)
                    );
                msg = $"Question in database with id: {Id} already exists\n";
                Console.WriteLine(msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                try
                {
                    // Check if the title already exists
                    HttpStatusCode statusCode = await CheckDuplicate(Title);
                    msg = $"Question in database with Title: {Title} already exists";
                    Console.WriteLine(msg);
                }
                catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    ItemResponse<Question> aResponse =
                    await myContainer!.CreateItemAsync(
                            question,
                            new PartitionKey(PartitionKey)
                        );
                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
                    return new QuestionEx(aResponse.Resource, "");
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                msg = ex.Message;
            }
            return new QuestionEx(null, msg);
        }


        public async Task<QuestionEx> CreateQuestion(QuestionDto questionDto)
        {
            var myContainer = await container();
            try
            {
                Question q = new(questionDto);
                QuestionEx questionEx = await AddNewQuestion(q);
                return questionEx;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                return new QuestionEx(null, ex.Message);
            }
        }

        public async Task<QuestionEx> GetQuestion(QuestionKey questionKey)
        {
            var myContainer = await container();
            Question? question = null;
            string msg = string.Empty;
            try
            {
                var (PartitionKey, Id) = questionKey;
                Console.WriteLine($"*****************************  {PartitionKey}/{Id}");
                // Read the item to see if it exists.  
                question = await myContainer.ReadItemAsync<Question>(
                    Id,
                    new PartitionKey(PartitionKey)
                );
                return new QuestionEx(question, msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                msg = "NotFound";
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                msg = ex.Message;
                Console.WriteLine(msg);
            }
            Console.WriteLine(JsonConvert.SerializeObject(question));
            Console.WriteLine("*****************************");
            return new QuestionEx(null, msg);
        }
       

        public async Task<QuestionEx> UpdateQuestion(Question q, List<AssignedAnswer>? assignedAnswers = null)
        {
            var (PartitionKey, Id, Title, ParentCategory, Type, Source, Status, _) = q;
            Console.WriteLine("========================UpdateQuestion-1");
            Console.WriteLine(JsonConvert.SerializeObject(q));
            Console.WriteLine("========================UpdateQuestion-2");
            Console.WriteLine(JsonConvert.SerializeObject(assignedAnswers));
            Console.WriteLine("========================UpdateQuestion-3");
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Question> aResponse =
                    await myContainer!.ReadItemAsync<Question>(
                        Id,
                        new PartitionKey(PartitionKey)
                    );
                Question question = aResponse.Resource;
                var doUpdate = true;
                if (!Title.Equals(question.Title, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        HttpStatusCode statusCode = await CheckDuplicate(Title);
                        doUpdate = false;
                        var msg = $"Question with Title: \"{Title}\" already exists in database.";
                        Console.WriteLine(msg);
                        return new QuestionEx(null, msg);
                    }
                    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        question.Title = question.Title;
                    }
                }
                if (doUpdate)
                {
                    if (assignedAnswers != null)
                    {
                        question.AssignedAnswers = assignedAnswers;
                        question.NumOfAssignedAnswers = assignedAnswers.Count;
                    }
                    question.Source = Source;
                    question.Status = Status;
                    question.Modified = q.Modified!;
                    aResponse = await myContainer.ReplaceItemAsync(question, Id, new PartitionKey(PartitionKey));
                    return new QuestionEx(aResponse.Resource, "");
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var msg = $"Question Id: \"{Id}\" Not Found in database.";
                Console.WriteLine(msg); 
                return new QuestionEx(null, msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return new QuestionEx(null, "Server Problem Update");
        }

        public async Task<QuestionEx> DeleteQuestion(QuestionDto questionDto)
        {
            var myContainer = await container();
            var duplicateTitle = false;
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Question> aResponse =
                    await myContainer!.ReadItemAsync<Question>(
                        questionDto.Id,
                        new PartitionKey(questionDto.PartitionKey)
                    );
                Question question = aResponse.Resource;

                //duplicateTitle = true;
                //if (!question.Title.Equals(questionDto.Title, StringComparison.OrdinalIgnoreCase))
                //{
                //    HttpStatusCode statusCode = await CheckDuplicate(questionDto.Title);
                //}
                // TODO check if is it already Archived
                question.Archived = new WhoWhen(questionDto.Archived!.NickName);

                aResponse = await myContainer.ReplaceItemAsync(question, question.Id, new PartitionKey(question.PartitionKey));
                Console.WriteLine("Updated Question [{0},{1}].\n \tBody is now: {2}\n", question.Title, question.Id, question);
                return new QuestionEx(aResponse.Resource, "");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var msg = $"Question item {questionDto.Id} NotFound in database.";
                if (duplicateTitle)
                {
                    msg = $"Question Title: {questionDto.Title} aleready exists in database.";
                }
                Console.WriteLine(msg); //, aResponse.RequestCharge);
                return new QuestionEx(null, msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return new QuestionEx(null, "Server Problem Delete");
        }

        public async Task<QuestionsMore> GetQuestions(string parentCategory, int startCursor, int pageSize, string includeQuestionId)
        {
            var myContainer = await container();
            try
            {
                // OR c.ParentCategory = ''
                string sqlQuery = $"SELECT * FROM c WHERE c.Type = 'question' AND IS_NULL(c.Archived) AND " +
                    $" c.ParentCategory = '{parentCategory}' ORDER BY c.Title OFFSET {startCursor} ";
                sqlQuery += includeQuestionId == "null"
                    ? $"LIMIT {pageSize}"
                    : $"LIMIT 9999";

                //Console.WriteLine("************ sqlQuery{0}", sqlQuery);

                int n = 0;
                bool included = false;

                List<Question> questions = [];
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Question> queryResultSetIterator = myContainer!.GetItemQueryIterator<Question>(queryDefinition);
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Question question in currentResultSet)
                    {
                        if (includeQuestionId != null && question.Id == includeQuestionId)
                        {
                            included = true;
                        }
                        //Console.WriteLine(">>>>>>>> question is: {0}", JsonConvert.SerializeObject(question));
                        questions.Add(question);
                        n++;
                        if (n >= pageSize && (includeQuestionId == null || included))
                        {
                            return new QuestionsMore(questions, true);
                        }
                    }
                    return new QuestionsMore(questions, false);
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return new QuestionsMore([], false);
        }

        public async Task<List<QuestDto>> GetQuests(List<string> words, int count)
        {
            var myContainer = await container();
            try
            {
                var sqlQuery = $"SELECT c.ParentCategory, c.Title, c.id FROM c WHERE c.Type = 'question' AND IS_NULL(c.Archived) AND ";
                if (words.Count == 1)
                {
                    sqlQuery += $" CONTAINS(c.Title, \"{words[0]}\", true) ";
                }
                else
                {
                    sqlQuery += "(";
                    for (var i=0; i < words.Count; i++)
                    {
                        if (i > 0)
                            sqlQuery += " OR ";
                        sqlQuery += $" CONTAINS(c.Title, \"{words[i]}\", true) ";
                    }
                    sqlQuery += ")";

                }
                sqlQuery += $" ORDER BY c.Title OFFSET 0 LIMIT {count}";


                List<QuestDto> quests = [];
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                using (FeedIterator<Question> queryResultSetIterator = 
                    myContainer!.GetItemQueryIterator<Question>(queryDefinition))
                {
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (Question question in currentResultSet)
                        {
                            quests.Add(new QuestDto(question));
                        }
                    }
                }
                return quests;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return [];
        }
           

        public async Task<QuestionEx> AssignAnswer(AssignedAnswerDto assignedAnswerDto)
        {
            var (questionKey, answerKey, answerTitle, created, modified) /*, Fixed, NotFixed, NotClicked)*/ = assignedAnswerDto;
            QuestionEx questionEx = await GetQuestion(questionKey!);
            var (question, msg) = questionEx;
            if (question != null)
            {
                var assignedAnswers = question.AssignedAnswers ?? new List<AssignedAnswer>();
                assignedAnswers.Add(new AssignedAnswer(assignedAnswerDto));
                question.Modified = new WhoWhen(created);
                questionEx = await UpdateQuestion(question, assignedAnswers);
            }
            return questionEx;
        }

        public async Task<QuestionEx> UnAssignAnswer(AssignedAnswerDto assignedAnswerDto)
        {
            var (questionKey, answerKey, answerTitle, created, modified/*, Fixed, NotFixed, NotClicked*/) = assignedAnswerDto;

            QuestionEx questionEx = await GetQuestion(questionKey!);
            var (question, msg) = questionEx;
            if (question != null)
            {
                var assignedAnswers = question.AssignedAnswers.FindAll(a => a.AnswerKey.Id != answerKey.Id);
                question.Modified = new WhoWhen(created);
                questionEx = await UpdateQuestion(question, assignedAnswers);
            }
            return questionEx;
        }


        public async Task<Question> SetAnswerTitles(Question question, CategoryService categoryService, AnswerService answerService)
        {
            var (PartitionKey, Id, Title, ParentCategory, Type, Source, Status, AssignedAnswers) = question;
            CategoryKey categoryKey = new(PartitionKey, question.ParentCategory!);
            // get category Title
            CategoryEx categoryEx = await categoryService.GetCategory(categoryKey);
            var (category, message) = categoryEx;
            question.CategoryTitle = category != null ? category.Title : "NotFound Category";
            if (AssignedAnswers.Count > 0)
            {
                var answerIds = AssignedAnswers.Select(a => a.AnswerKey.Id).Distinct().ToList();
                Dictionary<string, string> answerTitles = await answerService.GetTitles(answerIds);
                Console.WriteLine(JsonConvert.SerializeObject(answerTitles));
                foreach (var assignedAnswer in AssignedAnswers)
                    assignedAnswer.AnswerTitle = answerTitles[assignedAnswer.AnswerKey.Id];
            }
            return question;
        }

        public async Task<Question> SetAnswerTitles(Question question, AnswerService answerService)
        {
            var (PartitionKey, Id, Title, ParentCategory, Type, Source, Status, AssignedAnswers) = question;
            if (AssignedAnswers.Count > 0)
            {
                var answerIds = AssignedAnswers.Select(a => a.AnswerKey.Id).Distinct().ToList();
                Dictionary<string, string> answerTitles = await answerService.GetTitles(answerIds);
                Console.WriteLine(JsonConvert.SerializeObject(answerTitles));
                foreach (var assignedAnswer in AssignedAnswers)
                    assignedAnswer.AnswerTitle = answerTitles[assignedAnswer.AnswerKey.Id];
            }
            return question;
        }


        public void Dispose()
        {
            _container = null;
            Db = null;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
