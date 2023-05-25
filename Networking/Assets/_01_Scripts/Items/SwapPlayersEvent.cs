using MarcoHelpers;

public class SwapPlayersEvent : ItemEvent
{
    public override void Trigger(APlayer player)
    {
        EventSystem.RaiseEvent(EventName.PLAYERS_SWAP);
    }
}
