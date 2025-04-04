using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Knowledge.Model;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using System.ComponentModel.DataAnnotations;
using Knowledge.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using System.ComponentModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Knowledge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]

    public class CategoryController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        DbService dbService { get; set; }


        public CategoryController(IConfiguration configuration)
        {
            dbService = new DbService(configuration);
            dbService.Initialize.Wait();
            Configuration = configuration;
        }

        // GET api/<FamilyController>
        [HttpGet]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)] //, VaryByQueryKeys = new[] { "impactlevel", "pii" })]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                Console.WriteLine("GetAllCategories");
                //using (var db = new Db(this.Configuration))
                //{
                //    await db.Initialize;
                //var category = new Category(_Db);
                //List<Category> subCategories = await category.GetAllCategories();
                var categoryService = new CategoryService(dbService);
                List<Category> subCategories = await categoryService.GetAllCategories();
                if (subCategories != null)
                {
                    List<CategoryDto> list = [];
                    foreach (Category cat in subCategories)
                    {
                        list.Add(new CategoryDto(cat));
                    }
                    return Ok(list);
                }
                //}
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{partitionKey}/{parentCategory}")]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "parentCategory" })]
        public async Task<IActionResult> GetSubCategories(string partitionKey, string parentCategory)
        {
            try
            {
                Console.WriteLine("GetSubCategories", partitionKey, parentCategory); 
                //using (var db = new Db(this.Configuration))
                //{
                //    await db.Initialize;
                //var category = new Category(_Db);
                //List<Category> subCategories = await category.GetSubCategories(partitionKey, parentCategory);
                var categoryService = new CategoryService(dbService);
                List<Category> subCategories = await categoryService.GetSubCategories(partitionKey, parentCategory);
                if (subCategories != null)
                {
                    List<CategoryDto> list = [];
                    foreach (Category cat in subCategories)
                    {
                        list.Add(new CategoryDto(cat));
                    }
                    return Ok(list);
                }
                //}
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{partitionKey}/{id}/{pageSize}/{includeQuestionId}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "id", "pageSize", "includeQuestionId" })]
        public async Task<IActionResult> GetCategory(string partitionKey, string id, int pageSize, string includeQuestionId)
        {
            try
            {
                Console.WriteLine("GetCategory: {0}, {1}, {2}, {3} \n", partitionKey, id, pageSize, includeQuestionId);

                // TODO 1. ovo 2. what does  /partitionKey mean?
                //using(var db = new Db(this.Configuration))
                //{
                //    await db.Initialize;
                // TODO Question.Db = db;
                //var category = new Category(_Db);
                // var container = await Db.GetContainer(this.containerId);
                //Category cat = await category.GetCategory(
                //    partitionKey, id, true, pageSize, includeQuestionId=="null" ? null : includeQuestionId);
                var categoryService = new CategoryService(dbService);
                Category cat = await categoryService.GetCategory(
                       partitionKey, id, true, pageSize, includeQuestionId == "null" ? null : includeQuestionId);
                if (cat != null)
                {
                    return Ok(new CategoryDto(cat));
                }
                return NotFound();
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
