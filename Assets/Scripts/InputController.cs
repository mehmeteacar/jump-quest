using UnityEngine;

public class InputController : MonoBehaviour
{
    public float MovementX { get; private set; }
    public float MovementY { get; private set; }

    void Update()
    {
        MovementX = Input.GetAxis("Horizontal");
        MovementY = Input.GetAxis("Vertical");

        if(Variables.levelFinished)
        {
            MovementX = 0.7f;
            MovementY = 0f;
        }
    }
}
