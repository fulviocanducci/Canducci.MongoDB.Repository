using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class PeopleController : ControllerBase
   {
      private readonly PeopleRepositoryAbstract _peopleRepository;

      public PeopleController(PeopleRepositoryAbstract peopleRepository)
      {
         _peopleRepository = peopleRepository;
      }

      [HttpGet]
      public async Task<IEnumerable<People>> Get()
      {
         return await _peopleRepository.AllAsync();
      }

      [HttpGet("{id}")]
      public async Task<ActionResult> Get(Guid id)
      {
         return Ok(await _peopleRepository.FindAsync(id));
      }

      // POST api/<PeopleController>
      [HttpPost]
      public async Task<ActionResult> Post([FromBody] People model)
      {
         if (ModelState.IsValid)
         {
            model = await _peopleRepository.AddAsync(model);
            return Created($"api/people/{model.Id}", model);
         }
         return BadRequest(model);
      }

      // PUT api/<PeopleController>/5
      [HttpPut("{id}")]
      public async Task<ActionResult> Put(Guid id, [FromBody] People model)
      {
         if (await _peopleRepository.EditAsync(id, model))
         {
            return Ok(model);
         }
         return BadRequest();
      }

      // DELETE api/<PeopleController>/5
      [HttpDelete("{id}")]
      public async Task<ActionResult> Delete(Guid id)
      {
         if (await _peopleRepository.DeleteAsync(id))
         {
            return Ok();
         }
         return NotFound();
      }
   }
}
