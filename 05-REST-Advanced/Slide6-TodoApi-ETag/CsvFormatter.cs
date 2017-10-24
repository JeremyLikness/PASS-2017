using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using TodoApi.Models;

namespace TodoApi
{
    public class CsvFormatter : TextOutputFormatter
    {
        public CsvFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(System.Type type)
        {
            return type == typeof(TodoItem);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();
            if (context.Object is IEnumerable<TodoItem>)
            {
                foreach(var todoItem in (IEnumerable<TodoItem>)context.Object)
                {
                    FormatCsv(buffer, todoItem);
                }
            }
            else
            {
                FormatCsv(buffer, (TodoItem)context.Object);
            }
            using (var writer = context.WriterFactory(response.Body, selectedEncoding))
            {
                return writer.WriteAsync(buffer.ToString());
            }
        }

        private static void FormatCsv(StringBuilder buffer, TodoItem item)
        {
            buffer.AppendLine($"{item.Id},\"{item.Name}\",{item.IsComplete}");
        }
    }
}