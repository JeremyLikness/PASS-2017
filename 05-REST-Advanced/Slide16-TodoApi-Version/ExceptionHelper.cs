using System;

namespace TodoApi
{
    public static class ExceptionHelper
    {
        public static object ProcessError(Exception ex)
        {
            return new {
                error = new {
                    code = ex.HResult,
                    message = ex.Message
                }
            };
        }
    }
}