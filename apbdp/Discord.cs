using apbdp.Models;

using DiscordRPC;
using DiscordRPC.Logging;

namespace apbdp
{
    internal class Discord
    {
        private DiscordRpcClient client;
        private string discordClientId = "465486956990693376";

        public RichPresence currentPresence;
        public RichPresence lastPresence;

        public Discord() 
        {
            
            client = new DiscordRpcClient(discordClientId);
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            client.OnReady += (sender, e) =>
            {
                Console.WriteLine($"Discord Presence is ready for user {e.User.Username}");
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Discord Presence updated");
            };

            client.Initialize();            
        }

        public void SetDiscordPresence(string details, string state, string largeImage = "apb_large", string imageText = "APB Reloaded")
        {
            // If current presence not null, set it to last presence
            if (currentPresence != null)
            {
                lastPresence = currentPresence;
            }

            // Create new current presence with given values
            currentPresence = new RichPresence()
            {
                Details = details,
                State = state,
                Assets = new Assets()
                {
                    LargeImageKey = largeImage,
                    LargeImageText = imageText
                }
            };

            // Update presence to client
            client.SetPresence(currentPresence);
        }

        public void DistrictPresenceUpdate(Instance inst, string state = null, string image = null, string imageText = null)
        {
            // Overrides if not null
            if (state == null)
                state = inst.stateText;
            if (image == null)
                image = inst.imageLarge;
            if (imageText == null)
                imageText = inst.Map;

            SetDiscordPresence(inst.Map, state, image, imageText);
        }
    }
}
