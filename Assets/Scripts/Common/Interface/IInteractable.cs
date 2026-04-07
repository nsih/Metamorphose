// Assets/Scripts/Common/Interface/IInteractable.cs
using UnityEngine;

public interface IInteractable
{
    Transform InteractPoint { get; }
    string PromptText { get; }
    void Interact();
}