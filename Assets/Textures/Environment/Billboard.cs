using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;
    // Start is called before the first frame update
    void Start()
    {
        // This is causing errors for some reason? What?
        // Shouldn't be too big an issue.
        mainCameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (mainCameraTransform != null)
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward, mainCameraTransform.rotation * Vector3.up);
        else
        {
            mainCameraTransform = Camera.main.transform;
        }
    }
}
