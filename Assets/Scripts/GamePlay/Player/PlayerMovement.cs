using UnityEngine;
using Reflex.Attributes;

public class PlayerMovement : MonoBehaviour
{
    [Inject]
    private IInputService input;

    [Inject]
    private PlayerStat playerStat;

    void Start()
    {
        if (input == null)
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
        if (input != null && playerStat != null)
        {
            Vector2 direction = input.MoveDirection;
            transform.Translate(direction * playerStat.MoveSpeed * Time.deltaTime);
        }
    }
}