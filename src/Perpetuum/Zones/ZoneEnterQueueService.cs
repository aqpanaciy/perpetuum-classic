using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Perpetuum.Accounting.Characters;
using Perpetuum.Log;
using Perpetuum.Players;
using Perpetuum.Threading.Process;
using Perpetuum.Timers;
using Perpetuum.Zones.Terrains.Materials.Plants;

namespace Perpetuum.Zones
{
    public class ZoneEnterQueueService : Process,IZoneEnterQueueService
    {
        private static readonly ILogger _logger = Logger.Factory.CreateLogger("ZoneEnterQueueService");

        public delegate IZoneEnterQueueService Factory(IZone zone);

        private readonly IntervalTimer _timer = new IntervalTimer(TimeSpan.FromSeconds(2));
        private readonly IZone _zone;
        private bool _processing;
        private Queue<QueueItem> _queue = new Queue<QueueItem>();

        public ZoneEnterQueueService(IZone zone)
        {
            _zone = zone;
            MaxPlayersOnZone = zone.Configuration.MaxPlayers;
        }

        private bool HasFreeSlot
        {
            get
            {
                var playersCount = _zone.Players.Count();
                return playersCount < MaxPlayersOnZone;
            }
        }

        private void OnQueueChanged()
        {
            QueueItem[] items;
            lock (_queue)
            {
                if ( _queue.Count == 0 )
                    return;
                
                items = _queue.ToArray();
            }

            var messageBuilder = Message.Builder.SetCommand(new Command("zoneEnterQueueInfo")).SetData("length", items.Length);

            foreach (var queueInfo in items.Select((info, currentPosition) => new { info, currentPosition }))
            {
                messageBuilder.SetData("current", queueInfo.currentPosition).ToCharacter(queueInfo.info.character).Send();
            }
        }

        public override void Update(TimeSpan time)
        {
            _timer.Update(time).IsPassed(ProcessQueueAsync);
        }

        private void ProcessQueueAsync()
        {
            if ( _processing || _queue.Count == 0 )
                return;

            _logger.LogInformation("Start processing queue. zone:" + _zone.Id + " count:" + _queue.Count);

            _processing = true;
            ThreadPool.UnsafeQueueUserWorkItem(_ => ProcessQueue(), null);
        }


        private void ProcessQueue()
        {
            using (var scope = _logger.BeginScope("ZPQ"))
            {
                try
                {
                    while (true)
                    {
                        if (!HasFreeSlot)
                            return;

                        QueueItem item;
                        lock (_queue)
                        {
                            if (_queue.Count == 0)
                                return;

                            item = _queue.Dequeue();
                        }


                        _logger.LogInformation("Start processing character. zone:" + _zone.Id + " character:" + item.character + " command:" + item.replyCommand);

                        var character = item.character;
                        var replyCommand = item.replyCommand;
                        using (var iner_scope = _logger.BeginScope("UREQ"))
                        {
                            try
                            {
                                LoadPlayerAndSendReply(character, replyCommand);
                            }
                            catch (Exception ex)
                            {
                                var err = ErrorCodes.ServerError;

                                var gex = ex as PerpetuumException;
                                if (gex != null)
                                {
                                    _logger.LogError($"[UREQ] {gex.error} Req: {replyCommand}");
                                }
                                else
                                {
                                    _logger.LogCritical(ex, ex.Message);
                                }

                                character.CreateErrorMessage(replyCommand, err).Send();
                            }
                        }

                        _logger.LogInformation("End processing character. zone:" + _zone.Id + " character:" + item.character + " command:" + item.replyCommand);

                        OnQueueChanged();
                    }
                }
                finally
                {
                    _processing = false;
                    _logger.LogInformation("End processing queue. zone:" + _zone.Id + " count:" + _queue.Count);
                }
            }
        }

        public void EnqueuePlayer(Character character, Command replyCommand)
        {
            using (var scope = _logger.BeginScope("ZEP"))
            {
                _logger.LogInformation("Start enqueue player. zone:" + _zone.Id + " character:" + character + " command:" + replyCommand);
                lock (_queue)
                {
                    _queue.Enqueue(new QueueItem { character = character, replyCommand = replyCommand });
                }

                OnQueueChanged();
                _logger.LogInformation("End enqueue player. zone:" + _zone.Id + " character:" + character + " command:" + replyCommand);
            }
        }

        public void RemovePlayer(Character character)
        {
            var changed = false;

            try
            {
                lock (_queue)
                {
                    if ( _queue.Count == 0 )
                        return;

                    var newQ = new Queue<QueueItem>();

                    QueueItem item;
                    while (_queue.TryDequeue(out item))
                    {
                        if (item.character == character)
                        {
                            changed = true;
                            continue;
                        }

                        newQ.Enqueue(item);
                    }

                    _queue = newQ;
                }
            }
            finally
            {
                if ( changed )
                    OnQueueChanged();
            }
        }

        public int MaxPlayersOnZone { get; set; }

        public void LoadPlayerAndSendReply(Character character, Command replyCommand)
        {
            if (!_zone.TryGetPlayer(character, out Player player))
            {
                _logger.LogInformation("Start loading player. zone:" + _zone.Id + " character:" + character.Id);
                player = Player.LoadPlayerAndAddToZone(_zone, character);
                _logger.LogInformation("End loading player. zone:" + _zone.Id + " character:" + character.Id);
            }

            SendReplyCommand(character, player, replyCommand);
        }

        public void SendReplyCommand(Character character, Player player, Command replyCommand)
        {
            _logger.LogInformation("Start sending reply command. player: " + player.Eid + " character:" + character.Id + " reply:" + replyCommand);

            var result = new Dictionary<string, object>
            {
                {k.characterID, character.Id},
                {k.rootEID, character.Eid},
                {k.corporationEID, character.CorporationEid},
                {k.allianceEID, character.AllianceEid},
                {
                    k.zone, new Dictionary<string, object>
                    {
                        {k.robot, player.ToDictionary()},
                        {k.guid, ZoneTicket.CreateAndEncryptFor(character)},
                        {k.plugin, _zone.Configuration.PluginName},
                        {k.decor, _zone.DecorHandler.DecorObjectsToDictionary()},
                        {k.plants, _zone.Configuration.PlantRules.GetPlantInfoForClient()},
                        {k.buildings, _zone.GetBuildingsDictionaryForCharacter(player.Character)}
                    }
                }
            };

            Message.Builder.SetCommand(replyCommand)
                            .WithData(result)
                            .WrapToResult()
                            .ToCharacter(character)
                            .Send();

            _logger.LogInformation("End sending reply command. player: " + player.Eid + " character:" + character.Id + " reply:" + replyCommand);
        }

        public Dictionary<string, object> GetQueueInfoDictionary()
        {
            var result = new Dictionary<string, object>
            {
                {k.zoneID, _zone.Id}, 
                {k.length, MaxPlayersOnZone}, 
                {k.count,_queue.Count}
            };

            return result;
        }

        private struct QueueItem
        {
            public Character character;
            public Command replyCommand;
        }
    }
}
