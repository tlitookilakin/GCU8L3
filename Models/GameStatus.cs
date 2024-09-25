using System.Text.Json.Serialization;

namespace BlackJack.Models
{
	public class GameStatus(string ID)
	{
		[JsonIgnore]
		public string DeckId { get; set; } = ID;
		public List<Card> DealerCards { get; set; } = [];
		public List<Card> PlayerCards { get; set; } = [];
		public bool GameOver { get; set; }
		public string? Outcome { get; set; }

		public void UpdateStatus(bool force)
		{
			int dealerScore = DealerCards.GetScore();
			int playerScore = PlayerCards.GetScore();
			GameOver = true;

			if (dealerScore > 21)
				Outcome = "Win";
			else if (playerScore > 21)
				Outcome = "Bust";
			else if (force && playerScore > dealerScore)
				Outcome = "Win";
			else if (force && dealerScore == playerScore)
				Outcome = "Standoff";
			else if (force && playerScore < dealerScore)
				Outcome = "Loss";
			else
				GameOver = false;
		}
	}
}
