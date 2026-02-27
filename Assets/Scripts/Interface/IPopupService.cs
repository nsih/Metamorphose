using UnityEngine;

namespace TJR.Core.Interface
{
    public interface IPopupService
    {
        GameObject OpenPopup(string popupName);
        void ClosePopup(GameObject popupInstance);
        bool IsPopupOpen(string popupName);
    }
}