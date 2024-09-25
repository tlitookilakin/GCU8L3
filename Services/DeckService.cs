using BlackJack.Models;

namespace BlackJack.Services
{
	public class DeckService
	{
		private readonly HttpClient httpClient;

		public DeckService(HttpClient httpClient)
		{
			this.httpClient = httpClient;
			this.httpClient.BaseAddress = new("https://www.deckofcardsapi.com/api/deck/");
		}

		private async Task<Response<Deck>> Get(string path)
		{
			var response = await httpClient.GetAsync(path);
			if (response.IsSuccessStatusCode)
				return new(await response.Content.ReadFromJsonAsync<Deck>());
			return new(response.StatusCode.ToString(), await response.Content.ReadAsStringAsync());
		}

		public Task<Response<Deck>> NewDeck(int count)
			=> Get($"new/shuffle/?deck_count={count}");

		public Task<Response<Deck>> Draw(string id, int count)
			=> Get($"deck/{id}/draw/?count={count}");

		public Task<Response<Deck>> Shuffle(string id, bool remainingOnly)
			=> Get($"deck/{id}/shuffle/?remaining={remainingOnly}");

		public Task<Response<Deck>> AddToPile(string id, string pile, params Card[] cards)
			=> Get($"deck/{id}/pile/{pile}/add/?cards={string.Join(',', cards.Select(c => c.code))}");
	}
}
