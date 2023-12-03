using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Player player;
    public ObjectManager objectManager;
    public DSLManager dslManager;
    public DontDestory dontDestory;
    public GameObject gameOverUI;

    public GameObject[] players, stairs, UI;
    public GameObject pauseBtn, backGround;

    public AudioSource[] sound;
    public Animator[] anim;
    public Text finalScoreText, bestScoreText, scoreText;
    public Image gauge;
    public Button[] settingButtons;

    int score, selectedIndex;
    public bool gaugeStart = false, vibrationOn = true, isGamePaused = false;
    float gaugeRedcutionRate = 0.0005f;
    public bool[] IsChangeDir = new bool[20];

    Vector3 beforePos,
    startPos = new Vector3(-0.8f, -1.5f, 0),
    leftPos = new Vector3(-0.8f, 0.4f, 0),
    rightPos = new Vector3(0.8f, 0.4f, 0),
    leftDir = new Vector3(0.8f, -0.4f, 0),
    rightDir = new Vector3(-0.8f, -0.4f, 0);

    enum State { start, leftDir, rightDir }
    State state = State.start;

    void Awake()
    {
        players[selectedIndex].SetActive(true);
        player = players[selectedIndex].GetComponent<Player>();

        StairsInit();
        GaugeReduce();
        StartCoroutine(CheckGauge());

        UI[0].SetActive(dslManager.IsRetry());
        UI[1].SetActive(!dslManager.IsRetry());
        gameOverUI.SetActive(false);
    }

    // 처음에 계단 생성
    void StairsInit()
    {
        for (int i = 0; i < 20; i++)
        {
            switch (state)
            {
                case State.start:
                    stairs[i].transform.position = startPos;
                    state = State.leftDir;
                    break;
                case State.leftDir:
                    stairs[i].transform.position = beforePos + leftPos;
                    break;
                case State.rightDir:
                    stairs[i].transform.position = beforePos + rightPos;
                    break;
            }
            beforePos = stairs[i].transform.position;

            if (i != 0)
            {
                // 랜덤 확률에 따른 코인 오브젝트 활성화
                if (Random.Range(1, 9) < 3)
                {
                    objectManager.MakeObj("coin", i);
                }
                if (Random.Range(1, 9) < 3 && i < 19)
                {
                    if (state == State.leftDir) state = State.rightDir;
                    else if (state == State.rightDir) state = State.leftDir;
                    IsChangeDir[i + 1] = true;
                }
            }
        }
    }

    // 임의의 위치에 계단 생성
    void SpawnStair(int num)
    {
        IsChangeDir[num + 1 == 20 ? 0 : num + 1] = false;
        beforePos = stairs[num == 0 ? 19 : num - 1].transform.position;

        switch (state)
        {
            case State.leftDir:
                stairs[num].transform.position = beforePos + leftPos;
                break;
            case State.rightDir:
                stairs[num].transform.position = beforePos + rightPos;
                break;
        }

        // 랜덤 확률에 따른 코인 오브젝트 활성화
        if (Random.Range(1, 9) < 3) objectManager.MakeObj("coin", num);
        if (Random.Range(1, 9) < 3)
        {
            if (state == State.leftDir) state = State.rightDir;
            else if (state == State.rightDir) state = State.leftDir;
            IsChangeDir[num + 1 == 20 ? 0 : num + 1] = true;
        }
    }

    // 방향을 따라 움직이는 계단
    public void StairMove(int stairIndex, bool isChange, bool isleft)
    {
        if (player.isDie) return;

        // 계단을 오른쪽이나 왼쪽으로 이동
        for (int i = 0; i < 20; i++)
        {
            if (isleft) stairs[i].transform.position += leftDir;
            else stairs[i].transform.position += rightDir;
        }

        // 계단이 일정 높이 이하면 이동
        for (int i = 0; i < 20; i++)
        {
            if (stairs[i].transform.position.y < -5) SpawnStair(i);
        }

        // 계단을 잘못 오르면 게임 오버
        if (IsChangeDir[stairIndex] != isChange)
        {
            GameOver();
            return;
        }

        // 점수 업데이트 및 게이지 증가
        scoreText.text = (++score).ToString();
        gauge.fillAmount += 0.7f;

        // 배경화면 무한 스크롤링
        backGround.transform.position += backGround.transform.position.y < -14f ?
            new Vector3(0, 4.7f, 0) : new Vector3(0, -0.05f, 0);
    }

    // Gauge (게이지바)
    void GaugeReduce()
    {
        if (gaugeStart)
        {
            // 점수가 높을수록 게이지 감소율 증가
            if (score > 30)  gaugeRedcutionRate = 0.0010f;
            if (score > 70)  gaugeRedcutionRate = 0.0015f;
            if (score > 100) gaugeRedcutionRate = 0.002f;
            if (score > 170) gaugeRedcutionRate = 0.0025f;
            if (score > 250) gaugeRedcutionRate = 0.003f;
            if (score > 400) gaugeRedcutionRate = 0.0035f;
            if (score > 600) gaugeRedcutionRate = 0.004f;
            gauge.fillAmount -= gaugeRedcutionRate;
        }
        Invoke("GaugeReduce", 0.01f);
    }

    // 게이지가 0이되는지 계속 검사
    IEnumerator CheckGauge()
    {
        while (gauge.fillAmount != 0)
        {
            yield return new WaitForSeconds(0.4f);
        }
        GameOver();
    }

    // 게임 오버
    void GameOver()
    {
        gameOverUI.SetActive(true);

        // Animation
        anim[0].SetBool("GameOver", true);
        player.anim.SetBool("Die", true);

        // UI
        ShowScore();
        pauseBtn.SetActive(false);

        player.isDie = true;
        player.MoveAnimation();
        if (vibrationOn) Vibration();
        dslManager.SaveMoney(player.money);

        CancelInvoke();
        Invoke("DisableUI", 1.5f);
    }

    // 게임 종료 후 점수 표시
    void ShowScore()
    {
        finalScoreText.text = score.ToString();
        dslManager.SaveRankScore(score);
        bestScoreText.text = dslManager.GetBestScore().ToString();

        // 최고점수가 기록되면
        if (score == dslManager.GetBestScore() && score != 0) UI[2].SetActive(true);
    }

    // 아래버튼
    public void BtnDown(GameObject btn)
    {
        btn.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        if (btn.name == "ClimbBtn") player.Climb(false);
        else if (btn.name == "ChangeDirBtn") player.Climb(true);
    }

    // 위버튼
    public void BtnUp(GameObject btn)
    {
        btn.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        if (btn.name == "PauseBtn")
        {
            CancelInvoke();
            isGamePaused = true;
        }
        if (btn.name == "ResumeBtn")
        {
            GaugeReduce();
            isGamePaused = false;
        }
    }

    // Setting (세팅)
    public void SoundInit()
    {
        selectedIndex = dslManager.GetSelectedCharIndex();
        player = players[selectedIndex].GetComponent<Player>();
        sound[3] = player.sound[0];
        sound[4] = player.sound[1];
        sound[5] = player.sound[2];
    }

    // 기본 설정 버튼
    public void SettingBtnInit()
    {
        bool on;

        for (int i = 0; i < 2; i++)
        {   // 배경음악 버튼
            on = dslManager.GetSettingOn("BgmBtn");
            if (on) settingButtons[i].image.color = new Color(1, 1, 1, 1f);
            else settingButtons[i].image.color = new Color(1, 1, 1, 0.5f);
        }

        for (int i = 2; i < 4; i++)
        {   // 사운드 버튼
            on = dslManager.GetSettingOn("SoundBtn");
            if (on) settingButtons[i].image.color = new Color(1, 1, 1, 1f);
            else settingButtons[i].image.color = new Color(1, 1, 1, 0.5f);
        }

        for (int i = 4; i < 6; i++)
        {   // 진동 버튼
            on = dslManager.GetSettingOn("VibrateBtn");
            if (on) settingButtons[i].image.color = new Color(1, 1, 1, 1f);
            else settingButtons[i].image.color = new Color(1, 1, 1, 0.5f);
        }
    }

    // 설정 버튼 색상 변경
    public void SettingBtnChange(Button btn)
    {
        bool on = dslManager.GetSettingOn(btn.name);

        if (btn.name == "BgmBtn")
        {   // 배경음악 버튼
            for (int i = 0; i < 2; i++)
            {
                if (on) settingButtons[i].image.color = new Color(1, 1, 1, 1f);
                else settingButtons[i].image.color = new Color(1, 1, 1, 0.5f);
            }
        }
        if (btn.name == "SoundBtn")
        {   // 사운드 버튼
            for (int i = 2; i < 4; i++)
            {
                if (on) settingButtons[i].image.color = new Color(1, 1, 1, 1f);
                else settingButtons[i].image.color = new Color(1, 1, 1, 0.5f);
            }
        }
        if (btn.name == "VibrateBtn")
        {   // 진동 버튼
            for (int i = 4; i < 6; i++)
            {
                if (on) settingButtons[i].image.color = new Color(1, 1, 1, 1f);
                else settingButtons[i].image.color = new Color(1, 1, 1, 0.5f);
            }
        }
    }

    // 설정 On, Off
    public void SettingOnOff(string type)
    {
        switch (type)
        {
            case "BgmBtn":
                if (dslManager.GetSettingOn(type))
                {
                    dontDestory.BgmPlay();
                }
                else
                {
                    dontDestory.BgmStop();
                }

                break;
            case "SoundBtn":
                bool isOn = !dslManager.GetSettingOn(type);

                for (int i = 0; i < sound.Length; i++)
                {
                    sound[i].mute = isOn;
                }

                break;
            case "VibrateBtn":
                vibrationOn = dslManager.GetSettingOn(type);

                break;
        }
    }

    void Vibration()
    {
        Handheld.Vibrate();
        sound[0].playOnAwake = false;
    }

    public void PlaySound(int index)
    {
        sound[index].Play();
    }

    void DisableUI()
    {
        UI[0].SetActive(false);
    }

    public void LoadScene(int i)
    {
        SceneManager.LoadScene(i);
    }

    private void OnApplicationQuit()
    {
        dslManager.SaveMoney(player.money);
    }
}
