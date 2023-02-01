using DCL.Interface;
using System.Collections.Generic;
using System.Linq;

namespace DCLServices.WearablesCatalogService
{
    public class WearablesWebInterfaceBridge
    {
        public virtual void RequestWearables(string ownedByUser, IEnumerable<string> wearableIds, IEnumerable<string> collectionIds, string context) =>
            WebInterface.RequestWearables(ownedByUser, wearableIds.ToArray(), collectionIds.ToArray(), context);

        public virtual void RequestThirdPartyWearables(string ownedByUser, string thirdPartyCollectionId, string context) =>
            WebInterface.RequestThirdPartyWearables(ownedByUser, thirdPartyCollectionId, context);
    }
}
