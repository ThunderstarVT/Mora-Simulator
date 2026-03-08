using System;
using System.Collections.Generic;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private static MenuManager instance;
    
    public static MenuManager Instance
    {
        get
        {
            if (instance == null) Debug.LogError("[Menu Manager]: Menu manager does not exist.");
            return instance;
        }
    }
    
    private InputSystem_Actions inputActions;
    
    private void Awake()
    {
        if (instance == null || instance == this)
        {
            instance = this;
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        inputActions = new InputSystem_Actions();
        
        inputActions.UI.Enable();
    }

    private enum CurrentMenu
    {
        TITLE_SCREEN,
        MAIN_MENU,
        PAUSE_MENU,
        NONE
    }

    private enum MenuTab
    {
        PLAY,
        KEY_BINDINGS,
        OPTIONS,
        ACHIEVEMENTS,
        CREDITS,
        NONE
    }
    
    private bool initialized = false;
    
    [SerializeField] private CurrentMenu currentMenu;
    [SerializeField] private MenuTab currentTab;

    [Header("Menus")]
    [SerializeField] private Canvas titleScreen;
    [SerializeField] private Canvas mainMenu;
    [SerializeField] private Canvas pauseMenu;
    
    [Header("Menu Tabs")]
    [SerializeField] private Canvas playTab;
    [SerializeField] private Canvas bindingsTab;
    [SerializeField] private Canvas optionsTab;
    [SerializeField] private Canvas achievementsTab;
    [SerializeField] private Canvas creditsTab;
    
    [Header("Level Scenes")]
    [SerializeField] private List<Level> levelScenes;
    [SerializeField] private RectTransform levelSceneParent;
    [SerializeField] private GameObject levelScenePrefab;
    [SerializeField] private TextMeshProUGUI levelSceneDescription;
    private int levelScene_SelectedIndex = 0;
    private Level levelScene_Selected => levelScenes[levelScene_SelectedIndex];
    public event Action<int> OnLevelSceneSelectedChange;
    
    [Header("Options Tab Requirements")]
    [SerializeField] private TextMeshProUGUI mouseSenseX_Text;
    [SerializeField] private Slider mouseSenseX_Slider;
    [SerializeField] private TextMeshProUGUI mouseInvertX_Text;
    [SerializeField] private Toggle mouseInvertX_Toggle;
    [Space]
    [SerializeField] private TextMeshProUGUI mouseSenseY_Text;
    [SerializeField] private Slider mouseSenseY_Slider;
    [SerializeField] private TextMeshProUGUI mouseInvertY_Text;
    [SerializeField] private Toggle mouseInvertY_Toggle;
    [Space]
    [SerializeField] private TextMeshProUGUI mouseSenseZ_Text;
    [SerializeField] private Slider mouseSenseZ_Slider;
    [SerializeField] private TextMeshProUGUI mouseInvertZ_Text;
    [SerializeField] private Toggle mouseInvertZ_Toggle;
    [Space]
    [SerializeField] private TextMeshProUGUI sfxVolume_Text;
    [SerializeField] private Slider sfxVolume_Slider;
    [Space]
    [SerializeField] private TextMeshProUGUI musicVolume_Text;
    [SerializeField] private Slider musicVolume_Slider;
    [Space]
    [SerializeField] private TextMeshProUGUI voiceVolume_Text;
    [SerializeField] private Slider voiceVolume_Slider;
    [Space]
    [SerializeField] private TextMeshProUGUI buoyancyAccuracy_Text;
    [SerializeField] private TMP_Dropdown buoyancyAccuracy_Dropdown;
    [Space]
    [SerializeField] private TextMeshProUGUI particleCount_Text;
    [SerializeField] private TMP_Dropdown particleCount_Dropdown;

    [Serializable]
    private struct Level
    {
        [SerializeField] public string displayName;
        [SerializeField] public string sceneName;
        [SerializeField, TextArea] public string description;
    }
    
    private CameraController cameraController => Camera.main?.GetComponent<CameraController>();
    private PlayerInputManager playerInputManager => GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInputManager>();

    private void Start()
    {
        if (!initialized) Init();
    }

    private void Init()
    {
        // start on title screen
        SetCurrentMenu(CurrentMenu.TITLE_SCREEN);
        
        // start with camera being path follow (mode none for now as path follow is yet to be implemented)
        CameraController cc = cameraController;
        cc.setInputActive(false);
        cc.setCameraMode(CameraController.CameraMode.NONE);
        
        // start with player input disabled
        playerInputManager.setInputActive(false);
        
        // set input stuff
        inputActions.UI.AnyKey.canceled += OnAnyKeyPressed;
        inputActions.UI.Pause.canceled += OnPause;

        for (int i = 0; i < levelScenes.Count; i++)
        {
            GameObject go = Instantiate(levelScenePrefab, levelSceneParent);
            LevelListEntry entry = go.GetComponent<LevelListEntry>();
            int index = i;

            entry.SetText(levelScenes[i].displayName);
            entry.SetButtonOnClick(() => SetLevelSceneSelected(index));

            OnLevelSceneSelectedChange += lsi =>
            {
                entry.SetImageAlpha(lsi == index ? 1f : 0.5f);
            };

            Vector2 delta = levelSceneParent.sizeDelta;
            delta.y += ((RectTransform) go.transform).sizeDelta.y + 
                       levelSceneParent.GetComponent<VerticalLayoutGroup>().spacing;
            levelSceneParent.sizeDelta = delta;
        }
        
        SetLevelSceneSelected(0);
    }

    private void SetCurrentMenu(CurrentMenu menu)
    {
        currentMenu = menu;
        
        SetCurrentTab(MenuTab.NONE);
        
        titleScreen.enabled = menu == CurrentMenu.TITLE_SCREEN;
        mainMenu.enabled = menu == CurrentMenu.MAIN_MENU;
        pauseMenu.enabled = menu == CurrentMenu.PAUSE_MENU;

        if (menu == CurrentMenu.NONE)
        {
            playerInputManager.setInputActive(true);
            cameraController.setInputActive(true);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            playerInputManager.setInputActive(false);
            cameraController.setInputActive(false);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void SetCurrentTab(MenuTab tab)
    {
        if (currentTab == MenuTab.OPTIONS && SettingsManager.Instance.UnsavedChanges)
        {
            // TODO: show warning thingy
            
            return;
        }
        
        currentTab = tab;
        
        playTab.enabled = tab == MenuTab.PLAY;
        //bindingsTab.enabled = tab == MenuTab.KEY_BINDINGS;
        optionsTab.enabled = tab == MenuTab.OPTIONS;
        //achievementsTab.enabled = tab == MenuTab.ACHIEVEMENTS;
        creditsTab.enabled = tab == MenuTab.CREDITS;

        if (tab == MenuTab.OPTIONS)
        {
            UpdateOptionsText();
            UpdateOptionsSettings();
        }
    }

    private void UpdateOptionsText()
    {
        if (SettingsManager.Instance.MouseSenseXChanged) mouseSenseX_Text.fontStyle |= FontStyles.Italic;
        else mouseSenseX_Text.fontStyle &= ~FontStyles.Italic;
        if (SettingsManager.Instance.MouseInvertXChanged) mouseInvertX_Text.fontStyle |= FontStyles.Italic;
        else mouseInvertX_Text.fontStyle &= ~FontStyles.Italic;
        
        if (SettingsManager.Instance.MouseSenseYChanged) mouseSenseY_Text.fontStyle |= FontStyles.Italic;
        else mouseSenseY_Text.fontStyle &= ~FontStyles.Italic;
        if (SettingsManager.Instance.MouseInvertYChanged) mouseInvertY_Text.fontStyle |= FontStyles.Italic;
        else mouseInvertY_Text.fontStyle &= ~FontStyles.Italic;
        
        if (SettingsManager.Instance.MouseSenseZChanged) mouseSenseZ_Text.fontStyle |= FontStyles.Italic;
        else mouseSenseZ_Text.fontStyle &= ~FontStyles.Italic;
        if (SettingsManager.Instance.MouseInvertZChanged) mouseInvertZ_Text.fontStyle |= FontStyles.Italic;
        else mouseInvertZ_Text.fontStyle &= ~FontStyles.Italic;
        
        if (SettingsManager.Instance.SfxVolumeChanged) sfxVolume_Text.fontStyle |= FontStyles.Italic;
        else sfxVolume_Text.fontStyle &= ~FontStyles.Italic;
        
        if (SettingsManager.Instance.MusicVolumeChanged) musicVolume_Text.fontStyle |= FontStyles.Italic;
        else musicVolume_Text.fontStyle &= ~FontStyles.Italic;
        
        if (SettingsManager.Instance.VoiceVolumeChanged) voiceVolume_Text.fontStyle |= FontStyles.Italic;
        else voiceVolume_Text.fontStyle &= ~FontStyles.Italic;
        
        if (SettingsManager.Instance.BuoyancyAccuracyChanged) buoyancyAccuracy_Text.fontStyle |= FontStyles.Italic;
        else buoyancyAccuracy_Text.fontStyle &= ~FontStyles.Italic;
        
        if (SettingsManager.Instance.ParticleCountChanged) particleCount_Text.fontStyle |= FontStyles.Italic;
        else particleCount_Text.fontStyle &= ~FontStyles.Italic;
    }

    private void UpdateOptionsSettings()
    {
        mouseSenseX_Slider.value = SettingsManager.Instance.mouseSenseX;
        mouseInvertX_Toggle.isOn = SettingsManager.Instance.mouseInvertX;
        
        mouseSenseY_Slider.value = SettingsManager.Instance.mouseSenseY;
        mouseInvertY_Toggle.isOn = SettingsManager.Instance.mouseInvertY;
        
        mouseSenseZ_Slider.value = SettingsManager.Instance.mouseSenseZ;
        mouseInvertZ_Toggle.isOn = SettingsManager.Instance.mouseInvertY;
        
        sfxVolume_Slider.value = SettingsManager.Instance.sfxVolume;
        musicVolume_Slider.value = SettingsManager.Instance.musicVolume;
        voiceVolume_Slider.value = SettingsManager.Instance.voiceVolume;

        buoyancyAccuracy_Dropdown.value = SettingsManager.Instance.buoyancyAccuracy switch
        {
            SettingsManager.Options.POTATO => 0,
            SettingsManager.Options.LOW => 1,
            SettingsManager.Options.MID => 2,
            SettingsManager.Options.HIGH => 3,
            _ => 2
        };

        particleCount_Dropdown.value = SettingsManager.Instance.particleCount switch
        {
            SettingsManager.Options.POTATO => 0,
            SettingsManager.Options.LOW => 1,
            SettingsManager.Options.MID => 2,
            SettingsManager.Options.HIGH => 3,
            _ => 2
        };
    }

    private void OnAnyKeyPressed(InputAction.CallbackContext context)
    {
        if (currentMenu == CurrentMenu.TITLE_SCREEN)
        {
            SetCurrentMenu(CurrentMenu.MAIN_MENU);
        }
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        switch (currentMenu)
        {
            case CurrentMenu.PAUSE_MENU:
                SetCurrentMenu(CurrentMenu.NONE);
                break;
            
            case CurrentMenu.NONE:
                SetCurrentMenu(CurrentMenu.PAUSE_MENU);
                break;
            
            case CurrentMenu.TITLE_SCREEN:
            case CurrentMenu.MAIN_MENU:
            default:
                break;
        }
    }
    
    
    public void SetCurrentTab_Play()
    {
        SetCurrentTab(MenuTab.PLAY);

        SetLevelSceneSelected(0);
    }
    
    public void SetCurrentTab_KeyBindings()
    {
        SetCurrentTab(MenuTab.KEY_BINDINGS);
    }
    
    public void SetCurrentTab_Options()
    {
        SetCurrentTab(MenuTab.OPTIONS);
    }
    
    public void SetCurrentTab_Achievements()
    {
        SetCurrentTab(MenuTab.ACHIEVEMENTS);
    }
    
    public void SetCurrentTab_Credits()
    {
        SetCurrentTab(MenuTab.CREDITS);
    }


    public void SetLevelSceneSelected(int index)
    {
        levelScene_SelectedIndex = index;

        levelSceneDescription.text = levelScene_Selected.description;
        
        OnLevelSceneSelectedChange?.Invoke(levelScene_SelectedIndex);
    }


    public void StartLevel()
    {
        if (true)
        {
            SceneManager.LoadScene(levelScene_Selected.sceneName);
        }
        
        SetCurrentMenu(CurrentMenu.NONE);
        
        cameraController.setCameraMode(CameraController.CameraMode.ORBITAL);
    }


    public void OptionsApply()
    {
        SettingsManager.Instance.Apply();
        
        UpdateOptionsText();
        UpdateOptionsSettings();
    }

    public void OptionsRevert()
    {
        SettingsManager.Instance.Revert();
        
        UpdateOptionsText();
        UpdateOptionsSettings();
    }


    public void OptionsChange_MouseSenseX(float value)
    {
        SettingsManager.Instance.mouseSenseX = value;
        UpdateOptionsText();
    }

    public void OptionsChange_MouseInvertX(bool value)
    {
        SettingsManager.Instance.mouseInvertX = value;
        UpdateOptionsText();
    }

    public void OptionsChange_MouseSenseY(float value)
    {
        SettingsManager.Instance.mouseSenseY = value;
        UpdateOptionsText();
    }

    public void OptionsChange_MouseInvertY(bool value)
    {
        SettingsManager.Instance.mouseInvertY = value;
        UpdateOptionsText();
    }

    public void OptionsChange_MouseSenseZ(float value)
    {
        SettingsManager.Instance.mouseSenseZ = value;
        UpdateOptionsText();
    }

    public void OptionsChange_MouseInvertZ(bool value)
    {
        SettingsManager.Instance.mouseInvertZ = value;
        UpdateOptionsText();
    }
    

    public void OptionsChange_SfxVolume(float value)
    {
        SettingsManager.Instance.sfxVolume = value;
        UpdateOptionsText();
    }

    public void OptionsChange_MusicVolume(float value)
    {
        SettingsManager.Instance.musicVolume = value;
        UpdateOptionsText();
    }

    public void OptionsChange_VoiceVolume(float value)
    {
        SettingsManager.Instance.voiceVolume = value;
        UpdateOptionsText();
    }


    public void OptionsChange_BuoyancyAccuracy(int value)
    {
        SettingsManager.Instance.buoyancyAccuracy = value switch
        {
            0 => SettingsManager.Options.POTATO,
            1 => SettingsManager.Options.LOW,
            2 => SettingsManager.Options.MID,
            3 => SettingsManager.Options.HIGH,
            _ => SettingsManager.Options.MID
        };
        
        UpdateOptionsText();
    }

    public void OptionsChange_ParticleCount(int value)
    {
        SettingsManager.Instance.particleCount = value switch
        {
            0 => SettingsManager.Options.POTATO,
            1 => SettingsManager.Options.LOW,
            2 => SettingsManager.Options.MID,
            3 => SettingsManager.Options.HIGH,
            _ => SettingsManager.Options.MID
        };
        
        UpdateOptionsText();
    }
    
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
