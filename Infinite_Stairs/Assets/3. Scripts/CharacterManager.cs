using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public int index;
    string[] characterNames = { "회사원", "래퍼", "비서" };
    public DSLManager dslManager;
    public GameObject selectBtn, purchaseBtn;
    AudioSource sound;
    public Image characterImage;
    public Text characterName, price;

    private void Awake() 
    {
        index = dslManager.GetSelectedCharIndex();
        sound = GetComponent<AudioSource>();
        sound.mute = !dslManager.GetSettingOn("SoundBtn");
        ArrowBtn("null");
    }

    // 좌우로 뒤집을 때 캐릭터의 이미지, 이름, 가격 변경
    public void ArrowBtn(string dir)
    {
        if (dir == "Right") 
        {
             if (++index == dslManager.characterSprite.Length-1) index = 0; 
        }

        if (dir == "Left") 
        { 
             if (--index == -1) index = dslManager.characterSprite.Length - 2; 
        }

        // 인덱스의 문자 정보 변경
        characterImage.sprite = dslManager.characterSprite[index];
        characterName.text = characterNames[index];
        price.text = "￦" + dslManager.GetPrice().ToString();

        // 구매에 따른 버튼의 종류 결정
        selectBtn.SetActive(dslManager.IsPurchased(index));
        purchaseBtn.SetActive(!dslManager.IsPurchased(index));
    }
}
