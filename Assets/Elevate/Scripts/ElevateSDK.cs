using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ElevateSDK : MonoBehaviour
{
    public static ElevateSDK Instance;

    private static AndroidJavaClass jc;

    private static AndroidJavaObject jo;

    private const string APP_ID = "mammmfmmaakk";

    private ShareCookies mCookies;

    // Need to implement this callback, complete the order no save.
    public Action<string> PurchaseSuccessCallback { get; set; }
    public Action LoginSuccessCallback { get; set; }
    public Action<bool> OrderInfoCallback { get; set; }
    public Action<bool, string> QueryByGoodsNoCallback { get; set; }

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        GameObject elevateObject = new GameObject();
        elevateObject.name = "ElevateObject";
        Instance = elevateObject.AddComponent<ElevateSDK>();

        jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

        ElevateSDK.Instance.InitSDK(true);
    }

    /// <summary>
    /// Sdk initializes the interface, and the login interface is opened by default.
    /// </summary>
    /// <param name="isDev">Fill in "true" during development</param>
    private void InitSDK(bool isDev)
    {
        jo.Call("init", APP_ID, isDev);
    }

    /// <summary>
    /// Called when the login screen is opened again.
    /// </summary>
    public void OpenLogin()
    {
        jo.Call("openLogin");
    }

    /// <summary>
    /// In-app purchase interface.
    /// </summary>
    /// <param name="goodsNo">Goods no</param>
    /// <param name="count">Goods amount.Generally fill in "1"</param>
    public void Purchase(string goodsNo, int count)
    {
        jo.Call("purchase", goodsNo, count);
    }

    /// <summary>
    /// Order status after purchase to check order status.
    /// </summary>
    /// <param name="orderNo"></param>
    public void GetOrderInfo(string orderNo)
    {
        jo.Call("getOrderInfo", orderNo);
    }

    /// <summary>
    /// Check if the goods can be used by goodsNo.
    /// </summary>
    /// <param name="goodsNo"></param>
    public void QueryByGoodsNo(string goodsNo)
    {
        jo.Call("queryByGoodsNo", goodsNo);
    }

    #region Android call Unity
    public void QueryByGoodsNoResult(string goodsNo)
    {
        if (String.IsNullOrEmpty(goodsNo))
        {
            QueryByGoodsNoCallback(false, null);
        }
        else
        {
            QueryByGoodsNoCallback(true, goodsNo);
        }
    }

    public void PurchaseSuccess(string orderNo)
    {
        if (PurchaseSuccessCallback != null)
        {
            PurchaseSuccessCallback(orderNo);
        }
    }

    public void OrderInfoResult(string data)
    {
        if (OrderInfoCallback != null)
        {
            Debug.Log("Order data: " + data);
            OrderInfoCallback(true);
        }
    }

    public void LoginSuccess(string data)
    {
        mCookies = JsonUtility.FromJson<ShareCookies>(data);
        if (LoginSuccessCallback != null)
        {
            LoginSuccessCallback();
            StartCoroutine(TestLogin());
        }
    }

    IEnumerator TestLogin()
    {
        string url = mCookies.url + "auth/passwordLogin";
        WWWForm form = new WWWForm();
        form.AddField("phoneNum", "13510576774");
        form.AddField("password", "cbd17e152f33588a9f372a396737b5f6");
        form.AddField("countryCode", "+86");
        form.AddField("abbreviation", "CN");
        using(UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);

                foreach(var a in request.GetResponseHeaders())
                {
                    Debug.Log($"Key: {a.Key}, Value: {a.Value}");
                }

                StartCoroutine(getUserInfo());
            }
        }
    }

    IEnumerator getUserInfo()
    {
        string url = mCookies.url + "mainapp/user/auth/getUpUserDto";
        WWWForm form = new WWWForm();
        using(UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            // StringBuilder builder = new StringBuilder();
            // for (int i = 0; i < mCookies.data.Count; i++)
            // {
            //     builder.Append(mCookies.url);
            //     Debug.Log("Cookie data:" + mCookies.data[i]);
            //     if(i != mCookies.data.Count - 1)
            //     {
            //         builder.Append(',');
            //     }
            // }
            // request.SetRequestHeader("Set-Cookie", builder.ToString());

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }
    #endregion

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        PurchaseSuccessCallback = null;
        LoginSuccessCallback = null;
        OrderInfoCallback = null;
        QueryByGoodsNoCallback = null;
    }
}