using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Knowledge.Services;
using Microsoft.AspNetCore.Authorization;
using NewKnowledgeAPI.Model.Questions;
using NewKnowledgeAPI.Model.Categories;
using Microsoft.VisualBasic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Knowledge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class QuestionController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        DbService dbService { get; set; }

        public QuestionController(IConfiguration configuration)
        {
            dbService = new DbService(configuration);
            dbService.Initialize.Wait();
            Configuration = configuration;
        }


        [HttpGet("{parentCategory}/{startCursor}/{pageSize}/{includeQuestionId}")]
        public async Task<IActionResult> GetQuestions(string parentCategory, int startCursor, int pageSize, string? includeQuestionId)
        {
            try
            {
                var questionService = new QuestionService(dbService);

                QuestionsMore questionsMore = await questionService.GetQuestions(parentCategory, startCursor, pageSize, includeQuestionId);
                Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>> Count {0}", questionsMore.questions.Count);
                CategoryDto categoryDto = new(parentCategory, questionsMore);
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{partitionKey}/{id}")]
        public async Task<IActionResult> GetQuestion(string partitionKey, string id)
        {
            try
            {
                var questionService = new QuestionService(dbService);
                QuestionEx questionEx = await questionService.GetQuestion(partitionKey, id);
                if (questionEx.question == null)
                    return NotFound();
                return Ok(new QuestionDtoEx(questionEx));
            }
            catch (Exception ex)
            {
                return BadRequest(new QuestionDtoEx(ex.Message));
            }
        }

        [HttpGet("{filter}/{count}/{nesto}")]
        public async Task<IActionResult> GetQuests(string filter, int count, string nesto)
        {
            Console.WriteLine("GetQuests", filter, count, nesto);
            try
            {
                var words = filter //.ToLower()
                                .Replace("?", "")
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Where(w => w.Length > 2)
                                .ToList();
                var questionService = new QuestionService(dbService);
                List<QuestDto> quests = await questionService.GetQuests(words, count);
                return Ok(quests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] QuestionDto questionDto)
        {
            try
            {
                Console.WriteLine("*********=====>>>>>> CreateQuestion"); 
                Console.WriteLine(JsonConvert.SerializeObject(questionDto));
                
                var questionService = new QuestionService(dbService);
                
                QuestionEx questionEx = await questionService.CreateQuestion(questionDto);
                if (questionEx.question != null)
                {
                    return Ok(new QuestionDtoEx(questionEx));
                }
                Ok(questionEx);
                //return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return BadRequest("");
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody] QuestionDto questionDto)
        {
            try
            {
                Console.WriteLine("===>>> UpdateQuestion: {0} \n", questionDto.Title);
                var questionService = new QuestionService(dbService);

                QuestionEx questionEx = await questionService.UpdateQuestion(questionDto);
                if (questionEx!.question != null)
                    return Ok(new QuestionDtoEx(questionEx));
                return NotFound(questionEx);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] QuestionKey questionKey) //string PartitionKey, string id)
        {
            try
            {
                Console.WriteLine("===>>> DeleteQuestion: {0}/{1} \n", questionKey.PartitionKey, questionKey.Id);
                var questionService = new QuestionService(dbService);
                string result = await questionService.DeleteQuestion(questionKey);
                if (result == "OK")
                    return Ok(new { msg = result });
                else if (result == "NotFound")
                    return NotFound();
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
