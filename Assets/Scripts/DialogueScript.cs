using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueScript : MonoBehaviour
{
    [System.Serializable]
    public class DialoguePanel
    {
        public string speakerName;           
        public Sprite leftPortrait;          
        public bool showLeftPortrait;        
        public Sprite rightPortrait;         
        public bool showRightPortrait;       
        [TextArea(3, 10)]
        public string dialogueText;          
        public AudioClip soundEffect;        
    }

    public DialoguePanel[] dialoguePanels;   
    public Image leftPortraitImage;          
    public Image rightPortraitImage;         
    public TMP_Text nameText;                
    public TMP_Text dialogueText;            
    public AudioSource audioSource;          
    public AudioClip nextSceneSound;
    public AudioClip titleCardSound;
    public GameObject titleCard;             
    public float textSpeed = 0.05f;          
    public float sceneTransitionDelay = 2f;  

    private int index = 0;                   
    private bool isTyping = false;           
    private bool advancing = false;          

    private void Start()
    {
        // Start with all dialogue panels hidden and title card inactive
        if (titleCard != null)
            titleCard.SetActive(false);

        LoadDialoguePanel(index);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        if (isTyping)
        {
            // Complete typing immediately if player advances mid-animation
            StopAllCoroutines();
            dialogueText.text = dialoguePanels[index].dialogueText;
            isTyping = false;
            return;
        }

        index++;
        if (index < dialoguePanels.Length)
        {
            LoadDialoguePanel(index);
        }
        else if (index == dialoguePanels.Length)
        {
            PlaySound(titleCardSound);
            ShowTitleCard();
        }
        else if (!advancing)
        {
            advancing = true;
            StartCoroutine(LoadNextScene());
        }
    }

    private void LoadDialoguePanel(int panelIndex)
    {
        DialoguePanel panel = dialoguePanels[panelIndex];

        // Update speaker name and dialogue text
        if (nameText != null) nameText.text = panel.speakerName;
        PlaySound(panel.soundEffect);

        // Display or hide portraits based on panel settings
        if (panel.showLeftPortrait)
        {
            leftPortraitImage.sprite = panel.leftPortrait;
            leftPortraitImage.gameObject.SetActive(true);
        }
        else
        {
            leftPortraitImage.gameObject.SetActive(false);
        }

        if (panel.showRightPortrait)
        {
            rightPortraitImage.sprite = panel.rightPortrait;
            rightPortraitImage.gameObject.SetActive(true);
        }
        else
        {
            rightPortraitImage.gameObject.SetActive(false);
        }

        // Start typing the text
        StartCoroutine(TypeDialogue(panel.dialogueText));
    }

    private IEnumerator TypeDialogue(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
        isTyping = false;
    }

    private void ShowTitleCard()
    {
        // Deactivate dialogue UI if needed and show the title card
        leftPortraitImage.gameObject.SetActive(false);
        rightPortraitImage.gameObject.SetActive(false);
        if (nameText != null) nameText.gameObject.SetActive(false);
        dialogueText.text = "";

        if (titleCard != null)
            titleCard.SetActive(true);
    }

    private IEnumerator LoadNextScene()
    {
        PlaySound(nextSceneSound);
        yield return new WaitForSeconds(sceneTransitionDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}