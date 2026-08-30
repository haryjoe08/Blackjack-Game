namespace BlackjackApi.Models;

public class GameActionResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static GameActionResult Success(string message) 
        => new GameActionResult { IsSuccess = true, Message = message };

    public static GameActionResult Failure(string message) 
        => new GameActionResult { IsSuccess = false, Message = message };
}