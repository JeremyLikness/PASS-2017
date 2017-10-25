using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using System.Linq;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

#region TodoController
namespace TodoApi.Controllers
{
    [FormatFilter]
    [Route("api/[controller]")]
    public class TodoController : Controller
    {
        const string ETAG_HEADER = "ETag";
        const string MATCH_HEADER = "If-Match";

        const string SECRET_HEADER = "My-Secret";

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

        [Route("GetToken")]
        [HttpGet]
        public IActionResult GetToken()
        {
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, "jeremy@jeremylikness.com"),
        new Claim(JwtRegisteredClaimNames.Jti, System.Guid.NewGuid().ToString()),
    };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Startup.AES256KEY));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken("http://localhost:5001",
                "http://localhost:5000",
                claims,
                expires: System.DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            var tokenEncoded = new JwtSecurityTokenHandler().WriteToken(token);

            return new OkObjectResult(new { token = tokenEncoded });
        }

        #region snippet_GetAll
        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            if (!HttpContext.Request.Headers.ContainsKey(SECRET_HEADER) ||
                !HttpContext.Request.Headers[SECRET_HEADER].Equals("my-secret"))
            {
                return new StatusCodeResult(401);
            }
            return new ObjectResult(_context.TodoItems.AsEnumerable());
        }

        [HttpGet("{id}.{format?}", Name = "GetTodo")]
        public IActionResult GetById(long id,
            string version,
            [FromHeader(Name = "Accept")]string accept)
        {
            try
            {
                if (id == 8675309)
                {
                    throw new Exception("Bad digits.");
                }
                var item = _context.TodoItems.FirstOrDefault(t => t.Id == id);
                if (item == null)
                {
                    return NotFound();
                }
                if (accept.EndsWith("hateoas"))
                {
                    var link = new LinkHelper<TodoItem>(item);
                    link.Links.Add(new Link
                    {
                        Href = Url.Link("GetTodo", new { item.Id }),
                        Rel = "self",
                        method = "GET"
                    });
                    link.Links.Add(new Link
                    {
                        Href = Url.Link("PutTodo", new { item.Id }),
                        Rel = "put-todo",
                        method = "PUT"
                    });
                    link.Links.Add(new Link
                    {
                        Href = Url.Link("DeleteTodo", new { item.Id }),
                        Rel = "delete-todo",
                        method = "DELETE"
                    });
                    return new ObjectResult(link);
                }
                var eTag = HashFactory.GetHash(item);
                HttpContext.Response.Headers.Add(ETAG_HEADER, eTag);

                if (HttpContext.Request.Headers.ContainsKey(MATCH_HEADER) &&
                    HttpContext.Request.Headers[MATCH_HEADER].Contains(eTag))
                {
                    return new StatusCodeResult(304);
                }

                if (version == "v2.0")
                {
                    return new ObjectResult(
                        new {
                            item.Id,
                            Description = item.Name,
                            State = item.IsComplete ? "Completed" : "Pending"
                        }
                    );
                }

                return new ObjectResult(item);

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ExceptionHelper.ProcessError(ex));
            }
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
        [HttpPut("{id}", Name = "PutTodo")]
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

            var dbTag = HashFactory.GetHash(todo);
            if (!HttpContext.Request.Headers.ContainsKey(MATCH_HEADER) ||
                !HttpContext.Request.Headers[MATCH_HEADER].Contains(dbTag))
            {
                return new StatusCodeResult(412);
            }

            todo.IsComplete = item.IsComplete;
            todo.Name = item.Name;

            _context.TodoItems.Update(todo);
            _context.SaveChanges();
            return new NoContentResult();
        }
        #endregion

        #region snippet_Delete
        [HttpDelete("{id}", Name = "DeleteTodo")]
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

