# Steps

```csharp
using System.Collections.Generic;

namespace TodoApi
{
    public class Link
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string method { get; set; }
    }

    public class LinkHelper<T> where T: class 
    {
        public T Value { get; set; }
        public List<Link> Links { get; set;}

        public LinkHelper()
        {
            Links = new List<Link>();
        }

        public LinkHelper(T item) : base()
        {
            Value = item;
            Links = new List<Link>();
        }
    }
}
```

Add to GET:
`[FromHeader(Name="Accept")]string accept`

Add names to PUT, DELETE (PutTodo, DeleteTodo)

Extend GET:

```csharp
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
```
