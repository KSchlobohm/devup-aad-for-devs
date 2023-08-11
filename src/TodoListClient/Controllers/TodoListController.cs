using Microsoft.AspNetCore.Mvc;
using TodoListClient.Models;
using TodoListClient.Services;

namespace TodoListClient.Controllers
{
    public class TodoListController : Controller
    {
        private ITodoListService _todoListService;

        public TodoListController(ITodoListService todoListService)
        {
            _todoListService = todoListService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _todoListService.GetTodoItems();

            return View(data);
        }

        // GET: TodoList/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var data = await _todoListService.GetTodoItem(id);
            return View(data);
        }

        // GET: TodoList/Create
        public ActionResult Create()
        {
            Todo todo = new Todo() { Owner = "Megatron" };
            return View(todo);
        }

        // POST: TodoList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Title,Owner")] Todo todo)
        {
            await _todoListService.CreateTodoItem(todo);
            return RedirectToAction("Index");
        }

        // GET: TodoList/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var data = await _todoListService.GetTodoItem(id);

            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // POST: TodoList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,Title,Owner")] Todo todo)
        {
            await _todoListService.UpdateTodoItem(todo);
            return RedirectToAction("Index");
        }

        // GET: TodoList/Delete/5
        public async Task<ActionResult> DeleteItem(int id)
        {
            var data = await _todoListService.GetTodoItem(id);

            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // POST: TodoList/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteItem(int id, [Bind("Id,Title,Owner")] Todo todo)
        {
            await _todoListService.DeleteTodoItem(id);
            return RedirectToAction("Index");
        }
    }
}
