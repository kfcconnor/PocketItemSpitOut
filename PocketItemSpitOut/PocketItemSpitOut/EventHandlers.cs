using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled;
using Exiled.Permissions.Extensions;
using MEC;

namespace PocketItemSpitOut
{
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Extensions;
    using Exiled.Events.EventArgs;
    using player = Exiled.Events.Handlers.Player;
    class EventHandlers
    {
        public CoroutineHandle coroutineHandle;
        public PocketItemSpitOut plugin;
        public List<ItemType> tempDropped;
        public List<ItemType> itemsDropped;
        config Config;
        bool firstRun = true;
        bool DconRun = false;
        bool tempBeingRead = false;
        bool LarryContained = false;
        int itemCount = 0;
        int sRoom = 0;
        public List<Room> ValidRooms = new List<Room>();

        public EventHandlers(PocketItemSpitOut plugin)
        {
            this.plugin = plugin;
            Config = plugin.Config;
        }

        public void onRoundStart()
        {
            coroutineHandle = Timing.RunCoroutine(ItemDrop());
            itemsDropped = new List<ItemType>();
            tempDropped = new List<ItemType>();
            firstRun = true;
            DconRun = false;
        }

        public void onRoundEnd(RoundEndedEventArgs ev)
        {
            Timing.KillCoroutines(coroutineHandle);
        }

        public void PocketDeath(FailingEscapePocketDimensionEventArgs ev)
        {
            var playerInventory = ev.Player.Inventory.items;
            if(playerInventory != null)
            {
                foreach (var item in playerInventory)
                {
                    tempBeingRead = true;
                    tempDropped.Add(item.id);
                    if (Config.debug)
                        Log.Debug("Item " + item.id + " has been added to Pocket List");
                }
                if (Config.debug)
                    Log.Debug("All Items Added.");
                tempBeingRead = false;
            }
            else
            {
                if (Config.debug)
                    Log.Debug("Inventory Empty, Skipping.");
            }
        }

        public void larryRecontained(ContainingEventArgs ev)
        {
            if(Config.allowDropOnLarryDeath)
                LarryContained = true;
        }

        public void OnCommand(SendingRemoteAdminCommandEventArgs ev)
        {
            Player sender = ev.Sender;

            switch (ev.Name)
            {
                case "count":
                    {
                        ev.IsAllowed = false;
                        if (!sender.CheckPermission("lsm.admin"))
                        {
                            ev.ReplyMessage = ("Permission denied");
                            return;
                        }
                        tempToList();
                        ev.ReplyMessage = ("Current count of dropped items: " + itemCount.ToString());
                        break;
                    }
                case "listitem":
                    {
                        ev.IsAllowed = false;
                        if (!sender.CheckPermission("lsm.admin"))
                        {
                            ev.ReplyMessage = ("Permission denied");
                            return;
                        }

                        tempToList();

                        int j = 0;
                        if (emptyCheck())
                        {
                            ev.ReplyMessage += "No Items Currently waiting to drop.";
                        }
                        ev.ReplyMessage = "Current Items Waiting to Drop: ";
                        foreach (ItemType i in itemsDropped)
                        {
                            ev.ReplyMessage += "\n" + j + ". " + i.ToString();
                            j++;
                        }
                        break;
                    }
                case "drop":
                    {
                        ev.IsAllowed = false;
                        if (!sender.CheckPermission("lsm.admin"))
                        {
                            ev.ReplyMessage = ("Permission denied");
                            return;
                        }

                        if (emptyCheck())
                        {
                            ev.ReplyMessage = "List is Empty, please wait for Larry to Murder.";
                            return;
                        }


                        int sItem;
                        try
                        {
                            sItem = Int32.Parse(ev.Arguments[0]);
                        }
                        catch (FormatException)
                        {
                            ev.ReplyMessage = "Error: Invalid input. Please enter the list number of the item you want to drop";
                            break;
                        }

                        if (sItem < 0 || sItem >= itemCount)
                        {
                            ev.ReplyMessage = "Error: Invalid Id.";
                        }
                        else
                        {
                            ev.ReplyMessage = "Dropping " + itemsDropped[sItem].ToString();
                            spitOutItem(sItem);

                        }

                        break;
                    }
                case "pinata":
                    {
                        ev.IsAllowed = false;
                        if (!sender.CheckPermission("lsm.admin"))
                        {
                            ev.ReplyMessage = ("Permission denied");
                            return;
                        }

                        
                        if (LarryContained) 
                        {
                            LarryContained = false;
                            ev.ReplyMessage = ("Pinata Mode Deactivated");
                        }
                        else
                        {
                            LarryContained = true;
                            ev.ReplyMessage = ("Pinata Mode Active");
                        }
                            
                        break;
                    }
            }
        }

