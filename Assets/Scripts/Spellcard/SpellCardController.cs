using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCardController : MonoBehaviour
{
    public SpellCard[] spellCards;

    private int activeSpellCard = -1;

    private void Start()
    {
        foreach (SpellCard spellCard in spellCards)
        {
            spellCard.transform.parent = transform;
            spellCard.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
    }
    public void SwitchSpellCard(int index)
    {
        if (index == activeSpellCard)
        {
            return;
        }
        foreach (SpellCard spellCard in spellCards)
        {
            spellCard.DeactivateSpellCard();
        }
        spellCards[index].ActivateSpellCard();
        activeSpellCard = index;
    }
    public void DisableAllCards()
    {
        foreach (SpellCard spellCard in spellCards)
        {
            spellCard.DeactivateSpellCard();
        }
    }
}
