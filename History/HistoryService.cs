using System.Net;
using Knowledge.Services;
using Microsoft.Azure.Cosmos;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using NewKnowledgeAPI.Hist.Model;

namespace NewKnowledgeAPI.Hist
{
    public class HistoryService : IDisposable
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
        public HistoryService()
        {
        }

        public HistoryService(DbService Db)
        {
            this.Db = Db;
        }
                 
        public async Task<HttpStatusCode> CheckDuplicate(string? Title, string? Id = null)
        {

            var sqlQuery = Title != null
                ? $"SELECT * FROM c WHERE c.Type = 'history' AND c.Title = '{Title}' AND IS_NULL(c.Archived)"
                : $"SELECT * FROM c WHERE c.Type = 'history' AND c.Id = '{Id}' AND IS_NULL(c.Archived)";
            QueryDefinition queryDefinition = new(sqlQuery);
            FeedIterator<History> queryResultSetIterator =
                _container!.GetItemQueryIterator<History>(queryDefinition);
            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<History> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                if (currentResultSet.Count == 0)
                {
                    throw new CosmosException("History Title already exists", HttpStatusCode.NotFound, 0, "0", 0);
                }
            }
            return HttpStatusCode.Found;
        }

        public async Task<HistoryEx?> AddHistory(HistoryData historyData)
        {
            var myContainer = await container();
            //Console.WriteLine(JsonConvert.SerializeObject(historyData));
            string msg = string.Empty;
            try
            {
                History history = new(historyData);
                Console.WriteLine("----->>>>> " + JsonConvert.SerializeObject(history));
                // Read the item to see if it exists.  
                //await CheckDuplicate(historyData.Title);
                //msg = $":::::: Item in database with Title: {historyData.Title} already exists";
                //Console.WriteLine(msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                History q = new(historyData);
                HistoryEx historyEx = await AddNewHistory(q);
                return historyEx;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                msg = ex.Message;
                Console.WriteLine(msg);
            }
            return new HistoryEx(null, msg);
        }


        public async Task<HistoryEx> AddNewHistory(History history)
        {
            var (PartitionKey, Id, QuestionId, AnswerId, Fixed, NickName ) = history;
            var myContainer = await container();
            string msg = string.Empty;
            try
            {
                // Check if the id already exists
                ItemResponse<History> aResponse =
                    await myContainer!.ReadItemAsync<History>(
                        Id,
                        new PartitionKey(PartitionKey)
                    );
                msg = $"History in database with id: {Id} already exists\n";
                Console.WriteLine(msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                    ItemResponse<History> aResponse =
                    await myContainer!.CreateItemAsync(
                            history,
                            new PartitionKey(PartitionKey)
                        );
                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
                    return new HistoryEx(aResponse.Resource, "");
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                msg = ex.Message;
            }
            return new HistoryEx(null, msg);
        }


        public async Task<HistoryEx> CreateHistory(HistoryDto historyDto)
        {
            var myContainer = await container();
            try
            {
                History history = new(historyDto);
                HistoryEx historyEx = await AddNewHistory(history);
                return historyEx;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                return new HistoryEx(null, ex.Message);
            }
        }

        public async Task<HistoryEx> GetHistory(string PartitionKey, string Id)
        {
            var myContainer = await container();
            History? history = null;
            string msg = string.Empty;
            try
            {
                Console.WriteLine($"*****************************  {PartitionKey}/{Id}");
                // Read the item to see if it exists.  
                history = await myContainer.ReadItemAsync<History>(
                    Id,
                    new PartitionKey(PartitionKey)
                );
                return new HistoryEx(history, msg);
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
            Console.WriteLine(JsonConvert.SerializeObject(history));
            Console.WriteLine("*****************************");
            return new HistoryEx(null, msg);
        }

        public async Task<HistoryListEx> GetHistories(string QuestionId)
        {
            List<History> histories = [];
            string msg = string.Empty;
            var myContainer = await container();
            try
            {
                // OR c.ParentCategory = ''
                string sqlQuery = $"SELECT * FROM c WHERE c.partitionKey = 'history' AND c.Type = 'history' AND " +
                    $" c.QuestionId = '{QuestionId}' OFFSET 0 LIMIT 999 "; // TODO
                //sqlQuery += includeHistoryId == "null"
                //    ? $"LIMIT {pageSize}"
                //    : $"LIMIT 9999";

                Console.WriteLine("************ sqlQuery{0}", sqlQuery);

                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<History> queryResultSetIterator = myContainer!.GetItemQueryIterator<History>(queryDefinition);
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<History> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (History history in currentResultSet)
                    {
                        Console.WriteLine(">>>>>>>> history is: {0}", JsonConvert.SerializeObject(history));
                        histories.Add(history);
                    }
                }
                return new HistoryListEx(histories, msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                msg = ex.Message;
            }
            return new HistoryListEx(histories, "");
        }
        /*

        public async Task<List<QuestDto>> GetQuests(List<string> words, int count)
        {
            var myContainer = await container();
            try
            {
                var sqlQuery = $"SELECT c.ParentCategory, c.Title, c.id FROM c WHERE c.Type = 'history' AND IS_NULL(c.Archived) AND ";
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
                using (FeedIterator<History> queryResultSetIterator = 
                    myContainer!.GetItemQueryIterator<History>(queryDefinition))
                {
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<History> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (History history in currentResultSet)
                        {
                            quests.Add(new QuestDto(history));
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
        */
           

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
