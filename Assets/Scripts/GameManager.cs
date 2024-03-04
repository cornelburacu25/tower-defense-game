using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public delegate void CurrencyChanged();

public class GameManager : Singleton<GameManager>
{
    public event CurrencyChanged Changed;
    public TowerButton ClickedButton { get; set; }

    private int currency;

    private int wave = 0;

    private int lives;

    private bool gameOver = false;

    private int health = 15;

    [SerializeField]
    private Text livesText;

    [SerializeField]
    private Text waveText;

    [SerializeField]
    private Text currencyText;

    [SerializeField]
    private GameObject waveButton;

    [SerializeField]
    private GameObject gameOverMenu;

    [SerializeField]
    private GameObject upgradePanel;

    [SerializeField]
    private GameObject statsPanel;

    [SerializeField]
    private Text sellText;

    [SerializeField]
    private Text statsText;

    [SerializeField]
    private Text upgradePrice;

    private Tower selectedTower;

    List<Monster> activeMonsters = new List<Monster>();

    public ObjectPool Pool { get; set; }

    [SerializeField]
    private GameObject inGameMenu;

    [SerializeField]
    private GameObject optionsMenu;

    public bool WaveActive
    {
        get
        {
            return activeMonsters.Count > 0;
        }
    }

    public int Currency
    { get 
        { 
            return currency; 
        } 
        set 
        {  
            this.currency = value;
            this.currencyText.text = value.ToString() + " <color=lime>$</color>";

            OnCurrencyChanged();
        } 
    }

    public int Lives
    { 
        get 
        { 
            return lives; 
        } 
        set 
        {
            this.lives = value;

            if (lives <= 0)
            {
                this.lives = 0;
                GameOver();
            }

            livesText.text = lives.ToString();
        } 
    }

    private void Awake()
    {
        Pool = GetComponent<ObjectPool>();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        Lives = 10;
        Currency = 50;
    }

    // Update is called once per frame
    void Update()
    {
        HandleEscape();
    }

    public void ShowInGameMenu()
    {
        if (optionsMenu.activeSelf)
        {
            ShowMenu();
        }
        else
        {
            inGameMenu.SetActive(!inGameMenu.activeSelf);

            if (!inGameMenu.activeSelf)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
            }
        }
    }

    public void ShowOptions()
    {
        inGameMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void ShowMenu()
    {
        inGameMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void OnCurrencyChanged()
    {
        if (Changed != null)
        {
            Changed();
        }
    }

    public void PickTower(TowerButton towerButton)
    {
        if(Currency >= towerButton.Price && !WaveActive)
        {
            this.ClickedButton = towerButton;
            Hover.Instance.Activate(towerButton.Sprite);
        }
       
    }

    public void BuyTower()
    {
        if( Currency >= ClickedButton.Price)
        {
            Currency -= ClickedButton.Price;

            Hover.Instance.Deactivate();
        }
        
    }

    private void DropTower()
    {
        ClickedButton = null;
        Hover.Instance.Deactivate();
    }

    private void HandleEscape()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            if (selectedTower == null && !Hover.Instance.IsVisible)
            {
                ShowInGameMenu();
            }

            else if (Hover.Instance.IsVisible)
            {
                DropTower();
            }

            else if (selectedTower != null)
            {
                DeselectTower();
            }
        }
    }

    public void SelectTower(Tower tower)
    {
        if (selectedTower != null)
        {
            selectedTower.Select();
        }

        selectedTower = tower;
        selectedTower.Select();

        sellText.text = "+ " + (selectedTower.Price / 2).ToString() + " $";
        upgradePanel.SetActive(true);
    }

    public void DeselectTower()
    {
        if (selectedTower != null)
        {
            selectedTower.Select();
        }
        upgradePanel.SetActive(false);
        selectedTower = null;
    }

    public void StartWave()
    {
        wave++;

        waveText.text = string.Format("Wave: <color=lime>{0}</color>", wave);

        StartCoroutine(SpawnWave());

        waveButton.SetActive(false);

    }

    private IEnumerator SpawnWave()
    {
        LevelManager.Instance.GeneratePath();

        for(int i = 0; i < wave; i++)
        {
            LevelManager.Instance.GeneratePath();

            int monsterIndex = Random.Range(0, 4);

            string type = string.Empty;

            switch (monsterIndex)
            {
                case 0:
                    type = "BlueMonster";
                    break;
                case 1:
                    type = "RedMonster";
                    break;
                case 2:
                    type = "GreenMonster";
                    break;
                case 3:
                    type = "PurpleMonster";
                    break;
            }

            Monster monster = Pool.GetObject(type).GetComponent<Monster>();
            monster.Spawn(health);

            if (wave % 3 == 0)
            {
                health += 5;
            }

            activeMonsters.Add(monster);

            yield return new WaitForSeconds(2.5f);
        }
    }

    public void RemoveMonster(Monster monster)
    {
        activeMonsters.Remove(monster);

        if(!WaveActive && !gameOver)
        {
            waveButton.SetActive(true);
        }
    }

    public void GameOver()
    {
        if (!gameOver)
        {
            gameOver = true;
            gameOverMenu.SetActive(true);
        }
    }

    public void Restart()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void SellTower()
    {
        if (selectedTower != null)
        {
            Currency += selectedTower.Price / 2;

            selectedTower.GetComponentInParent<TileScript>().IsEmpty = true;

            Destroy(selectedTower.transform.parent.gameObject);

            DeselectTower();
        }
    }

    public void ShowStats()
    {
        statsPanel.SetActive(!statsPanel.activeSelf);
    }

    public void SetTooltipText(string text)
    {
        statsText.text = text;
    }

    public void UpdateTooltip()
    {
        if (selectedTower != null)
        {
            sellText.text = "+ " + (selectedTower.Price / 2).ToString() + " $";
            SetTooltipText(selectedTower.GetStats());

            if (selectedTower.NextUpgrade != null)
            {
                upgradePrice.text = selectedTower.NextUpgrade.Price.ToString() + " $";
            }
            else
            {
                upgradePrice.text = string.Empty;
            }
        }
    }

    public void ShowSelectedTowerStats()
    {
        statsPanel.SetActive(!statsPanel.activeSelf);
        UpdateTooltip();
    }

    public void UpgradeTower()
    {
        if (selectedTower != null)
        {
            if (selectedTower.Level <= selectedTower.Upgrades.Length && Currency >= selectedTower.NextUpgrade.Price)
            {
                selectedTower.Upgrade();
            }
        }
    }
}
