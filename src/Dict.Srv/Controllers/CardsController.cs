using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;

//TODO: next ->
// WebAPI tutorial https://docs.asp.net/en/latest/tutorials/first-web-api.html#id10
// MVC https://docs.asp.net/en/latest/tutorials/first-mvc-app/adding-controller.html

namespace WebDictionary.Controllers
{
    [Route("api/[controller]")]
    public class CardsController : Controller
    {
        private readonly ICardRepository _repository;
        private int _defaultCount = 10;

        //private ILogger _logger;

        public CardsController(ICardRepository cardRepository) // , ILogger<CardsController> logger
        {
            _repository = cardRepository;
            //_logger = logger; 
        }

        [HttpGet("collection/{collectionId}")]
        public async Task<IActionResult> GetAsync(int? collectionId, int? first, int? count)
        {
            if (collectionId == null)
                return NotFound();

            var items = await _repository.GetListAsync(collectionId.Value, first??0, count??_defaultCount);

            return new ObjectResult(items);
        }

        [HttpGet("{id}", Name = "GetCard")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var card = await _repository.FindAsync(id);
            if (card == null)
                return NotFound();

            return new ObjectResult(card);
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] Card item)
        {
            if (item == null || !ModelState.IsValid)
                return BadRequest();

            var item2 = await _repository.AddAsync(item);
            if (item2 == null)
                return BadRequest();

            return CreatedAtRoute("GetCollection", new { id = item2.Id }, item2);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int? id, [FromBody] Card item)
        {
            if (item == null || !ModelState.IsValid || item.Id != id)
                return BadRequest();

            
            if (!await _repository.UpdateAsync(item))
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int? id)
        {
            if (id == null)
                return NotFound();

            await _repository.DeleteAsync(id.Value);

            return NoContent();
        }
    }
}
