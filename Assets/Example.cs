using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    public void OpenLogin()
    {
        ElevateSDK.Instance.OpenLogin();
    }

    public void Purchase()
    {
        ElevateSDK.Instance.Purchase("0", 1);
    }

    public void Login()
    {
        ElevateSDK.Instance.Login();
    }
}