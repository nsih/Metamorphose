using UnityEngine;
using Reflex.Attributes;

public class PlayerMovement : MonoBehaviour
{
    [Inject]
    private IInputService iInputService;

    [Inject]
    private PlayerStat playerStat;

    void Start()
    {
        if (iInputService == null)
        {
            Debug.LogError("IInputService error");
        }

        if (playerStat == null)
        {
            Debug.LogError("PlayerStats error");
        }
    }

    void Update()
    {
        if (iInputService != null && playerStat != null)
        {
            Vector2 direction = iInputService.MoveDirection;
            transform.Translate(direction * playerStat.MoveSpeed * Time.deltaTime);
        }
    }
}