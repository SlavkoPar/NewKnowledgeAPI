using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using System.ComponentModel.DataAnnotations;
using Knowledge.Services;
using Microsoft.AspNetCore.Authorization;
using NewKnowledgeAPI.Model.Categories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Knowledge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CatController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        DbService dbService { get; set; }

        public CatController(IConfiguration configuration)
        {
            dbService = new DbService(configuration);
            dbService.Initialize.Wait();
            Configuration = configuration;
        }

    
        [HttpGet("{partitionKey}/{id}")]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "id" })]
        public async Task<IActionResult> GetCatsUpTheTree(string partitionKey, string Id)
        {
            try
            {
                //using (var db = new Db(this.Configuration))
                //{
                    //await db.Initialize;
                    //var category = new Category(db); 
                    //Category cat = await category.GetCategory(partitionKey, id, false, 0, null);
                    var categoryService = new CategoryService(dbService);
                    Category cat = await categoryService.GetCategory(partitionKey, Id, false, 0, null);

                    if (cat != null)
                    {
                        List<CategoryDto> list = [];
                        list.Add(new CategoryDto(cat));
                        var parentCategory = cat.ParentCategory;
                        while (parentCategory != null)
                        {
                            Category c = await categoryService.GetCategory(partitionKey, parentCategory, false, 0, null);
                            if (c != null)
                            {
                                list.Add(new CategoryDto(c));
                                parentCategory = c.ParentCategory;
                            }
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


        [HttpGet("{partitionKey}/{id}/{hidrate}")]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "id" })]

        public async Task<IActionResult> GetCategoryHidrated(string partitionKey, string id, bool hidrate)
        {
            // hidrate collections except questions
            try
            {
                // TODO what does  /partitionKey mean?
                //using (var db = new Db(this.Configuration))
                //{
                    //await db.Initialize;
                    //var category = new Category(db);
                var categoryService = new CategoryService(dbService);
                Category cat = await categoryService.GetCategory(partitionKey, id, hidrate, 0, null);
                if (cat != null)
                {
                    return Ok(new CategoryDto(cat));
                }
                //}
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
