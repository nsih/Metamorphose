using UnityEngine;
using BulletPro;
using FMODUnity;

public class BulletShootAudioBehaviour : BaseBulletBehaviour
{
    [SerializeField] private string _eventPath = "";

    public override void OnBulletBirth()
    {
        if (string.IsNullOrEmpty(_eventPath)) return;
        RuntimeManager.PlayOneShot(_eventPath, bullet.self.position);
    }
}