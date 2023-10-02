﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.Friends
{
    internal class FriendUserMarkerObject : MapRendererMarkerBase
    {
        [field: SerializeField]
        internal SpriteRenderer[] spriteRenderers { get; private set; }

        [field: SerializeField]
        internal Image profilePicture { get; private set; }

        [field: SerializeField]
        internal TMP_Text profileName { get; private set; }
    }
}