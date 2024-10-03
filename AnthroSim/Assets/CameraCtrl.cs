using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    [SerializeField]
    float _cameraSpeed;

    Vector3 _input;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _input.x = Input.GetAxisRaw("Horizontal");
        _input.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        Vector3 currentCameraPosition = gameObject.transform.position;
        currentCameraPosition += _input * _cameraSpeed;
        gameObject.transform.position = currentCameraPosition;
    }
}
