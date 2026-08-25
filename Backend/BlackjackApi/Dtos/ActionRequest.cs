using BlackjackApi.Models;

namespace BlackjackApi.Dtos
{
    public record ActionRequest(ActionType Action, int HandIndex);
}
