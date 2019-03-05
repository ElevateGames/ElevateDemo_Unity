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
        StartCoroutine(GetUserInfo());
        // NewGetUserInfo();
        if (LoginSuccessCallback != null)
        {
            LoginSuccessCallback();
        }
    }

    public void Login()
    {
        StartCoroutine(TestLogin());
    }

    IEnumerator TestLogin()
    {
        WWWForm form = new WWWForm();
        string url = "http://emptyxu.f3322.net:1998/auth/passwordLogin";
        Debug.Log("send url: " + url);

        form.AddField("phoneNum", "13510576774");
        form.AddField("password", "cbd17e152f33588a9f372a396737b5f6");
        form.AddField("countryCode", "+86");
        form.AddField("abbreviation", "CN");

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return www.SendWebRequest();

            Dictionary<string, string> headers = www.GetResponseHeaders();
            foreach(var h in headers)
            {
                Debug.Log($"Key: {h.Key}, Value: {h.Value}");
            }

            if (www.isNetworkError || www.isHttpError)
            {
                // Debug.Log(www.error);
            }
            else
            {
                // Debug.Log("user info:" + www.downloadHandler.text);
            }

        }

        form = new WWWForm();
        url = "http://emptyxu.f3322.net:1998/mainapp/user/auth/getUpUserDto";

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("user info:" + www.downloadHandler.text);
            }
        }
    }

    IEnumerator GetUserInfo()
    {
        WWWForm form = new WWWForm();
        string url = mCookies.url + "mainapp/user/auth/getUpUserDto";
        Debug.Log("send url: " + url);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            StringBuilder stringbuilder = new StringBuilder();
            for (int i = 0; i < mCookies.data.Count; i++)
            {
                stringbuilder.Append(mCookies.data[i]);
                Debug.Log("Cookie data: " + mCookies.data[i]);
                if (i != mCookies.data.Count - 1)
                {
                    stringbuilder.Append(',');
                } 
            }
            String content = stringbuilder.ToString();
            Debug.Log("cookies content: " + content);
            SaveData.Instance.DebugText.text = content;
            www.SetRequestHeader("Set-Cookie", content);
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("user info:" + www.downloadHandler.text);
            }
        }
    }

    private async Task NewGetUserInfo()
    {
        Debug.Log("Enter newGetUserInfo");
        using (HttpClientHandler handler = new HttpClientHandler())
        {
            using (HttpClient httpClient = new HttpClient(handler))
            {
                for (int i = 0; i < mCookies.data.Count; i++)
                {
                    string s = mCookies.data[i];
                    httpClient.DefaultRequestHeaders.Add("Cookie", s);
                }
                httpClient.Timeout = System.TimeSpan.FromSeconds(3);

                await Task.Yield();
                Dictionary<string, string> param = new Dictionary<string, string>();
                string url = mCookies.url + "mainapp/user/auth/getUpUserDto";

                HttpResponseMessage response = await httpClient.PostAsync(url, new FormUrlEncodedContent(param));

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Debug.Log("Get userInfo: " + result);
                }
                else
                {
                    Debug.Log("Get userInfo Failed");
                    return;
                }
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