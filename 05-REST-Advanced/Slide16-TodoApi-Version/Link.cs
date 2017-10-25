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
