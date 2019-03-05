using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveData : MonoBehaviour
{
    void Start()
    {
        ElevateSDK.Instance.LoginSuccessCallback += LoginSuccessCallback;
        ElevateSDK.Instance.PurchaseSuccessCallback += PurchaseSuccessCallback;
        ElevateSDK.Instance.OrderInfoCallback += OrderInfoCallback;
        ElevateSDK.Instance.QueryByGoodsNoCallback += QueryByGoodsNoCallback;
    }

    private void LoginSuccessCallback()
    {
        Debug.Log("Login finished!");
        ElevateSDK.Instance.QueryByGoodsNo("0");
    }

    private void PurchaseSuccessCallback(string orderNo)
    {
        //orderNo can be modified to goodsNo, 
    }

    private void QueryByGoodsNoCallback(bool state, string goodsNo)
    {
        if (state)
        {
            //TODO Logic of non-consumed goods purchased.
            // if(goodsNo.Equals("0")){
            // }
        }
    }

    private void OrderInfoCallback(bool orderNo)
    {
        Debug.Log("Can use this product!");
    }
}
