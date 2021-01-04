using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameUI : MonoBehaviour {
    
    public Image fadePlane;
    public GameObject gameOverUI;
    public GameObject gameWinUI;
    public GameObject inGameHUD;
    public Text healthHUD;
    public Text ammoHUD;
    public Text ammoTotal;

    public GameObject newWaveUI;
    public Text nwTitle;
    public Text nwInfo;
    public Image nwCover;

    public GameObject crosshair;

    public Text timerText;

    float timeSinceStart;

    LivingEntity player;
    Spawner spawner;
    GunController gunController;
    Gun gun;

    bool pauseTimer;
    
    Color originalCoverColor;

    void Awake() {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
        player.UpdateHealth += UpdateHealth;
        spawner = FindObjectOfType<Spawner>();
        spawner.OnPlayerWin += OnGameWin;
        spawner.OnHealedPlayer += UpdateHealth;
        spawner.OnNewWave += NewWaveBanner;
        gunController = FindObjectOfType<GunController>();
        gunController.OnGunChange += UpdateGun;

        
        healthHUD.text = player.GetHealth().ToString();
        originalCoverColor = nwCover.color;
        StartTimer();
    }

    private void Update() {
        if ( !pauseTimer )
            UpdateTimer();
    }

    public void UpdateHealth () {
        StartCoroutine(HealthAnimate(1));
    }
    
    public void UpdateAmmo () {
        StartCoroutine(AmmoAnimate(0.2f));
    }

    public void UpdateGun() {
        gun = FindObjectOfType<Gun>();
        gun.UpdateAmmo += UpdateAmmo;
        ammoTotal.text = "/" + gun.ammoRemaining.ToString();
        StartCoroutine(AmmoAnimate(0.2f));
    }

    public void NewWaveBanner(int waveNumber) {
        nwTitle.text = "Wave " + (waveNumber+1);
        nwInfo.text = "";
        
        foreach ( var i in spawner.currentWave.enemies ) {
            
            switch ( i.count ) {
                case 0:
                    break;
                case 1:
                    nwInfo.text += i.count.ToString() + " " + i.enemy.name.ToString() + ". ";
                    break;
                default:
                    nwInfo.text += i.count.ToString() + " " + i.enemy.name.ToString() + "s. ";
                    break;
            }

        }
        
        StartCoroutine(NewWaveAnimate(0.5f, 2f, 0.5f, originalCoverColor)); //fade in, stay, fade out
    }

    IEnumerator HealthAnimate(float t) {
        float timePassed = 0;
        float percent;
        
        Color originalHealthColor = Color.white;
        int originalFontSize = 47;
        int enlargedFontSize = 82;
        float newPlayerHealth = player.GetHealth();

        if ( int.Parse(healthHUD.text) == newPlayerHealth )
            yield break;

        Color newColor = HealthAnimateColor(int.Parse(healthHUD.text), newPlayerHealth);

        if ( newPlayerHealth < 0 ) 
            healthHUD.text = "0";
        else
            healthHUD.text = newPlayerHealth.ToString();
        
        healthHUD.fontSize = enlargedFontSize;

        while (timePassed < t ) {
            percent = timePassed/t;
            timePassed += Time.deltaTime;
            healthHUD.fontSize = (int)Mathf.Lerp(enlargedFontSize, originalFontSize, percent);
            healthHUD.color = Color.Lerp(newColor, originalHealthColor, percent);
            yield return null;
        }

    }

    IEnumerator AmmoAnimate(float t) {
        float timePassed = 0;
        float percent;
        
        Color originalAmmoColor = Color.white;
        int originalFontSize = 47;
        int enlargedFontSize = 60;
        float newPlayerAmmo = gun.ammoRemaining;

        ammoHUD.text = gun.ammoRemaining.ToString();
        
        ammoHUD.fontSize = enlargedFontSize;

        while (timePassed < t ) {
            percent = timePassed/t;
            timePassed += Time.deltaTime;
            ammoHUD.fontSize = (int)Mathf.Lerp(enlargedFontSize, originalFontSize, percent);
            yield return null;
        }
    }

    IEnumerator NewWaveAnimate(float tin, float tstay, float tout, Color originalCoverColor) {
        newWaveUI.SetActive(true);
        
        //fade in
        float timePassed = 0;
        float percent;
        while (timePassed < tin ) {
            percent = timePassed/tin;
            timePassed += Time.deltaTime;
            nwCover.color = Color.Lerp(Color.clear, originalCoverColor, percent);
            nwTitle.color = Color.Lerp(Color.clear, Color.white, percent);
            nwInfo.color = Color.Lerp(Color.clear, Color.white, percent);
            yield return null;
        }

        //stay
        yield return new WaitForSeconds(tstay);

        //fade out
        timePassed = 0;
        while (timePassed < tin ) {
            percent = timePassed/tin;
            timePassed += Time.deltaTime;
            nwCover.color = Color.Lerp(originalCoverColor, Color.clear, percent);
            nwTitle.color = Color.Lerp(Color.white, Color.clear, percent);
            nwInfo.color = Color.Lerp(Color.white, Color.clear, percent);
            yield return null;
        }

        newWaveUI.SetActive(false);
    }

    Color HealthAnimateColor ( float oldHealth, float newHealth ) {
        if ( newHealth < oldHealth )
            return Color.red;
        else
            return Color.cyan;
    }

    void OnGameOver() {
        StartCoroutine( Fade(Color.clear, Color.black, 1) );
        Cursor.visible = true;
        gameOverUI.SetActive(true);
        healthHUD.color = Color.clear;
        ammoHUD.color = Color.clear;
        ammoTotal.color = Color.clear;
        crosshair.SetActive(false);
        pauseTimer = true;
    }

    void OnGameWin() {
        StartCoroutine( Fade(Color.clear, Color.black, 1) );
        Cursor.visible = true;
        gameWinUI.SetActive(true);
        healthHUD.color = Color.clear;
        ammoHUD.color = Color.clear;
        ammoTotal.color = Color.clear;
        crosshair.SetActive(false);
        pauseTimer = true;
    }

    IEnumerator Fade(Color from, Color to, float time) {
        float speed = 1/time;
        float percent = 0;

        while ( percent < 1 ) {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from,to,percent);
            yield return null;
        }
    }

    public void StartNewGame() {
        SceneManager.LoadScene("Game");
    }

    public void StartTimer() {
        timeSinceStart = 0f;
        pauseTimer = true;
    }

    public void UpdateTimer() {
        timeSinceStart += Time.deltaTime;
        
        int mins = (int) (timeSinceStart / 60);
        int seconds = (int)timeSinceStart % 60;

        timerText.text = mins.ToString() + ":" + seconds.ToString();
    }
}

