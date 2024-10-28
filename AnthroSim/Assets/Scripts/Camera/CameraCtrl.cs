using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    [SerializeField]
    float _cameraSpeed;

    [SerializeField]
    float _zoomSpeed;

    Vector3 _horiVertiInput;

    float _zoomInput;

    bool _fixedUpdateComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_fixedUpdateComplete)
        {
            _horiVertiInput.x = Input.GetAxisRaw("Horizontal");
            _horiVertiInput.z = Input.GetAxisRaw("Vertical");
            _zoomInput = Input.GetAxis("Zoom");
            _fixedUpdateComplete = false;
        }
    }

    private void FixedUpdate()
    {
        Vector3 currentCameraPosition = gameObject.transform.position;
        currentCameraPosition += _horiVertiInput * _cameraSpeed;
        currentCameraPosition.y += _zoomInput * _zoomSpeed;
        gameObject.transform.position = currentCameraPosition;
        _fixedUpdateComplete = true;
    }
}
