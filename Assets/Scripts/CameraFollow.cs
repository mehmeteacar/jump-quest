using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraFollow : MonoBehaviour
{
    [FormerlySerializedAs("takipTransform")] public Transform followTransform;
    [FormerlySerializedAs("sinirlar")] public BoxCollider2D borders;

    private float xMin, xMax, yMin, yMax;
    private float camY, camX;
    private float camOrthSize;
    private float cameraWidth;
    private Camera mainCam;

    private float smoothness = 5f;
    private float yGap = 1.8f;

    void Start()
    {
        xMin = borders.bounds.min.x;
        xMax = borders.bounds.max.x;
        yMin = borders.bounds.min.y;
        yMax = borders.bounds.max.y;
        mainCam = GetComponent<Camera>();
        camOrthSize = mainCam.orthographicSize;
        cameraWidth = camOrthSize * mainCam.aspect;
    }

    void FixedUpdate()
    {
        camY = Mathf.Clamp(followTransform.position.y + yGap, yMin + camOrthSize, yMax - camOrthSize);
        camX = Mathf.Clamp(followTransform.position.x, xMin + cameraWidth, xMax - cameraWidth);
        transform.position = Vector3.Lerp(transform.position, new Vector3(camX, camY, transform.position.z), smoothness * Time.fixedDeltaTime);
    }
}
