using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject coinPrefab;

    GameObject[] coin;
    GameObject[] targetPool;

    // 오브젝트 풀링
    void Awake()
    {
        coin = new GameObject[20];
        Generate();
    }

    void Generate()
    {
        for(int i = 0; i < coin.Length; i++)
        {
            coin[i] = Instantiate(coinPrefab, gameManager.stairs[i].transform); // 계단 오브젝트의 자식으로 설정
            coin[i].transform.position += new Vector3(0, 0.6f, 0);
            coin[i].SetActive(false);
        }
       
    }

    public void MakeObj(string type, int index)
    {
        switch (type)
        {
            case "coin":
                targetPool = coin;
                break;
        }

        if (!targetPool[index].activeSelf)
        {
            targetPool[index].SetActive(true);
        }
    }
}
