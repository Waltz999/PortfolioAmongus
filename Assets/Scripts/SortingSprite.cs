﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SortingSprite : MonoBehaviour
{
    public enum ESortingType
    { 
        Static, Update
    }

    [SerializeField]
    private ESortingType sortingType;

    private SpriteSorter sorter;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        sorter = FindObjectOfType<SpriteSorter>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sortingOrder = sorter.GetSortingOrder(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(sortingType == ESortingType.Update)
        {
            spriteRenderer.sortingOrder = sorter.GetSortingOrder(gameObject);
        }
    }
}
