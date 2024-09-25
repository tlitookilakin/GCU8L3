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

			if (!(await service.NewDeck(count)).TryGetResult(out Deck? deck, out IActionResult? error))
				return error;
			game = new(deck.deck_id);

			if (!(await service.Draw(game.DeckId, 4)).TryGetResult(out deck, out error))
				return error;
			var drawn = deck.cards;

			if (!(await service.AddToPile(game.DeckId, "player", drawn[..2])).TryGetResult(out _, out error))
				return error;
			game.PlayerCards.AddRange(drawn[..2]);

			if (!(await service.AddToPile(game.DeckId, "dealer", drawn[2..])).TryGetResult(out _, out error))
				return error;
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

			if (!(await service.Draw(game.DeckId, 1)).TryGetResult(out Deck? deck, out IActionResult? error))
				return error;
			var drawn = deck.cards;

			if (!(await service.AddToPile(game.DeckId, pile, drawn)).TryGetResult(out _, out error))
				return error;

			if (pile is "player")
				game.PlayerCards.AddRange(drawn);
			else if (pile is "dealer")
				game.DealerCards.AddRange(drawn);

			game.UpdateStatus(pile is "dealer");
			return Ok(game);
		}
	}
}
