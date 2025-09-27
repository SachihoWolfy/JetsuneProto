using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleCard : MonoBehaviour
{
    public List<ParticleCard> cards;
    public int selectedIndex = -1;
    // Start is called before the first frame update
    void Start()
    {
        foreach(ParticleCard card in cards)
        {
            card.StopCard();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            Cycle();
        }
    }
    public void Cycle()
    {
        cards[selectedIndex].StopCard();
        if(selectedIndex + 1 < cards.Count)
        {
            selectedIndex++;
        }
        else
        {
            selectedIndex = 0;
        }
        cards[selectedIndex].gameObject.SetActive(true);
        cards[selectedIndex].PlayCard();
    }
}
