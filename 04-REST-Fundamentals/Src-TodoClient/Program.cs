using System;
using System.Threading.Tasks;

namespace TodoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            UseApi().Wait();
        }

        static async Task UseApi()
        {
            var client = new TodoApi();
            Console.WriteLine("Getting list of todo items...");
            var list = await client.GetTodosAsync();
            foreach(var item in list) 
            {
                Console.WriteLine($"ID: {item.Id} Name: {item.Name} IsComplete: {item.IsComplete}");
            }
        }
    }
}
