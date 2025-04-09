using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemStats : ScriptableObject
{
    public int itemID;
    public int itemRarity;
    public string itemName;
    public string itemDescription;
    public Sprite itemSprite;

    public int hpBonus;
    public int meleeBonus;
    public int rangedBonus;
    public int spdBonus;
}


    