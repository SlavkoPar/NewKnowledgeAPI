using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Knowledge.Model;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Knowledge.Services;
using Microsoft.AspNetCore.Authorization;

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
                var categoryDto = new CategoryDto(questionsMore);
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{parentCategory}/{id}")]
        public async Task<IActionResult> GetQuestion(string parentCategory, string id)
        {
            try
            {

                var questionService = new QuestionService(dbService);
                Question q = await questionService.GetQuestion(parentCategory, id);
                if (q != null)
                {
                    return Ok(new QuestionDto(q));
                }
                    
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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


        // POST api/<FamilyController>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<FamilyController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<FamilyController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
