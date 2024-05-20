using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SceneMemBuilder : MonoBehaviour
{
    //different main manu screens
    public GameObject Startmenu;//the login or creat account screen
    public GameObject loginMenu;//the login, username pass screen
    public GameObject createAccountMenu;//the create new accound screen (email and pass etc)
    public GameObject VerifyEmailScreen;//the enter email verification code screen
    public GameObject AccountSettup;//the account setup with everything from name, address, workshop capabilities etc
    public GameObject BuilderHomePage;//the page with all of the builders current active and new projects

    //account creation
    public TMP_InputField emailInput;
    public TMP_InputField emailInputConfirm;
    public TMP_InputField passwordInput;
    public TMP_InputField passwordConfirm;
    public Button createAccountButton;//need to be set to un-interactable while we await response from server otherwise the user can sent many requests

    
    public string StoreEmail;//used to remember the email while verification takes place so that the user wont have to input it again later
    public string StoreHashedPass;//used to remember the password while verification takes place so that the user wont have to input it again later


    //account login
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPassInput;

    //email verification
    public TMP_InputField VerifyInput;
    public Button VerifyButton;
    public Button ResendVerifyButton;

    //account settings
    public TMP_InputField BuilderName;
    public TMP_InputField BuilderAddress;
    public TMP_InputField BuilderRange;
    public TMP_InputField BuilderMaxDim1;
    public TMP_InputField BuilderMaxDim2;
    public Toggle[] BuilderMaterialPreferences;
    public Button AccountInfoButton;


    public NetworkClient client;

    //for popups
    bool awaitingpopup;
    public GameObject PopUpPannel;
    public TextMeshProUGUI PopupText;

    //For builder main menu
    public GameObject noProjectsText;
    public ProjectEntry[] allProjects;
    public GameObject projectEntryPrefab;
    public GameObject listParent;

    private void Start()
    {
        GoToStartmenu();
        closePopup();
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    #region Popup
    public void showPopup(string PopupMessage)
    {
        awaitingpopup = true;
        PopUpPannel.SetActive(true);
        PopupText.text = PopupMessage;
    }

    public void closePopup()
    {
        awaitingpopup = false;
        PopUpPannel.SetActive(false);
    }
    #endregion

    #region account creation/login
    public void TryCreateAccount()
    {
        createAccountButton.interactable = false;
        emailInput.interactable = false;
        emailInputConfirm.interactable = false;
        passwordInput.interactable = false;
        passwordConfirm.interactable = false;

        //check to see if emails match
        if (emailInput.text != emailInputConfirm.text)
        {
            //they dont match

            emailInput.interactable = true;
            emailInputConfirm.interactable = true;
            passwordInput.interactable = true;
            passwordConfirm.interactable = true;
            createAccountButton.interactable = true;
            showPopup("emails do not match");
            Debug.Log("emails dont match");
            return;
        }

        //check to see if passwords match
        if (passwordInput.text != passwordConfirm.text)
        {
            //they dont match

            emailInput.interactable = true;
            emailInputConfirm.interactable = true;
            passwordInput.interactable = true;
            passwordConfirm.interactable = true;
            createAccountButton.interactable = true;
            showPopup("passwords do not match");
            Debug.Log("passwords dont match");
            return;
        }

        


        bool shouldsend = true;

        //check that email is an email
        if (!Utility.IsValidEmail(emailInput.text) && emailInput.text != "")
        {
            //not an email
            showPopup("please enter a valid email");
            Debug.Log(emailInput.text + " is not a valid email");
            
            shouldsend = false;

        }
        else
        {
            if (emailInput.text == "")
            {
                showPopup("please enter a valid email");
                Debug.Log("No email in input");
                shouldsend = false;
            }
        }

        

        if (passwordInput.text.Length == 0)
        {
            //password too short
            showPopup("please enter a valid password");
            Debug.Log("passowrd too short");
            shouldsend = false;
        }

        

        if (!shouldsend)
        {
            emailInput.interactable = true;
            emailInputConfirm.interactable = true;
            passwordInput.interactable = true;
            passwordConfirm.interactable = true;
            createAccountButton.interactable = true;
            return;
        }
        else
        {
            
            StoreEmail = emailInput.text;
            StoreHashedPass = Utility.GetHashString(passwordInput.text);
        }
        Net_CreateAccount msg = new Net_CreateAccount();

        msg.Email = emailInput.text;
        msg.Password = StoreHashedPass;
        

        Debug.Log("sending create account request");
        client.SendServer(msg);
    }

    public void EmailInUse()
    {
        emailInput.interactable = true;
        emailInputConfirm.interactable = true;
        passwordInput.interactable = true;
        passwordConfirm.interactable = true;
        createAccountButton.interactable = true;
        showPopup("an account with that email already exists");

    }

    public void TryLogin()
    {
        loginEmailInput.interactable = false;
        loginPassInput.interactable = false;

        StoreEmail = loginEmailInput.text;
        StoreHashedPass = Utility.GetHashString(loginPassInput.text);

        Net_LoginAtempt msg = new Net_LoginAtempt();

        msg.Email = StoreEmail;
        msg.HashedPass = StoreHashedPass;

        client.SendServer(msg);
    }

    public void LoginFail(string message)
    {
        loginEmailInput.interactable = true;
        loginPassInput.interactable = true;

        showPopup(message);
    }

    public void TryVerifyEmail()
    {
        //first lets just make sure that the input is a 4 digit number
        if (VerifyInput.text.Length != 4)
        {
            //not a 4 digit input, send popup
            showPopup("the verification code must be 4 digits");
            return;
        }
        Net_VerifyEmail msg = new Net_VerifyEmail();
        int code = 0;
        try
        {
            code = int.Parse(VerifyInput.text);
        }
        catch (System.Exception)
        {
            showPopup("the verification code only contains numbers");
            return;
            throw;
        }
        VerifyButton.interactable = false;
        ResendVerifyButton.interactable = false;
        msg.Code = code;
        msg.email = StoreEmail;
        msg.HashedPass = StoreHashedPass;
        client.SendServer(msg);

    }


    public void ResendVerify()
    {
        VerifyButton.interactable = false;
        ResendVerifyButton.interactable = false;
        Net_GenerigMessage msg = new Net_GenerigMessage();
        msg.MessageId = 0;
        client.SendServer(msg);
    }

    public void TrySendAccountInfo()//send all of the builders preferences and capabilities to the server
    {
        //should probably do some checks to make sure the fields all have inputs
        if (BuilderName.text == "")
        {
            showPopup("please enter your name, or your companies name");
            return;
        }

        if (BuilderAddress.text == "")
        {
            showPopup("please enter an address");
            return;
        }




        Net_BuilderInfo msg = new Net_BuilderInfo();

        //first lets assign the string values because they're easy
        msg.address = BuilderAddress.text;
        msg.name = BuilderName.text;

        //then lets do the toggle values
        bool[] matprefs = new bool[BuilderMaterialPreferences.Length];
        for (int i = 0; i < matprefs.Length; i++)
        {
            if (BuilderMaterialPreferences[i] != null)
            {
                matprefs[i] = BuilderMaterialPreferences[i].isOn;
            }
        }
        msg.materialpreferences = matprefs;

        //finally lets do the numbers cause here things can go wrong if not formatted correctly

        float range;
        float dim1;
        float dim2;
        try
        {
            range = float.Parse(BuilderRange.text);
            dim1 = float.Parse(BuilderMaxDim1.text);
            dim2 = float.Parse(BuilderMaxDim2.text);
        }
        catch (System.Exception)
        {
            //one of the numbers was not able to be parsed - tell the builder that they can only input a number in some fields
            showPopup("service range, and maximum distances can only contain numbers");
            return;
            throw;
        }

        msg.serviceRange = range;
        msg.maxDim1 = dim1;
        msg.maxDim2 = dim2;

        //thats everything, go ahead and send to server then wait for response
        AccountInfoButton.interactable = false;//so that the builder can only send one at a time
        client.SendServer(msg);
    }
    #endregion

    #region screen navigation
    public void GoToLogin()
    {
        //go to the login screen
        loginMenu.SetActive(true);
        Startmenu.SetActive(false);
        createAccountMenu.SetActive(false);
        VerifyEmailScreen.SetActive(false);
        AccountSettup.SetActive(false);
        BuilderHomePage.SetActive(false);
    }

    public void GoToCreateAccount()
    {
        //go to the create account screen
        loginMenu.SetActive(false);
        Startmenu.SetActive(false);
        createAccountMenu.SetActive(true);
        VerifyEmailScreen.SetActive(false);
        AccountSettup.SetActive(false);
        BuilderHomePage.SetActive(false);
    }

    public void GoToVerifyEmailScreen()
    {
        //go to the "enter email verification code" screen
        loginMenu.SetActive(false);
        Startmenu.SetActive(false);
        createAccountMenu.SetActive(false);
        VerifyEmailScreen.SetActive(true);
        AccountSettup.SetActive(false);
        BuilderHomePage.SetActive(false);

        VerifyButton.interactable = true;
        ResendVerifyButton.interactable = true;
    }

    public void GoToStartmenu()
    {
        //go to the initial login or create account screen
        loginMenu.SetActive(false);
        Startmenu.SetActive(true);
        createAccountMenu.SetActive(false);
        VerifyEmailScreen.SetActive(false);
        AccountSettup.SetActive(false);
        BuilderHomePage.SetActive(false);
    }

    public void GoToAccountInfoSettup()
    {
        //go to the screen with all of the workshop info and capabilities
        loginMenu.SetActive(false);
        Startmenu.SetActive(false);
        createAccountMenu.SetActive(false);
        VerifyEmailScreen.SetActive(false);
        AccountSettup.SetActive(true);
        BuilderHomePage.SetActive(false);
    }


    public void GoToBuilderHomePge()
    {
        //go to the screen with all of the builders current projects list
        loginMenu.SetActive(false);
        Startmenu.SetActive(false);
        createAccountMenu.SetActive(false);
        VerifyEmailScreen.SetActive(false);
        AccountSettup.SetActive(false);
        BuilderHomePage.SetActive(true);
    }
    #endregion



   
}
