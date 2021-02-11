using UnityEngine;
using System;
using Variables.RealmsInfo;

namespace DCL
{
    /// <summary>
    /// Wrapper class to handle user's marker GameObject
    /// </summary>
    internal class UserPositionMarker : IDisposable
    {
        public Vector2Int coords { set; get; }
        public string realmServer { set; get; }
        public string realmLayer { set; get; }

        public string name
        {
            set { markerObject.name = value; }
        }

        public Vector3 localPosition
        {
            set { markerObject.transform.localPosition = value; }
        }

        private UserMarkerObject markerObject;

        public UserPositionMarker(UserMarkerObject markerObject)
        {
            this.markerObject = markerObject;
            markerObject.gameObject.SetActive(false);
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                OnRealmChanged(DataStore.i.playerRealm.Get(), null);
                DataStore.i.playerRealm.OnChange -= OnRealmChanged;
                DataStore.i.playerRealm.OnChange += OnRealmChanged;
            }
            else
            {
                DataStore.i.playerRealm.OnChange -= OnRealmChanged;
            }

            markerObject.gameObject.SetActive(active);
        }

        public void Dispose()
        {
            DataStore.i.playerRealm.OnChange -= OnRealmChanged;
            GameObject.Destroy(markerObject.gameObject);
        }

        private void OnRealmChanged(CurrentRealmModel current, CurrentRealmModel prev)
        {
            if (current == null)
                return;

            SetColor(current.Equals(realmServer, realmLayer) ? markerObject.sameRealmColor : markerObject.otherRealmColor);
        }

        private void SetColor(Color color)
        {
            markerObject.color = color;
        }
    }
}