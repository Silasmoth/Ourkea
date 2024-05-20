using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProjectEntry : MonoBehaviour
{

    //references to UI elements
    public TextMeshProUGUI projectNameUI;
    public TextMeshProUGUI clientNameUI;
    public TextMeshProUGUI clientEmailUI;
    public Button viewButton;
    public Button acceptButton;
    public Button declineButton;
    public Button deleteButton;

    //project Data
    byte[] modelData;
    public bool accepted = false;
    string projectName;
    string clientName;
    string clientEmail;

    public void SetText(string _project, string _client, string _email)
    {
        projectName = _project;
        clientName =  _client;
        clientEmail =  _email;

        setAccepted(accepted);
    }

    public void SetData(byte[] _data)
    {
        modelData = _data;
    }

    public void setAccepted(bool _accepted)
    {
        accepted = _accepted;
        if (accepted)
        {
            acceptButton.gameObject.SetActive(false);
            declineButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(true);

            projectNameUI.text = "Project: " + projectName;
            clientNameUI.text = "Client name: " + clientName;
            clientEmailUI.text = "Client email: " + clientEmail;
        }
        else
        {
            acceptButton.gameObject.SetActive(true);
            declineButton.gameObject.SetActive(true);
            deleteButton.gameObject.SetActive(false);

            projectNameUI.text = "*NEW* Project: " + projectName;
            clientNameUI.text = "Client name: " + clientName;
            clientEmailUI.text = "Client email: " + clientEmail;
        }
    }
}
