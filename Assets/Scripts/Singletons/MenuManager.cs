using System;
using System.Collections.Generic;
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
    [SerializeField] private Transform levelSceneParent;
    [SerializeField] private GameObject levelScenePrefab;
    [SerializeField] private TextMeshProUGUI levelSceneDescription;
    private int levelScene_SelectedIndex = 0;
    private Level levelScene_Selected => levelScenes[levelScene_SelectedIndex];
    public event Action<int> OnLevelSceneSelectedChange;

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
        currentTab = tab;
        
        playTab.enabled = tab == MenuTab.PLAY;
        //bindingsTab.enabled = tab == MenuTab.KEY_BINDINGS;
        //optionsTab.enabled = tab == MenuTab.OPTIONS;
        //achievementsTab.enabled = tab == MenuTab.ACHIEVEMENTS;
        //creditsTab.enabled = tab == MenuTab.CREDITS;
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
        SetCurrentTab(MenuTab.PLAY);
    }
    
    public void SetCurrentTab_Options()
    {
        SetCurrentTab(MenuTab.PLAY);
    }
    
    public void SetCurrentTab_Achievements()
    {
        SetCurrentTab(MenuTab.PLAY);
    }
    
    public void SetCurrentTab_Credits()
    {
        SetCurrentTab(MenuTab.PLAY);
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
}
