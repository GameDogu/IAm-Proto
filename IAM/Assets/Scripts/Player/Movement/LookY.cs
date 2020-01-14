using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookY : MonoBehaviour
{
    [SerializeField] float sensitivity = 2.5f;
    [SerializeField] Vector2 yAngleMinMax = Vector2.right * -30 + Vector2.up * 30;
    [SerializeField] bool invert = false;
    float rotationY = 0;
    float invertAxis = -1f;

    private void OnValidate()
    {
        if (invert)
            invertAxis = 1;
        else
            invertAxis = -1;
    }

    private void Start()
    {
        OnValidate();
    }

    private void Update()
    {
        rotationY += Input.GetAxis("Mouse Y")*sensitivity;
        rotationY = Mathf.Clamp(rotationY, yAngleMinMax.x, yAngleMinMax.y);
        transform.localEulerAngles = new Vector3(invertAxis*rotationY, transform.localEulerAngles.y, 0);
    }
}
