using BlackJack.Models;
using BlackJack.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlackJack.Controllers
{
	[Route("blackjack")]
	[ApiController]
	public class DeckController(DeckService service) : ControllerBase
	{
		private static GameStatus? game = null;

		[HttpGet]
		public IActionResult GetGame()
		{
			if (game is null || game.GameOver)
				return NotFound("No game in play");

			return Ok(game);
		}

		[HttpPost]
		public async Task<IActionResult> NewGame(int count = 6)
		{
			if (game is not null && !game.GameOver)
				return StatusCode(409, "Game in progress");

			var response = await service.NewDeck(count);
			if (!response.IsOK)
				return response.AsStatus(500);

			var deck = response.Value;
			game = new(deck.deck_id);

			response = await service.Draw(game.DeckId, 4);
			if (!response.IsOK)
				return response.AsStatus(500);

			var drawn = response.Value.cards;

			response = await service.AddToPile(game.DeckId, "player", drawn[..2]);
			if (!response.IsOK)
				return response.AsStatus(500);
			game.PlayerCards.AddRange(drawn[..2]);

			response = await service.AddToPile(game.DeckId, "dealer", drawn[2..]);
			if (!response.IsOK)
				return response.AsStatus(500);
			game.DealerCards.AddRange(drawn[2..]);

			return Created("blackjack", game);
		}

		[HttpPost("Play")]
		public async Task<IActionResult> MakePlay(string? action = null)
		{
			if (action is null)
				return BadRequest("Action not specified");

			if (game is null)
				return NotFound("No game in play");

			if (game.GameOver)
				return StatusCode(409, "Game is over");

			string? pile = action.Trim().ToLowerInvariant() switch
			{
				"hit" => "player",
				"stand" => "dealer",
				_ => null
			};
			if (pile is null)
				return BadRequest($"'{action}' is not a supported action");

			var response = await service.Draw(game.DeckId, 1);
			if (!response.IsOK)
				return response.AsStatus(500);

			var drawn = response.Value.cards;

			if (pile is "player")
				game.PlayerCards.AddRange(drawn);
			else if (pile is "dealer")
				game.DealerCards.AddRange(drawn);

			response = await service.AddToPile(game.DeckId, pile, drawn);
			if (!response.IsOK)
				return response.AsStatus(500);

			game.UpdateStatus(pile is "dealer");
			return Ok(game);
		}
	}
}
