using System;

namespace Game.Systems.Bridges
{
    public interface ITrigger
    {
        event EventHandler<EventArgs> onTRReceived_TS;
    }
}