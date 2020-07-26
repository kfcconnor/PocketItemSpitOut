using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled;
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

        public IEnumerator<float> ItemDrop()
        {
            yield return Timing.WaitForSeconds(UnityEngine.Random.Range(Config.minTimeForDrop, Config.maxTimeForDrop));
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
                            ValidRooms.Add(room);
                            if (Config.debug)
                                Log.Debug("Added Room " + room.Name + " To Valid Rooms");
                        }
                        DconRun = true;
                    }
                }
                int randChance = UnityEngine.Random.Range(0, 100);
                if(randChance <= Config.chanceForDrop)
                {
                    int sRoom = UnityEngine.Random.Range(0, ValidRooms.Count - 1);
                    if (Config.debug)
                        Log.Debug("Selected Room is " + ValidRooms[sRoom].Name);
                    if (!tempBeingRead)
                    {
                        foreach (ItemType item in tempDropped)
                        {
                            itemsDropped.Add(item);
                        }
                        tempDropped.Clear();
                    }
                    bool isEmpty = !itemsDropped.Any();
                    int sItem;
                    if (!isEmpty)
                    {
                        sItem = UnityEngine.Random.Range(0, itemsDropped.Count - 1);

                        if (Config.debug)
                            Log.Debug("Selected Item is " + itemsDropped[sItem].ToString());

                        UnityEngine.Vector3 Spawnlocation = ValidRooms[sRoom].Position;
                        Item.Spawn(itemsDropped[sItem], 1, Spawnlocation);
                        if (Config.debug)
                            Log.Debug("Item Spawned");
                        itemsDropped.RemoveAt(sItem);
                    }
                    else
                    {
                        if (Config.debug)
                            Log.Debug("List Empty, Skipping");
                    }
                }
                else
                {
                    if (Config.debug)
                        Log.Debug("No Item Spawned");
                }
                float timeBetween = UnityEngine.Random.Range(Config.minTimeForDrop, Config.maxTimeForDrop);
                yield return Timing.WaitForSeconds(timeBetween);
            }
        }
    }
}
