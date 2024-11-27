using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnloadTrigger : MonoBehaviour
{
    public GameObject[] gameObjects;
    public bool loadMode;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (loadMode)
            {
                foreach (GameObject obj in gameObjects)
                {
                    obj.SetActive(true);
                }
            }
            else
            {
                foreach (GameObject obj in gameObjects)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
