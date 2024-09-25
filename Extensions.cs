using BlackJack.Models;

namespace BlackJack
{
	public static class Extensions
	{
		public static int GetScore(this IEnumerable<Card> cards)
		{
			int sum = 0;
			int aces = 0;

			foreach (var card in cards)
			{
				if (int.TryParse(card.value, out int rank))
					sum += rank;
				else if (card.value is "ACE")
					aces += 1;
				else
					sum += 10;

				if (sum + aces > 21)
					break;
			}

			int highAces = Math.Min(aces, (21 - sum) / 11);
			sum += highAces * 11 + (aces - highAces);
			if (sum > 21 && highAces is not 0)
				sum -= 10;
			return sum;
		}
	}
}
