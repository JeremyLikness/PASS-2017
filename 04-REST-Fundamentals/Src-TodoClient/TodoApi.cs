using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TodoClient 
{
    public class TodoApi
    {
        private const string ENDPOINT = "http://localhost:5000/api/todo/";

        public async Task<IEnumerable<TodoItem>> GetTodosAsync()
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync(ENDPOINT);
            return JsonConvert.DeserializeObject<IEnumerable<TodoItem>>(result);
        }

        public async Task<TodoItem> GetTodoAsync(int id)
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync($"{ENDPOINT}{id}");
            return JsonConvert.DeserializeObject<TodoItem>(result);
        }
    }

    public class TodoItem 
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsComplete { get; set; }
    }
}