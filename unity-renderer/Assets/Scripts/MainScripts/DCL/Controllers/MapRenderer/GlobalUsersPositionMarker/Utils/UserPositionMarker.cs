using UnityEngine;
using System;
using Decentraland.Bff;
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

        public string name { set { markerObject.name = value; } }

        public Vector3 localPosition { set { markerObject.transform.localPosition = value; } }

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
                OnRealmChanged(DataStore.i.realm.realmName.Get(), null);
                DataStore.i.realm.realmName.OnChange -= OnRealmChanged;
                DataStore.i.realm.realmName.OnChange += OnRealmChanged;
            }
            else
            {
                DataStore.i.realm.realmName.OnChange -= OnRealmChanged;
            }

            markerObject.gameObject.SetActive(active);
        }


        public void Dispose()
        {
            DataStore.i.realm.realmName.OnChange -= OnRealmChanged;
            UnityEngine.Object.Destroy(markerObject.gameObject);
        }

        private void OnRealmChanged(string current, string prev)
        {
            if (current == null)
                return;
            
            SetColor(current.Equals(realmServer) ? markerObject.sameRealmColor : markerObject.otherRealmColor);
        }
        
        private void SetColor(Color color) { markerObject.color = color; }
    }
}