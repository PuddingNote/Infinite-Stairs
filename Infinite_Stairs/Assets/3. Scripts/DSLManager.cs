using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Character 
{
    public string E_Name, K_Name;
    public int price;
    public bool selected, purchased;

    public Character(string E_Name, string K_Name, int price, bool selected, bool purchased) 
    {
        this.E_Name = E_Name;
        this.K_Name = K_Name;
        this.price = price;
        this.selected = selected;
        this.purchased = purchased;
    }
}

public class Ranking
{
    public int score, characterIndex;

    public Ranking(int score, int characterIndex) 
    {
        this.score = score;
        this.characterIndex = characterIndex;
    }
}

public class Inform
{
    public int money;
    public bool bgmOn, soundEffectOn, vibrationOn, Retry;

    public Inform(int money, bool bgmOn, bool soundEffectOn, bool vibrationOn, bool Retry) 
    {
        this.money = money;
        this.bgmOn = bgmOn;
        this.soundEffectOn = soundEffectOn;
        this.vibrationOn = vibrationOn;
        this.Retry = Retry;
    }
}

public class DSLManager : MonoBehaviour 
{
    List<Character> characters = new List<Character>();
    List<Ranking> rankings = new List<Ranking>();
    List<Inform> informs = new List<Inform>();

    public GameManager gameManager;
    public CharacterManager CharacterManager;
    public Text[] moneyText, rankingText;
    public Sprite[] characterSprite;
    public Image[] rankCharacterImg;
    
    private void Awake() 
    {
        gameManager = FindObjectOfType<GameManager>();

        // 처음에 데이터 저장
        if (!File.Exists(Application.persistentDataPath + "/Characters.json")) 
        {
            characters.Add(new Character("BusinessMan", "회사원", 0, true, true));
            characters.Add(new Character("Rapper", "래퍼", 500, false, false));
            characters.Add(new Character("Secretary", "비서", 500, false, false));

            rankings.Add(new Ranking(0, 7));
            rankings.Add(new Ranking(0, 7));
            rankings.Add(new Ranking(0, 7));
            rankings.Add(new Ranking(0, 7));

            informs.Add(new Inform(0, true, true, true, false));

            DataSave();
        }

        DataLoad();
        LoadMoney(GetMoney());
        LoadRanking();
        gameManager.SettingBtnInit();
        gameManager.SoundInit();
        gameManager.SettingOnOff("BgmBtn");
        gameManager.SettingOnOff("SoundBtn");
        gameManager.SettingOnOff("VibrateBtn");
    }

    // Data Save & Load (데이터 저장 및 로드)
    // 데이터 저장
    public void DataSave() 
    {   // 리스트들을 암호화하고 지정경로에 Json으로 변환하여 저장
        string jdata_0 = JsonConvert.SerializeObject(characters);
        string jdata_1 = JsonConvert.SerializeObject(rankings);
        string jdata_2 = JsonConvert.SerializeObject(informs);
       
        byte[] bytes_0 = System.Text.Encoding.UTF8.GetBytes(jdata_0);
        byte[] bytes_1 = System.Text.Encoding.UTF8.GetBytes(jdata_1);
        byte[] bytes_2 = System.Text.Encoding.UTF8.GetBytes(jdata_2);

        string format_0 = System.Convert.ToBase64String(bytes_0);
        string format_1 = System.Convert.ToBase64String(bytes_1);
        string format_2 = System.Convert.ToBase64String(bytes_2);       

        File.WriteAllText(Application.persistentDataPath + "/Characters.json", format_0);
        File.WriteAllText(Application.persistentDataPath + "/Rankings.json", format_1);
        File.WriteAllText(Application.persistentDataPath + "/Informs.json", format_2);
    }

    // 데이터 로드
    public void DataLoad()
    {   // 지정경로에 암호화된 Json파일을 복호화하고 변환 후 로드
        string jdata_0 = File.ReadAllText(Application.persistentDataPath + "/Characters.json");
        string jdata_1 = File.ReadAllText(Application.persistentDataPath + "/Rankings.json");
        string jdata_2 = File.ReadAllText(Application.persistentDataPath + "/Informs.json");
      
        byte[] bytes_0 = System.Convert.FromBase64String(jdata_0);
        byte[] bytes_1 = System.Convert.FromBase64String(jdata_1);
        byte[] bytes_2 = System.Convert.FromBase64String(jdata_2);

        string reformat_0 = System.Text.Encoding.UTF8.GetString(bytes_0);
        string reformat_1 = System.Text.Encoding.UTF8.GetString(bytes_1);
        string reformat_2 = System.Text.Encoding.UTF8.GetString(bytes_2);
        
        characters = JsonConvert.DeserializeObject<List<Character>>(reformat_0);
        rankings = JsonConvert.DeserializeObject<List<Ranking>>(reformat_1);
        informs = JsonConvert.DeserializeObject<List<Inform>>(reformat_2);
    }