        public void spitOutItem(int sItem)
        {
            sRoom = UnityEngine.Random.Range(0, ValidRooms.Count - 1);
            if (Config.debug)
                Log.Debug("Selected Room is " + ValidRooms[sRoom].Name);


            UnityEngine.Vector3 Spawnlocation = ValidRooms[sRoom].Position;
            Item.Spawn(itemsDropped[sItem], 100, Spawnlocation);
            if (Config.debug)
                Log.Debug("Item Spawned");
            itemsDropped.RemoveAt(sItem);
            itemCount--;
            
        }

        public void spitOutItems(int NumOfItems)
        {
            int sItem;
            for(int i = 0; i < NumOfItems; i++)
            {
                sItem = UnityEngine.Random.Range(0, itemsDropped.Count - 1);
                spitOutItem(sItem);
            }
        }

        public bool emptyCheck()
        {
            bool isEmpty = !itemsDropped.Any();
            return isEmpty;
        }

        public void tempToList()
        {
            if (!tempBeingRead)
            {
                foreach (ItemType item in tempDropped)
                {
                    itemsDropped.Add(item);
                }
                tempDropped.Clear();
                itemCount = itemsDropped.Count;
            }
        }

        public IEnumerator<float> ItemDrop()
        {
            yield return Timing.WaitForSeconds(UnityEngine.Random.Range(Config.minTimeForDrop, Config.maxTimeForDrop));
            int sItem;
            while (Round.IsStarted)
            {
                if (!Map.IsLCZDecontaminated)
                {
                    if (firstRun)
                    {
                        if (Config.debug)
                            Log.Debug("Gathering Rooms, Pre-Decontamiation");
                        foreach (Room room in Map.Rooms)
                        {
                            if (!room.Name.Contains("Tesla") || !Config.allowTesla)
                            {
                                ValidRooms.Add(room);
                            }
                            else
                            {
                                if (Config.debug)
                                    Log.Debug("Skipping Tesla Room");
                            }
                            if (Config.debug)
                                Log.Debug("Added Room " + room.Name + " To Valid Rooms");
                        }
                        firstRun = false;
                    }
                }
                else
                {
                    if (!DconRun)
                    {
                        ValidRooms.Clear();
                        if (Config.debug)
                            Log.Debug("Gathering Rooms, Post Decontamiation");
                        foreach (Room room in Map.Rooms)
                        {
                            if (room.Zone == ZoneType.LightContainment)
                                continue;
                            else if (!room.Name.Contains("Tesla") || !Config.allowTesla)
                            {
                                ValidRooms.Add(room);
                            }
                            else
                            {
                                if (Config.debug)
                                    Log.Debug("Skipping Tesla Room");
                            }
                            if (Config.debug)
                                Log.Debug("Added Room " + room.Name + " To Valid Rooms");
                        }
                        DconRun = true;
                    }
                }

                tempToList();

                if (emptyCheck())
                {
                    if (Config.debug)
                        Log.Debug("List Empty, Skipping");
                }
                else
                {
                    if (!LarryContained)
                    {
                        int randChance = UnityEngine.Random.Range(0, 100);
                        if (randChance <= Config.chanceForDrop)
                        {
                            sItem = UnityEngine.Random.Range(0, itemsDropped.Count - 1);
                            if (Config.debug)
                                Log.Debug("Selected Item is " + itemsDropped[sItem].ToString());
                            spitOutItem(sItem);
                        }
                        else
                        {
                            if (Config.debug)
                                Log.Debug("No Item Spawned");
                        }
                    }
                    else if (LarryContained)
                    {
                        if (Config.debug)
                        {
                            Log.Debug("Larry has been contained. Pinata activated.");
                        }
                        if (itemCount <= Config.itemsDroppedOnLarryDeath)
                        {
                            spitOutItems(itemCount);
                        }
                        else if (itemCount > Config.itemsDroppedOnLarryDeath)
                        {
                            spitOutItems(Config.itemsDroppedOnLarryDeath);
                        }
                    }
                }
                float timeBetween = UnityEngine.Random.Range(Config.minTimeForDrop, Config.maxTimeForDrop);
                yield return Timing.WaitForSeconds(timeBetween);
            }
        }
    }
}
