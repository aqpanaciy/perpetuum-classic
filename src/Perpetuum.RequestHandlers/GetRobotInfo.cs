using System.Collections.Generic;
using System.Transactions;
using Perpetuum.Containers.SystemContainers;
using Perpetuum.Data;
using Perpetuum.Groups.Corporations;
using Perpetuum.Host.Requests;
using Perpetuum.Robots;
using Perpetuum.Zones;

namespace Perpetuum.RequestHandlers
{
    public class GetRobotInfo : IRequestHandler
    {
        private readonly IZoneManager _zoneManager;
        private readonly RobotHelper _robotHelper;

        public GetRobotInfo(IZoneManager zoneManager,RobotHelper robotHelper)
        {
            _zoneManager = zoneManager;
            _robotHelper = robotHelper;
        }

        public bool ForFitting { private get; set; } = true;

        public void HandleRequest(IRequest request)
        {
            using(var scope = Db.CreateTransaction())
            {
                var result = new Dictionary<string, object>();
                Transaction.Current.OnCompleted((_) =>
                {
                    Message.Builder.FromRequest(request).WithData(result).WrapToResult().WithEmpty().Send();
                });

                if (TryGetRobotFromZone(request, out Robot robot))
                {
                    robot.EnlistTransaction();
                }
                else
                {
                    var robotEid = request.Data.GetOrDefault<long>(k.robotEID);
                    robot = _robotHelper.LoadRobotForCharacter(robotEid,request.Session.Character);
                }

                if (robot == null)
                    throw new PerpetuumException(ErrorCodes.RobotNotFound);

                if ( !robot.IsSingleAndUnpacked)
                    throw new PerpetuumException(ErrorCodes.RobotMustbeSingleAndNonRepacked);

                if (ForFitting)
                {
                    robot.CheckOwnerOnlyCharacterAndThrowIfFailed(request.Session.Character);
                }
                else
                {
                    robot.CheckOwnerCharacterAndCorporationAndThrowIfFailed(request.Session.Character);
                }

                switch (robot.GetOrLoadParentEntity())
                {
                    case DefaultSystemContainer _:
                    case RobotInventory _ when ForFitting:
                    case CorporateHangar _ when ForFitting:
                    {
                        throw new PerpetuumException(ErrorCodes.AccessDenied);
                    }
                }

                result.Add(k.robot, robot.ToDictionary());
                scope.Complete();
            }
        }

        private bool TryGetRobotFromZone(IRequest request, out Robot robot)
        {
            foreach (var zone in _zoneManager.Zones)
            {
                robot = zone.GetPlayer(request.Session.Character);
                if (robot != null)
                    return true;
            }

            robot = null;
            return false;
        }
    }
}