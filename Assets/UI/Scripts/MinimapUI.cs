﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    [SerializeField]
    private Transform left;
    [SerializeField]
    private Transform right;
    [SerializeField]
    private Transform top;
    [SerializeField]
    private Transform bottom;

    [SerializeField]
    private Image minimapImage;
    [SerializeField]
    private Image minimapPlayerImage;

    private CharacterMover targetPlayer;

    // Start is called before the first frame update
    void Start()
    {
        var inst = Instantiate(minimapImage.material);
        minimapImage.material = inst;
        targetPlayer = AmongUsRoomPlayer.MyRoomPlayer.myCharacter;
    }

    // Update is called once per frame
    void Update()
    {
        if(targetPlayer != null)
        {
            Vector2 mapArea = new Vector2(Vector3.Distance(left.position, right.position), Vector3.Distance(bottom.position, top.position));
            Vector2 charPos = new Vector2(Vector3.Distance(left.position, new Vector3(targetPlayer.transform.position.x, 0f, 0f)), Vector3.Distance(bottom.position, new Vector3(0f, targetPlayer.transform.position.y, 0f)));
            Vector2 normalPos = new Vector2(1f - (charPos.x / mapArea.x), 1f - (charPos.y / mapArea.y));
            // 미니맵 상의 캐릭터 아이콘 위치 업데이트
            Vector2 centeredPosition = new Vector2(
                (normalPos.x - 0.5f) * minimapImage.rectTransform.sizeDelta.x * 3,
                (normalPos.y - 0.5f) * minimapImage.rectTransform.sizeDelta.y * 3
            );

            minimapPlayerImage.rectTransform.anchoredPosition = centeredPosition;
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
