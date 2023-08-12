using Microsoft.Identity.Abstractions;
using TodoListClient.Models;

namespace TodoListClient.Services
{
    public class TodoListDownstreamApi : ITodoListService
    {
        private IDownstreamApi _downstreamApi;

        public TodoListDownstreamApi(IDownstreamApi todoListService)
        {
            _downstreamApi = todoListService;
        }

        public Task CreateTodoItem(Todo todo)
        {
            return _downstreamApi.PostForUserAsync("TodoList", todo);
        }

        public Task DeleteTodoItem(int id)
        {
            return _downstreamApi.DeleteForUserAsync("TodoList", new Todo(),
                options => options.RelativePath = $"api/todolist/{id}");
        }

        public Task<Todo> GetTodoItem(int id)
        {
            return _downstreamApi.GetForUserAsync<Todo>(
                "TodoList",
                options => options.RelativePath = $"api/todolist/{id}")!;
        }

        public Task<IEnumerable<Todo>> GetTodoItems()
        {
            return _downstreamApi.GetForUserAsync<IEnumerable<Todo>>("TodoList")!;
        }

        public Task UpdateTodoItem(Todo data)
        {
            return _downstreamApi.CallApiForUserAsync<Todo, Todo>(
                "TodoList", data,
                options => { options.RelativePath = $"api/todolist/{data.Id}"; options.HttpMethod = HttpMethod.Patch; });
        }
    }
}
