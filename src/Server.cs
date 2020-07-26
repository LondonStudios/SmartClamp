using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Server
{
    public class Server : BaseScript
    {
        public Server()
        {
            EventHandlers["Server:ChangeCarState"] += new Action<int, bool>((netid, disable) =>
            {
                TriggerClientEvent("Client:ChangeCarState", netid, disable);
            });
        }
    }
}
