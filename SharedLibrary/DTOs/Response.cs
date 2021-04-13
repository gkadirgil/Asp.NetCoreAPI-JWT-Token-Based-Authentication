using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedLibrary.DTOs
{
    public class Response<T> where T : class
    {
        public T Data { get; private set; }
        public int StatusCode { get; private set; }

        [JsonIgnore] //Data Serilaze edilirken bu data yok sayılacak.
        public bool IsSuccessful { get; set; }

        public ErrorDTO Error { get; set; }

        public static Response<T> Success(T data, int statusCode)
        {
            return new Response<T> { Data = data, StatusCode = statusCode, IsSuccessful = true };
        }

        public static Response<T> Success(int statusCode)
        {
            return new Response<T> { Data = default, StatusCode = statusCode, IsSuccessful = true };
        }

        public static Response<T> Fail(ErrorDTO errorDTO, int statusCode)
        {
            return new Response<T> { Error = errorDTO, StatusCode = statusCode, IsSuccessful = false };
        }

        public static Response<T> Fail(string errorMessage, int statusCode, bool isShow)
        {
            var errorDTO = new ErrorDTO(errorMessage, isShow);

            return new Response<T> { Error = errorDTO, StatusCode = statusCode, IsSuccessful = false };
        }
    }
}
