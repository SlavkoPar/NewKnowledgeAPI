using Microsoft.AspNetCore.Mvc;

using Knowledge.Services;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Newtonsoft.Json;
using NewKnowledgeAPI.Q.Categories.Model;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860



namespace NewKnowledgeAPI.Q.Categories
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class CategoryRowController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        DbService dbService { get; set; }

        public CategoryRowController(IConfiguration configuration)
        {
            dbService = new DbService(configuration);
            dbService.Initialize.Wait();
            Configuration = configuration;
        }


        [HttpGet]
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAllCategoryRows()
        {
            try
            {
                Console.WriteLine("GetAllCats");
                // using (var db = new Db(this.Configuration))
                var categoryRowService = new CategoryRowService(dbService);
                List<CategoryRowDto> categoryRowDtos = await categoryRowService.GetAllCategoryRows();
                return Ok(categoryRowDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{partitionKey}/{id}")]
        [ResponseCache(Duration = 12, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "id" })]
        public async Task<IActionResult> GetSubCategories(string partitionKey, string id)
        {
            try
            {
                Console.WriteLine("GetSubCategories {0}/{1}", partitionKey, id);
                //using (var db = new Db(this.Configuration))
                //{
                //    await db.Initialize;
                //var category = new Category(_Db);
                var categoryRowService = new CategoryRowService(dbService);
                List<CategoryRow> subCategories = await categoryRowService.GetSubCategoryRows(null, partitionKey, id);
                //Console.WriteLine(JsonConvert.SerializeObject(subCategories.Select( c => c.Title).ToList()));
                subCategories.Sort(CategoryRow.Comparer);
                //Console.WriteLine(JsonConvert.SerializeObject(subCategories.Select(c => c.Title).ToList()));
                if (subCategories != null)
                {
                    List<CategoryRowDto> list = [];
                    foreach (CategoryRow categoryRow in subCategories)
                    {
                        list.Add(new CategoryRowDto(categoryRow));
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


        [HttpGet("{partitionKey}/{id}/{upTheTree}")]
        [ResponseCache(Duration = 12, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "id", "upTheTre" })]
        public async Task<IActionResult> GetCategorRowsUpTheTree(string partitionKey, string id, bool upTheTree)
        {
            try
            {
                Console.WriteLine("GetCatsUpTheTree {0}/{1}", partitionKey, id);
                var categoryRowService = new CategoryRowService(dbService);
                var categoryKey = new CategoryKey(partitionKey, id);
                CategoryRowEx categoryRowEx = await categoryRowService.GetCategoryRowsUpTheTree(categoryKey);
                //Console.WriteLine(JsonConvert.SerializeObject(categoryEx));
                var categoryDtoEx = new CategoryRowDtoEx(categoryRowEx);
                return Ok(categoryDtoEx);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                Console.WriteLine(msg);
                return BadRequest(new CategoryRowDtoEx(null, msg));
            }
        }


 
    }
}
