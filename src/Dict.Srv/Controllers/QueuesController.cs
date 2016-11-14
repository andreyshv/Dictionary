using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Models;

//TODO: next ->
// WebAPI tutorial https://docs.asp.net/en/latest/tutorials/first-web-api.html#id10
// MVC https://docs.asp.net/en/latest/tutorials/first-mvc-app/adding-controller.html

namespace WebDictionary.Controllers
{
    [Route("api/[controller]")]
    public class QueuesController : Controller
    {
        private readonly ICardRepository _cards;

        public QueuesController(ICardRepository cardRepository)
        {
            _cards = cardRepository;
        }

        // Return Collection's Learn Queue 
        [HttpGet("{collectionId}")]
        public async Task<IActionResult> GetAsync(int? collectionId)
        {
            if (collectionId == null)
            {
                return BadRequest();
            }

            var cards = await _cards.GetLearnQueueAsync(collectionId.Value);
            if (cards == null)
            {
                return NotFound();
            }

            return new ObjectResult(cards);
        }
    }
}
