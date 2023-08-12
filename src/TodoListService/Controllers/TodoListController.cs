using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    /// <summary>
    /// This is an example of ToDo list controller that serves requests from client apps that sign-in users and as themselves (client credentials flow).
    /// </summary>
    /// aad4devs-todolistapi
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        // In-memory TodoList
        private static readonly Dictionary<int, Todo> TodoStore = new Dictionary<int, Todo>();

        public TodoListController()
        {
            // Pre-populate with sample data
            if (TodoStore.Count == 0)
            {
                TodoStore.Add(1, new Todo() { Id = 1, Owner = "Fred Flintstone", Title = "Pick up groceries" });
                TodoStore.Add(2, new Todo() { Id = 2, Owner = "Barney Rubble", Title = "Finish invoice report" });
            }
        }

        [HttpGet()]
        public IEnumerable<Todo> Get()
        {
            return TodoStore.Values;
        }

        [HttpGet("{id}", Name = "Get")]
        public Todo Get(int id)
        {
            return TodoStore.Values.FirstOrDefault(todo => todo.Id == id);
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            TodoStore.Remove(id);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Todo todo)
        {
            int nextid = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;

            todo.Id = nextid;
            TodoStore.Add(nextid, todo);
            return Created($"/todo/{nextid}", todo);
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] Todo todo)
        {
            Todo existingToDo = TodoStore.Values.FirstOrDefault(todo => todo.Id == id);

            if (id != todo.Id || existingToDo == null)
            {
                return NotFound();
            }

            TodoStore.Remove(id);
            TodoStore.Add(id, todo);
            return Ok(todo);
        }
    }
}
