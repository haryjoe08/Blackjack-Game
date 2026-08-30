using BlackjackApi.Models.Enums;

namespace BlackjackApi.Dtos
{
    public record ActionRequest(ActionType Action, int HandIndex);
}
