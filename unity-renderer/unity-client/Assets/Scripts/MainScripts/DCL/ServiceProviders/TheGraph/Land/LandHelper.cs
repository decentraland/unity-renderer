using System.Collections.Generic;
using System.Linq;

internal static class LandHelper
{
    public static List<Land> ConvertQueryResult(LandQueryResult queryResult, string lowerCaseAddress)
    {
        List<Land> lands = new List<Land>();

        // parcels and estates that I own
        for (int i = 0; i < queryResult.ownerParcels.Length; i++)
        {
            lands.Add(FromParcel(queryResult.ownerParcels[i], LandRole.OWNER));
        }
        for (int i = 0; i < queryResult.ownerEstates.Length; i++)
        {
            lands.Add(FromEstate(queryResult.ownerEstates[i], LandRole.OWNER));
        }

        // parcels and estates that I operate
        for (int i = 0; i < queryResult.updateOperatorParcels.Length; i++)
        {
            lands.Add(FromParcel(queryResult.updateOperatorParcels[i], LandRole.OPERATOR));
        }
        for (int i = 0; i < queryResult.updateOperatorEstates.Length; i++)
        {
            lands.Add(FromEstate(queryResult.updateOperatorEstates[i], LandRole.OPERATOR));
        }

        // I'm operator of all the lands from addresses that gave me UpdateManager permission
        for (int i = 0; i < queryResult.operatorAuthorizations.Length; i++)
        {
            var owner = queryResult.operatorAuthorizations[i].owner;
            for (int j = 0; j < owner.parcels.Length; j++)
            {
                var authLand = FromParcel(owner.parcels[i], LandRole.OPERATOR);
                authLand.operators.Add(lowerCaseAddress);

                // skip if already owned or operated
                if (!lands.Exists(l => l.id == authLand.id))
                {
                    lands.Add(authLand);
                }
            }
            for (int j = 0; j < owner.estates.Length; j++)
            {
                if (owner.estates[i].parcels.Length == 0)
                    continue;

                var authEstate = FromEstate(owner.estates[i], LandRole.OPERATOR);
                authEstate.operators.Add(lowerCaseAddress);

                // skip if already owned or operated
                if (!lands.Exists(l => l.id == authEstate.id))
                {
                    lands.Add(authEstate);
                }
            }
        }

        return lands
               .Where(land => land.type == LandType.PARCEL || land.parcels.Count > 0)
               .ToList();
    }

    static Land FromParcel(ParcelFields parcel, LandRole role)
    {
        string id = CoordsToId(parcel.x, parcel.y);
        Land result = new Land()
        {
            id = id,
            name = parcel.data?.name ?? $"Parcel {id}",
            type = LandType.PARCEL,
            role = role,
            description = parcel.data?.description,
            owner = parcel.owner.address,
            operators = new List<string>()
        };

        if (int.TryParse(parcel.x, out int x))
            result.x = x;
        if (int.TryParse(parcel.y, out int y))
            result.y = y;

        if (!string.IsNullOrEmpty(parcel.updateOperator))
        {
            result.operators.Add(parcel.updateOperator);
        }

        return result;
    }

    static Land FromEstate(EstateFields estate, LandRole role)
    {
        string id = estate.id;

        Land result = new Land()
        {
            id = id,
            name = estate.data?.name ?? $"Parcel {id}",
            type = LandType.ESTATE,
            role = role,
            description = estate.data?.description,
            size = estate.size,
            parcels = new List<Parcel>(),
            owner = estate.owner.address,
            operators = new List<string>()
        };

        for (int i = 0; i < estate.parcels.Length; i++)
        {

            if (!(int.TryParse(estate.parcels[i].x, out int x) && (int.TryParse(estate.parcels[i].y, out int y))))
            {
                continue;
            }
            result.parcels.Add(new Parcel()
            {
                x = x,
                y = y,
                id = CoordsToId(estate.parcels[i].x, estate.parcels[i].y)
            });
        }

        if (!string.IsNullOrEmpty(estate.updateOperator))
        {
            result.operators.Add(estate.updateOperator);
        }

        return result;
    }

    static string CoordsToId(string x, string y) { return $"{x},{y}"; }
}