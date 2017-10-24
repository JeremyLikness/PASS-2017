using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using System.Linq;

#region TodoController
namespace TodoApi.Controllers
{
    [FormatFilter]
    [Route("api/[controller]")]
    public class TodoController : Controller
    {
        private readonly TodoContext _context;
#endregion

        public TodoController(TodoContext context)
        {
            _context = context;

            if (_context.TodoItems.Count() == 0)
            {
                _context.TodoItems.Add(new TodoItem { Name = "Item1" });
                _context.SaveChanges();
            }
        }

        #region snippet_GetAll
        [HttpGet]
        public IEnumerable<TodoItem> GetAll()
        {
            return _context.TodoItems.ToList();
        }

        [HttpGet("{id}.{format?}", Name = "GetTodo")]
        public IActionResult GetById(long id,
            [FromHeader(Name="Accept")]string accept)
        {
            var item = _context.TodoItems.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            if (accept.EndsWith("hateoas"))
            {
                var link = new LinkHelper<TodoItem>(item);
                link.Links.Add(new Link {
                    Href = Url.Link("GetTodo", new { item.Id }),
                    Rel = "self",
                    method = "GET"
                });
                link.Links.Add(new Link {
                    Href = Url.Link("PutTodo", new { item.Id }),
                    Rel = "put-todo",
                    method = "PUT"
                });
                link.Links.Add(new Link {
                    Href = Url.Link("DeleteTodo", new { item.Id }),
                    Rel = "delete-todo",
                    method = "DELETE"
                });
                return new ObjectResult(link);
            }
            return new ObjectResult(item);
        }
        #endregion
        #region snippet_Create
        [HttpPost]
        public IActionResult Create([FromBody] TodoItem item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            _context.TodoItems.Add(item);
            _context.SaveChanges();

            return CreatedAtRoute("GetTodo", new { id = item.Id }, item);
        }
        #endregion

        #region snippet_Update
        [HttpPut("{id}", Name="PutTodo")]
        public IActionResult Update(long id, [FromBody] TodoItem item)
        {
            if (item == null || item.Id != id)
            {
                return BadRequest();
            }

            var todo = _context.TodoItems.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            todo.IsComplete = item.IsComplete;
            todo.Name = item.Name;

            _context.TodoItems.Update(todo);
            _context.SaveChanges();
            return new NoContentResult();
        }
        #endregion

        #region snippet_Delete
        [HttpDelete("{id}", Name="DeleteTodo")]
        public IActionResult Delete(long id)
        {
            var todo = _context.TodoItems.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todo);
            _context.SaveChanges();
            return new NoContentResult();
        }
        #endregion
    }
}

