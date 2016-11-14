using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Models;

namespace WebDictionary.Controllers
{
    [Route("api/[controller]")]
    public class CollectionsController : Controller
    {
        private ICollectionRepository _repository;

        public CollectionsController(IAppRepository appRepository)
        {
            _repository = appRepository.GetCollectionRepository();
        }

        [HttpGet]
        public async Task<IEnumerable<Collection>> GetAll()
        {
            return await _repository.GetCollectionsAsync();
        }

        [HttpGet("{id}", Name = "GetCollection")]
        public async Task<IActionResult> GetById(int? id)
        {
            if (id == null)
                return NotFound();

            var item = await _repository.GetCollectionAsync(id.Value);
            if (item == null)
                return NotFound();

            return new ObjectResult(item);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Collection item)
        {
            if (item == null || !ModelState.IsValid)
                return BadRequest();

            item = await _repository.AddAsync(item);

            return CreatedAtRoute("GetCollection", new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int? id, [FromBody] Collection item)
        {
            if (item == null || !ModelState.IsValid || item.Id != id)
                return BadRequest();

            if (!await _repository.UpdateAsync(item))
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            await _repository.DeleteAsync(id.Value);

            return NoContent();
        }
    }
}
