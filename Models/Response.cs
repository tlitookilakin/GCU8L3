using Microsoft.AspNetCore.Mvc;

namespace BlackJack.Models
{
	public class Response<T> where T : class
	{
		public T Value { get; init; }
		public string Error => error!;
		private readonly string? error;

		public Response(T? value)
		{
			if (value is null)
				error = "No Value";

			Value = value!;
		}

		public Response(string code, string message)
		{
			error = $"{code}: {message}";
			Value = default!;
		}

		public bool IsOK => error is null;

		public IActionResult AsStatus(int code)
		{
			return new ObjectResult("Upstream error: " + error) { StatusCode = code };
		}
	}
}
