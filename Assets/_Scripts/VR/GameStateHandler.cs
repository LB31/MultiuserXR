using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameStateHandler : Singleton<GameStateHandler>
{
    public CharacterController characterController;
    public ContinuousMovement continuousMovement;
    public DeviceBasedSnapTurnProvider snapTurnProvider;
    public PositionChanger positionChanger;

    public GameObject menuBox;
    private bool inMenu;

    public enum GameState
    {
        MoveFreely,
        ControlGirl,
        PortIntoCharacter,
        PassiveFirstPerson
    };
    public GameState StartState;

    private List<MonoBehaviour> allComponents = new List<MonoBehaviour>();

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        continuousMovement = GetComponent<ContinuousMovement>();
        snapTurnProvider = GetComponent<DeviceBasedSnapTurnProvider>();
        positionChanger = GetComponent<PositionChanger>();

        //allComponents.Add(characterController);
        allComponents.Add(continuousMovement);
        allComponents.Add(snapTurnProvider);
        allComponents.Add(positionChanger);

        DisableAllComponents();

        switch (StartState)
        {
            case GameState.MoveFreely:
                MoveFreely();
                break;
            case GameState.ControlGirl:
                ControlGirl();
                break;
            case GameState.PortIntoCharacter:
                PortIntoCharacter();
                break;
            case GameState.PassiveFirstPerson:
                PassiveFirstPerson();
                break;
            default:
                break;
        }
    }

    public void RigsterMenu()
    {
        XRControls.Instance.ControllerEventMenu -= Menu;
        XRControls.Instance.ControllerEventMenu += Menu;

        menuBox = FindObjectOfType<MenuController>().gameObject;
        menuBox.SetActive(false);
    }

    public void DisableAllComponents(bool snapTurn = true)
    {
        foreach (MonoBehaviour component in allComponents)
        {
            component.enabled = false;
        }
        characterController.enabled = false;
        snapTurnProvider.enabled = snapTurn;
    }

    public void MoveFreely()
    {
        DisableAllComponents();

        characterController.enabled = true;
        continuousMovement.enabled = true;
    }

    public void ControlGirl()
    {
        DisableAllComponents();

        positionChanger.enabled = true;
    }

    public void PortIntoCharacter()
    {
        DisableAllComponents();

        positionChanger.enabled = true;
    }

    public void PassiveFirstPerson()
    {
        DisableAllComponents();
        // TODO
    }

    public void Menu()
    {
        inMenu = !inMenu;
        if (inMenu)
        {
            Time.timeScale = 0;
            menuBox.SetActive(true);
            XRControls.Instance.VRInteractionRays.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            menuBox.SetActive(false);
            //XRControls.Instance.VRInteractionRays.SetActive(false);
        }
    }



}
