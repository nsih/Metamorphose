using UnityEngine;
using Common;

public class ExplosionTest : MonoBehaviour
{
    [SerializeField] private ExplosionConfigSO _config;
    [SerializeField] private ExplosionOwner _owner = ExplosionOwner.Player;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_config != null)
            {
                ExplosionUtil.Explode(transform.position, _owner, _config);
            }
            else
            {
                ExplosionUtil.Explode(transform.position, _owner, 3f, 5f);
            }
            
            Debug.Log("explosion triggered");
        }
    }
}