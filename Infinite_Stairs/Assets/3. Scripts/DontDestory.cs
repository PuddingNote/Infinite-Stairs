using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 오브젝트가 Scene을 다시 로드해도 파괴되지않도록 하기
public class DontDestory : MonoBehaviour
{
    public static DontDestory instance = null;
    AudioSource bgm;

    public static DontDestory Instance
    {
        get
        {
            if (instance == null || instance == default)
            {
                return null;
            }
            return instance;
        }
    }

    // 싱글톤 패턴 사용(Singleton Pattern) (그냥 사용하면 로드할때마다 오브젝트가 중복됨)
    // 전역 변수를 사용하지않고 객체를 하나만 생성 하도록 하며, 생성된 객체를 어디에서든지 참조할 수 있도록 하는 패턴
    // 하나의 객체로 중복 생성없이 객체를 유지할 수 있음
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
            {
                Destroy(this.gameObject);
            }
        }
    }

    // 배경음악재생
    public void BgmPlay()
    {
        if (Instance == null)
        {
            bgm = gameObject.GetComponent<AudioSource>();
        }
        else
        {
            bgm = Instance.GetComponent<AudioSource>();
        }
        bgm.enabled = true;
    }

    // 배경음악중지
    public void BgmStop()
    {
        if (Instance == null)
        {
            bgm = gameObject.GetComponent<AudioSource>();
        }
        else
        {
            bgm = Instance.GetComponent<AudioSource>();
        }
        bgm.enabled = false;
    }
}


