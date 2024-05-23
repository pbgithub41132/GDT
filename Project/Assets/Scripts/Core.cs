using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Core : MonoBehaviour
{
	//
	// Constants
	//
    const int   DEFAULT_NUM_FRUITS = 10;
    const int   DEFAULT_NUM_KINDS  = 4;
    const int   DEFAULT_IS_MUTED   = 0;    // Use int instead of bool due to Unity limitations https://docs.unity3d.com/2021.2/Documentation/ScriptReference/PlayerPrefs.html   
    const float SPAWN_TIMER        = 3;
	const float SPAWN_X            = 10;
	const float SPAWN_Y            = 0;
	const float SPAWN_Z            = 0;
	const float TARGET_X           = -10;
	const float TARGET_Y           = 0;
	const float TARGET_Z           = 0;
	//
	// Public variables
	//    
    public Sprite          SpriteUnmute;
    public Sprite          SpriteMute;
	public Sprite          SpritePlay;
	public Sprite          SpritePause;
	public Sprite[]        aSpriteFruits;
	public AudioSource     ASAudio;
	public Button          ButtonOptions;
	public Button          ButtonPlay;
	public Button          ButtonSound;
    public Slider          SliderResult;
    public Slider          SliderNumFruits;
    public Slider          SliderNumKinds;
    public TextMeshProUGUI TMPNumFruits;
    public TextMeshProUGUI TMPNumKinds;
    public TextMeshProUGUI TMPResult;
    public TextMeshProUGUI TMPGameEnd;
    public GameObject      GOFruit;
	public GameObject      GOConfirmButton;		
    public GameObject      GOOptionsMenu;
	public GameObject      GOResultSlider;
	public GameObject      GOPlayAgainButton;
	public GameObject      GOGameEndOverlay;
	//
	// Private variables
	//
	private int        NumFruitsToSpawn;
	private int        NumKinds;
	private int        IdKind;
	private int        NumFruitsSpawnedId;
	private int        NumFruitsSpawnedTotal;
    private float      fSpawnTimer;
	private float      fSpeed; 
	private bool	   IsPaused;
    private bool       IsMuted;   
	private bool       IsLastFruitSpawned;    
    private bool       IsGameOver;  
    private Vector3    V3Spawn;
    private Vector3    V3Target;
    private GameObject GOCurrentFruit;
	//
	// Public functions
	//
    public void OnSliderValueChanged() {
        //
        // Update PlayerPrefs and slider texts
        //
        PlayerPrefs.SetInt("NumFruits", (int)SliderNumFruits.value);
        PlayerPrefs.SetInt("NumKinds" , (int)SliderNumKinds.value );
        TMPNumFruits.SetText("" + SliderNumFruits.value);
        TMPNumKinds.SetText(""  + SliderNumKinds.value );  
        TMPResult.SetText(""    + SliderResult.value   );      
    } 
    
	public void OnClickPlayButton () {
        if (GOOptionsMenu.activeInHierarchy) { // Menu active? => Deactivate
            GOOptionsMenu.SetActive(false);
        }
		TogglePause();        
	}
    
	public void OnClickOptionsButton () {  
        if (GOOptionsMenu.activeInHierarchy) { // Menu active?      => Deactivate and resume
            GOOptionsMenu.SetActive(false);
            Resume();
        } else {                               // Menu deactivated? => Activate and pause
            GOOptionsMenu.SetActive(true);  
            Pause();
        }
	}
    
	public void OnClickRestartButton() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    } 
		
	public void OnClickSoundButton() {
        IsMuted = IsMuted ? false : true;
        PlayerPrefs.SetInt("IsMuted", IsMuted ? 1 : 0); // Use int instead of bool due to Unity limitations https://docs.unity3d.com/2021.2/Documentation/ScriptReference/PlayerPrefs.html   
        UpdateSound();
    } 
    
	public void OnClickExitButton() {
        Application.Quit();
    }
    
	public void OnClickConfirmButton() {
        string s0;
        string s1;
		// 
		// Adjust game end overlay
		//
		GOResultSlider.SetActive(false);
        GOConfirmButton.SetActive(false);
        GOPlayAgainButton.SetActive(true);    
		//
		// Evaluate input and write response
		//
        s0 = NumFruitsSpawnedId == SliderResult.value ? "Richtig" : "Falsch";
        s1 = NumFruitsSpawnedId == 1                  ? "war " : "waren ";
        TMPGameEnd.SetText(s0 + ", es " + s1 + NumFruitsSpawnedId + " <sprite name=\"Fruit" + IdKind + "\">");
	}	
    //
    // Private functions
    //
	private void Resume() {
		Time.timeScale = 1f;
		IsPaused = false;
		ButtonPlay.image.sprite = SpritePause;
	}
	
	private void Pause()	{
		Time.timeScale = 0f;
		IsPaused = true;
		ButtonPlay.image.sprite = SpritePlay;
	}
		
	private void TogglePause() {
        if (IsPaused) {
			Resume();
        } else {
			Pause();
        }
    }
	
    private void UpdateSound() {
        if (IsMuted) {
            ASAudio.Stop();
            ButtonSound.image.sprite = SpriteUnmute;
        } else {
            ASAudio.Play();
            ButtonSound.image.sprite = SpriteMute;   
        }        
    }    
    
	private void Start() {
		IsGameOver            = false;
		IsLastFruitSpawned    = false;           
		NumFruitsSpawnedId    = 0;
        NumFruitsSpawnedTotal = 0;           
        fSpawnTimer           = SPAWN_TIMER; 
        V3Spawn               = new Vector3( SPAWN_X,  SPAWN_Y,  SPAWN_Z);
        V3Target              = new Vector3(TARGET_X, TARGET_Y, TARGET_Z);   
        IsMuted               = PlayerPrefs.GetInt("IsMuted",   DEFAULT_IS_MUTED) == 0 ? false : true;
		NumFruitsToSpawn      = PlayerPrefs.GetInt("NumFruits", DEFAULT_NUM_FRUITS);	
		NumKinds              = PlayerPrefs.GetInt("NumKinds",  DEFAULT_NUM_KINDS );
        SliderResult.maxValue = NumFruitsToSpawn;
        SliderNumFruits.value = NumFruitsToSpawn;   
        SliderNumKinds.value  = NumKinds;
        IdKind                = Random.Range(0, NumKinds);
        UpdateSound();
        Resume();
	}

    private void Update() {
        //
        // Local variables
        //
        int Id;
        
		if (IsGameOver) {                                                         // Game is over? => skip updating
			return;
		}
        if (!IsLastFruitSpawned && fSpawnTimer <= 0) {                            // Not all fruits have been spawned yet and spawn timer is up? => Spawn fruit
            //
            // Randomly select kind of fruit to spawn
            //
            Id = Random.Range(0, NumKinds);
            //
            // Set sprite
            //
            GOFruit.GetComponent<SpriteRenderer>().sprite = aSpriteFruits[Id];
            //
            // Spawn the Fruit
            //
            GOCurrentFruit = Instantiate(GOFruit, V3Spawn, transform.rotation);
			//
            // Track number of fruits spawned
            //
            NumFruitsSpawnedTotal += 1;
            if (Id == IdKind) {
                NumFruitsSpawnedId += 1;
            }                
            if (NumFruitsSpawnedTotal == NumFruitsToSpawn) {                       // Last Fruit has been spawned? => Spawn no more fruits and prepare for game end
                IsLastFruitSpawned = true;
            }
            //
            // Reset spawn timer
            //
            fSpawnTimer = SPAWN_TIMER;
        }
		if (IsLastFruitSpawned && GOCurrentFruit.transform.position == V3Target) { // Last fruit reached the target? => Adjust GUI, early out Update()
            ButtonPlay.interactable    = false;
            ButtonOptions.interactable = false;
            TMPGameEnd.SetText("Wie viele <sprite name=\"Fruit" + IdKind + "\"> hast du gezählt?");
            GOGameEndOverlay.SetActive(true); 			
			//
			// Update() is no longer required, enable early out
			//
			IsGameOver = true;
			return;
        }
        //
        // Count down spawn timer
        //
        fSpawnTimer -= Time.deltaTime;               
    } 
}