    // Select Character (캐릭터 선택)
    // 문자 선택 및 문자 인덱스 저장
    public void SaveCharacterIndex()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].selected = false;
        }
        characters[CharacterManager.index].selected = true;
        DataSave();
        SceneManager.LoadScene(0);
    }

    // 캐릭터 인덱스값 가져오기
    public int GetSelectedCharIndex() 
    {
        DataLoad();
        for (int i = 0; i < characters.Count; i++)
            if (characters[i].selected) return i;
        return 0;
    }

    // Purchase Character (캐릭터 구입)
    public bool IsPurchased(int index) 
    {
        DataLoad();
        return characters[index].purchased;
    }

    // 캐릭터 구입 정보 저장
    public void SaveCharacterPurchased(Animator obj) 
    {
        if (characters[CharacterManager.index].price > informs[0].money)
        {
            obj.GetComponent<Animator>().SetTrigger("notice");
        }
        else 
        {
            // 캐릭터 구매 후 데이터 수정
            characters[CharacterManager.index].purchased = true;
            DataSave();
            DataLoad();
            informs[0].money -= characters[CharacterManager.index].price;
            DataSave();
            LoadMoney(informs[0].money);
            CharacterManager.ArrowBtn("null");
        }
    }

    // 캐릭터들 가격
    public int GetPrice() 
    { 
        return characters[CharacterManager.index].price;
    }

    // Money (재화)
    public int GetMoney() 
    {
        DataLoad();
        return informs[0].money;
    }

    // 재화 저장
    public void SaveMoney(int money) 
    {
        DataLoad();
        informs[0].money = money;
        DataSave();
    }

    // UI에서 금액 수정
    public void LoadMoney(int money) 
    {
        DataLoad();
        for (int i = 0; i < moneyText.Length; i++)
        {
            moneyText[i].text = money.ToString();
        }
        DataSave();
    }

    // Retry (재시작)
    public bool IsRetry() 
    { 
        return informs[0].Retry; 
    }

    public void ChangeRetry(bool isRetry)
    {
        DataLoad();
        informs[0].Retry = isRetry;
        DataSave();
    }

    // Ranking (랭킹)
    public void LoadRanking() 
    {
        for (int i = 0; i < rankingText.Length; i++) 
        {
            rankingText[i].text = rankings[i].score == 0 ? " " : rankings[i].score.ToString();
            rankCharacterImg[i].sprite = characterSprite[rankings[i].characterIndex];
        }
    }

    // 랭킹 점수 저장
    public void SaveRankScore(int finalScore)
    {
        rankings[3].score = finalScore;
        DataSave();

        // 현재 선택된 문자 인덱스 저장
        int charIndex = GetSelectedCharIndex();
        rankings[3].characterIndex = charIndex;

        // 점수를 기준으로 내림차순 정렬
        rankings.Sort(delegate (Ranking a, Ranking b) { return b.score.CompareTo(a.score); });

        DataSave();
        DataLoad();
    }

    // 최고 점수
    public int GetBestScore()
    {
        DataLoad();

        return rankings[0].score;
    }

    // Setting (설정)
    public bool GetSettingOn(string type) 
    {
        DataLoad();

        switch (type) 
        {
            case "BgmBtn":
                return informs[0].bgmOn;
            case "SoundBtn":
                return informs[0].soundEffectOn;
            case "VibrateBtn":
                return informs[0].vibrationOn;
        }

        return false;
    }

    // On,Off상태 변경
    public void ChangeOnOff(Button btn) 
    {
        DataLoad();

        if (btn.name == "BgmBtn") 
        {
            informs[0].bgmOn = !informs[0].bgmOn;
        }
        if (btn.name == "SoundBtn") 
        {
            informs[0].soundEffectOn = !informs[0].soundEffectOn;
        }
        if (btn.name == "VibrateBtn") 
        {
            informs[0].vibrationOn = !informs[0].vibrationOn;
        }
        DataSave();
        gameManager.SettingOnOff(btn.name);
        gameManager.SettingBtnChange(btn);
    }

    private void OnApplicationQuit() 
    {
        ChangeRetry(false);
    }
    
    private void OnApplicationPause() 
    {
        if (gameManager.isGamePaused)
        {
            return;
        }
        ChangeRetry(false);
    }

}
