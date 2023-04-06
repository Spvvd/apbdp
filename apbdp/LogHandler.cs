using apbdp.Models;

namespace apbdp
{
    internal class LogHandler
    {
        private Discord discord;

        // Game state
        public Instance currentInstance;

        private bool joiningInstance = false;

        // Instance list
        private int instanceListState = -1;
        private int instanceListCount = 0;
        private int instanceListCounter;
        private List<Instance> instances = new List<Instance>();

        // Temps
        private Instance tmpInstance;
        private Instance tmpJoiningInstance;

        public LogHandler(Discord discord)
        {
            this.discord = discord;
        }

        public void Parse(string line)
        {
            if (line.Contains("Initializing Engine..."))
            {
                discord.SetDiscordPresence("Menus", "Starting");
            }
            else if (line.Contains("LS2GC_CHARACTER_LIST"))
            {
                Console.WriteLine("Character list received (LS2GC_CHARACTER_LIST)");
                discord.SetDiscordPresence("Menus", "Character Selection");
            }
            else if (line.Contains("WS2GC_ANS_INSTANCE_LIST") || instanceListState >= 0)
            {
                OnInstanceList(line);
            }
            else if (line.Contains("GC2WS_ASK_DISTRICT_RESERVE") || joiningInstance)
            {
                if (joiningInstance)
                {
                    HandleDistrictJoin(line);
                    return;
                }
                joiningInstance = true;
                Console.WriteLine("Asking to reserve instance slot (GC2WS_ASK_DISTRICT_RESERVE)");
            }
            else if (line.Contains("TriggerSceneOpenedPopupDialog"))
            {
                // Pass dialog class name in brackets (and remove whitespaces)
                HandleTriggerSceneOpenedPopupDialog(GetDialogNameFromLine(line));
            }
            else if (line.Contains("CloseSceneOpenedPopupDialog"))
            {
                // Pass dialog class name in brackets (and remove whitespaces)
                HandleCloseSceneOpenedPopupDialog(GetDialogNameFromLine(line));
            }
            else if (line.Contains("Got AFK Message :You are now AFK:"))
            {
                discord.DistrictPresenceUpdate(currentInstance, "AFK");
                Console.WriteLine("Player is AFK");
            }
            else if (line.Contains("Got AFK Message :You are no longer AFK"))
            {
                discord.DistrictPresenceUpdate(currentInstance);
                Console.WriteLine("Player is not AFK anymore");
            }
            else if (line.Contains("ClientStartMission()"))
            {
                discord.DistrictPresenceUpdate(currentInstance, "In Mission");
                Console.WriteLine("Mission started");
            }
            else if (line.Contains("ClientEndMission()"))
            {
                discord.DistrictPresenceUpdate(currentInstance);
                Console.WriteLine("Mission ended");
            }
        }

        private void OnInstanceList(string line)
        {
            // If WS2GC_ANS_INSTANCE_LIST event received and state is -1,
            // set state to 1 indicating an upcoming instance list
            if (instanceListState == -1)
            {
                // TODO: Enable if closing event found.
                //SetDiscordPresence("District Selection", "Idle");

                instances = new List<Instance>();
                instanceListCounter = 0;
                instanceListState = 1;
                Console.WriteLine("Instance list received (WS2GC_ANS_INSTANCE_LIST)");
            }
            // Return code of instance list call is 0, set state to 0 
            // -> confirmed response
            else if (line.Contains("m_nReturnCode"))
            {
                int returnCode = int.Parse(line.Split(new char[0]).Last());

                Console.WriteLine($"Instance list status returned {returnCode}");
                if (returnCode == 0)
                {
                    instanceListState = 0;
                } 
                else
                {
                    instanceListState = -1;
                }
            }
            // Get number of instances and set globaly
            else if (line.Contains("m_nInstances"))
            {
                instanceListCount = Int32.Parse(line.Split(new char[0]).Last());
                Console.WriteLine($"Number of instances: {instanceListCount.ToString()}");
            }
            // While 0 and counter smaller than instance count, 
            // handle upcoming instance information
            else if (instanceListState == 0)
            {
                if (instanceListCount > instanceListCounter)
                {
                    HandleInstance(line);
                }
                else
                {
                    // Debug logging each instance
                    foreach (Instance inst in instances)
                    {
                        Console.WriteLine(String.Format("{0} | {1}:{2} | E:{3} | C:{4}",
                            inst.Id, inst.MapId, inst.Map, inst.Enforcers, inst.Criminals));
                    }
                    instanceListState = -1;
                }
            }
            else
            {
                instanceListState = -1;
            }
        }

