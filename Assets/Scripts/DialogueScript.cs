using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueScript : MonoBehaviour
{
    public GameObject[] dialoguePanels;
    int index = 0;
    bool advancing = false;
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    private void Start()
    {
        foreach(GameObject panel in dialoguePanels)
        {
            panel.SetActive(false);
        }
        dialoguePanels[index].SetActive(true);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceScreen();
        }
    }

    void AdvanceScreen()
    {
        index++;
        if (index < dialoguePanels.Length)
        {
            dialoguePanels[index - 1].SetActive(false);
            dialoguePanels[index].SetActive(true);
            PlaySound(1);
        }
        else
        {
            if (!advancing)
            {
                advancing = true;
                StartCoroutine(LoadNextScene());
            }
        }
    }

    IEnumerator LoadNextScene()
    {
        PlaySound(0);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void PlaySound(int index = 0)
    {
        audioSource.clip = audioClips[index];
        audioSource.Play();
    }
}
