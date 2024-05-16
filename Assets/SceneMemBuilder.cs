using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SceneMemBuilder : MonoBehaviour
{
    //account creation
    public TMP_InputField emailInput;
    public TMP_InputField emailInputConfirm;
    public TMP_InputField passwordInput;
    public TMP_InputField passwordConfirm;

    public string StoreEmail;
    public string StoreHashedPass;


    //account login
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPassInput;

    public NetworkClient client;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void TryCreateAccount()
    {
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

            Debug.Log("passwords dont match");
            return;
        }

        


        bool shouldsend = true;

        //check that email is an email
        if (!Utility.IsValidEmail(emailInput.text) && emailInput.text != "")
        {
            //not an email
            Debug.Log(emailInput.text + " is not a valid email");
            
            shouldsend = false;

        }
        else
        {
            if (emailInput.text == "")
            {
                Debug.Log("No email in input");
            }
        }

        

        if (passwordInput.text.Length == 0)
        {
            //password too short
           
            Debug.Log("passowrd too short");
            shouldsend = false;
        }

        

        if (!shouldsend)
        {
            emailInput.interactable = true;
            emailInputConfirm.interactable = true;
            passwordInput.interactable = true;
            passwordConfirm.interactable = true;
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

}
