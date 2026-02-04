using System;
using System.Collections.Generic;
using System.Linq;
using TJR.Core.Interface;
using UnityEngine;

namespace TJR.Core.GamePlay.Service
{
    public class PopupService : IPopupService, IDisposable
    {
        Dictionary<string, GameObject> _popupPrefabs;
        List<GameObject> _popupInstances;

        public PopupService(GameObject popupPrefab)
        {
            _popupPrefabs = new Dictionary<string, GameObject>();
            _popupPrefabs.Add(popupPrefab.name, popupPrefab);
            _popupInstances = new List<GameObject>();
        }

        public GameObject OpenPopup(string popupName)
        {
            if (!_popupPrefabs.TryGetValue(popupName, out var prefab))
            {
                Debug.LogError($"PopupService: Popup prefab {popupName} not found");
                return null;
            }

            var mainCanvas = GameObject.FindFirstObjectByType<Canvas>().transform;
            var instance = GameObject.Instantiate(prefab, mainCanvas);
            instance.name = popupName;
            _popupInstances.Add(instance);
            return instance;
        }

        public void ClosePopup(GameObject popupInstance)
        {
            if (_popupInstances.Contains(popupInstance))
            {
                GameObject.Destroy(popupInstance);
                _popupInstances.Remove(popupInstance);
            }
            else
            {
                Debug.LogError($"PopupService: Popup instance {popupInstance.name} not found");
            }
        }

        public bool IsPopupOpen(string popupName)
        {
            return _popupInstances.Any(instance => instance.name == popupName);
        }

        public void Dispose()
        {
            foreach (var instance in _popupInstances)
            {
                GameObject.Destroy(instance);
            }
            _popupInstances.Clear();
            _popupPrefabs.Clear();
        }
    }
}