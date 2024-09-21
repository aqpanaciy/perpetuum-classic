using System.Collections.Generic;
using System.Transactions;
using Perpetuum.Accounting.Characters;
using Perpetuum.Data;
using Perpetuum.Host.Requests;
using Perpetuum.Robots.Fitting;

namespace Perpetuum.RequestHandlers.FittingPreset
{
    public abstract class FittingPresetRequestHandler : IRequestHandler
    {
        public abstract void HandleRequest(IRequest request);

        protected static void SendAllPresetsToCharacter(IRequest request, IFittingPresetRepository repo)
        {
            using (var scope = Db.CreateTransaction())
            {
                var result = new Dictionary<string, object>();
                Transaction.Current.OnCommited(() =>
                {
                    Message.Builder.FromRequest(request).WithData(result).Send();
                });

                result.AddMany(repo.GetAll().ToDictionary("p", p => p.ToDictionary()));
            }
        }

        protected static IFittingPresetRepository GetFittingPresetRepository(Character character, bool forCorporation)
        {
            if (!forCorporation)
                return new CharacterFittingPresetRepository(character);

            var corporation = character.GetPrivateCorporationOrThrow();
            return new CorporationFittingPresetRepository(corporation);
        }
    }
}
