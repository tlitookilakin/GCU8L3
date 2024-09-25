using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

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

		public bool TryGetResult([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out IActionResult? error)
		{
			value = Value;
			error = null;

			if (this.error is null)
				return true;

			error = new ObjectResult("Upstream error: " + error) { StatusCode = 500 };
			return false;
		}
	}
}
