using TodoApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace TodoApi
{
    public static class HashFactory
    {
        public static string GetHash(TodoItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            var itemText = $"{item.Id}|{item.IsComplete}|{item.Name}";
            using (var md5 = MD5.Create())
            {
                byte[] retVal = md5.ComputeHash(Encoding.Unicode.GetBytes(itemText));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}