        private void HandleInstance(string line)
        {
            if (line.Contains($"m_nInstanceUID[{instanceListCounter}]"))
            {
                tmpInstance = new Instance();
                tmpInstance.Id = instanceListCounter;
                tmpInstance.ParseMapId(line);
                tmpInstance.ParseUId(line);
                tmpInstance.SetInstanceDetails();
                return;
            }
            else if (line.Contains($"m_nEnforcers[{tmpInstance.Id}]"))
            {
                tmpInstance.Enforcers = tmpInstance.ParseLastAsInteger(line);
                return;
            }
            else if (line.Contains($"m_nCriminals[{tmpInstance.Id}]"))
            {
                tmpInstance.Criminals = tmpInstance.ParseLastAsInteger(line);
                return;
            }
            else if (line.Contains($"m_nDistrictStatus[{tmpInstance.Id}]"))
            {
                tmpInstance.Status = tmpInstance.ParseLastAsInteger(line);
                return;
            }
            else if (line.Contains($"m_nQueueSize[{tmpInstance.Id}]"))
            {
                tmpInstance.QueueSize = tmpInstance.ParseLastAsInteger(line);
            }
            instances.Add(tmpInstance);
            instanceListCounter++;
        }

        private void HandleDistrictJoin(string line)
        {
            if (tmpJoiningInstance != null)
            {
                if (line.Contains("m_nInstanceUID"))
                {
                    tmpJoiningInstance.ParseMapId(line);
                    tmpJoiningInstance.ParseUId(line);

                    Console.WriteLine($"Got instance MapId/UId ({tmpJoiningInstance.MapId}/{tmpJoiningInstance.UId})");
                }
                else if (line.Contains("GC2WS_ASK_DISTRICT_ENTER"))
                {
                    Console.WriteLine("Asking to enter instance event (GC2WS_ASK_DISTRICT_ENTER)");
                }
                else if (line.Contains("WS2GC_ANS_DISTRICT_ENTER"))
                {
                    foreach (Instance inst in instances)
                    {
                        if (inst.MapId == tmpJoiningInstance.MapId &&
                            inst.UId == tmpJoiningInstance.UId)
                        {
                            currentInstance = inst;
                            discord.DistrictPresenceUpdate(inst);
                        }
                    }

                    joiningInstance = false;
                    tmpJoiningInstance = null;
                    Console.WriteLine("District enter successful event (WS2GC_ANS_DISTRICT_ENTER)");
                }
                else if (line.Contains("WS2GC_ANS_DISTRICT_EXIT"))
                {
                    Console.WriteLine("Exiting district event (WS2GC_ANS_DISTRICT_EXIT)");
                }
            }
            else if (line.Contains("m_nReturnCode"))
            {
                int returnCode = Int32.Parse(line.Split(new char[0]).Last());

                if (returnCode == 0)
                {
                    tmpJoiningInstance = new Instance();
                    Console.WriteLine("Created new joining instance. Return Code: 0");
                }
                else if (returnCode == 70006)
                {
                    joiningInstance = false;
                    Console.WriteLine("District enter failed. Return Code: 70006");
                }
            }
        }

        private string GetDialogNameFromLine(string line)
        {
            string dialogName = "";
            try
            {
                dialogName = line.Split('(', ')')[1].Replace(" ", string.Empty);
            }
            catch (System.IndexOutOfRangeException)
            {
                Console.WriteLine("No dialog name found in line: " + line);
            }
            return dialogName;
        }

        private void HandleTriggerSceneOpenedPopupDialog(string dialog)
        {
            Console.WriteLine($"Popup Dialog Opened: {dialog}");
            switch (dialog)
            {
                case "CharacterCustomisation_UI":
                    discord.DistrictPresenceUpdate(currentInstance, "Designing Character");
                    break;
                case "SymbolEditor_0001":
                    discord.DistrictPresenceUpdate(currentInstance, "Designing Symbols");
                    break;
                case "Wardrobe_Main":
                    discord.DistrictPresenceUpdate(currentInstance, "Designing Clothing", "cd_small", "Clothing Editor");
                    break;
                case "MarketPlace":
                    discord.DistrictPresenceUpdate(currentInstance, "Browsing Marketplace");
                    break;
                case "ThemeEditor":
                    discord.DistrictPresenceUpdate(currentInstance, "Composing Music", "ms_small", "Music Studio");
                    break;
                case "VehicleUI_Main":
                    discord.DistrictPresenceUpdate(currentInstance, "Designing Vehicle", "vd_small", "Vehicle Editor");
                    break;
                case "ViewMessage":
                    discord.DistrictPresenceUpdate(currentInstance, "Viewing Mails");
                    break;
            }
        }

        private void HandleCloseSceneOpenedPopupDialog(string dialog)
        {
            Console.WriteLine("Popup Dialog Closed: " + dialog);
            switch (dialog)
            {
                case "CharacterCustomisation_UI":
                case "SymbolEditor_0001":
                case "Wardrobe_Main":
                case "MarketPlace":
                case "ThemeEditor":
                case "VehicleUI_Main":
                case "ViewMessage":
                    discord.DistrictPresenceUpdate(currentInstance);
                    break;
            }
        }
    }
}
