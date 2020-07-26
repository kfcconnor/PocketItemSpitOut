using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled;

namespace PocketItemSpitOut
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using Server = Exiled.Events.Handlers.Server;
    using player = Exiled.Events.Handlers.Player;

    public class PocketItemSpitOut : Plugin<config>
    {

        private EventHandlers eHandler;

        public override void OnEnabled()
        {
            eHandler = new EventHandlers(this);
            Server.RoundStarted += eHandler.onRoundStart;
            Server.RoundEnded += eHandler.onRoundEnd;
            player.FailingEscapePocketDimension += eHandler.PocketDeath;
            Log.Info("PocketItemSpitOut V1.0.0 'Larry's Snack Machine' has loaded successfully");
        }

        public override void OnDisabled()
        {
            Server.RoundStarted -= eHandler.onRoundStart;
            Server.RoundEnded -= eHandler.onRoundEnd;
            player.FailingEscapePocketDimension -= eHandler.PocketDeath;
            eHandler = null;
        }

    }
}
