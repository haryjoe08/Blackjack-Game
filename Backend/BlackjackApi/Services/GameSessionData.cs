namespace BlackjackApi.Services;
using BlackjackApi.Models;

public class GameSessionData
{
    public string GameId { get; set; } = Guid.NewGuid().ToString();
    public Player Player { get; set; }
    public Table Table { get; set; }
    public Deck Deck { get; set; }
    public Dealer Dealer { get; set; }
    public string LastMessage { get; set; } = "Game baru dimulai.";
    public int ActiveHandIndex { get; set; } = 0;

    public GameSessionData(Player player, Table table, Deck deck, Dealer dealer)
    {
        Player = player;
        Table = table;
        Deck = deck;
        Dealer = dealer;
    }
}