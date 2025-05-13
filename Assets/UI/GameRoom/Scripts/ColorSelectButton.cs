using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSelectButton : MonoBehaviour
{
    [SerializeField]
    private GameObject x;
    
    public bool isInteractable = true;
    
    public void SetInteractable(bool isInteractable)
    {
        this.isInteractable = isInteractable;
        x.SetActive(!isInteractable);
    }

    public void ResetInteractable()
    {
        x.SetActive(!isInteractable); // isInteractable 값에 따라 활성화/비활성화를 설정합니다.
    }
}