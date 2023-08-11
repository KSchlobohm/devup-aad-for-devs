using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using TodoListClient.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TodoListClient.Services
{
    public interface ITodoListService
    {
        Task CreateTodoItem(Todo todo);
        Task DeleteTodoItem(int id);
        Task<Todo> GetTodoItem(int id);
        Task<IEnumerable<Todo>> GetTodoItems();
        Task UpdateTodoItem(Todo id);
    }

    public class TodoListService : ITodoListService
    {
        IHttpClientFactory _clientFactory;
        string _baseAddress;

        public TodoListService(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _baseAddress = config.GetValue<string>("TodoList:BaseUrl");
        }

        public async Task<IEnumerable<Todo>> GetTodoItems()
        {
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(_baseAddress);
            var response = await client.GetAsync("api/TodoList");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var todoItems = JsonSerializer.Deserialize<IEnumerable<Todo>>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return todoItems;
            }

            return new List<Todo>();
        }

        public async Task<Todo> GetTodoItem(int id)
        {
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(_baseAddress);
            var response = await client.GetAsync($"api/TodoList/{id}");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var todoItem = JsonSerializer.Deserialize<Todo>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return todoItem;
            }

            return new Todo();
        }

        public async Task CreateTodoItem(Todo data)
        {
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(_baseAddress);

            var stringData = JsonSerializer.Serialize(data);
            var content = new StringContent(stringData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/TodoList", content);
            if (!response.IsSuccessStatusCode)
            {
                Trace.WriteLine(response.ReasonPhrase);
            }
        }

        public async Task UpdateTodoItem(Todo data)
        {
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(_baseAddress);

            var stringData = JsonSerializer.Serialize(data);
            var content = new StringContent(stringData, Encoding.UTF8, "application/json");

            var response = await client.PatchAsync($"api/TodoList/{data.Id}", content);
            if (!response.IsSuccessStatusCode)
            {
                Trace.WriteLine(response.ReasonPhrase);
            }
        }

        public async Task DeleteTodoItem(int id)
        {
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(_baseAddress);
            var response = await client.DeleteAsync($"api/TodoList/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Trace.WriteLine(response.ReasonPhrase);
            }
        }
    }
}
