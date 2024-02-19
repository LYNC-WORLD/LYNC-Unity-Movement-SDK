using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LYNC;
using UnityEngine.UI;
using LYNC.Wallet;
using UnityEngine.EventSystems;
using System;

public class APTOSExample : MonoBehaviour
{
    [Header("General settings")]
    public string loginUrl = "http://localhost:5173/test";
    public string backendUrl = "http://localhost:5000";
    public Button login, logout, mint;

    [Space]
    [Header("Aptos")]
    public Transform aptosContainer;
    public TMP_Text publicKey, privateKey, loginDateTxt, balance;

    [Space]
    [Header("Pontem")]
    public Transform pontemContainer;
    public TMP_Text pontemPublicAddress;

    [Space]
    [Header("Transactions")]
    public Transform transactionResultsParent;
    public GameObject transactionResultHolder;
    public Transaction mintTxn;

    private AuthBase authBase;
    public static APTOSExample Instance;

    private void OnEnable()
    {
        API.BackendUrl = backendUrl;
        LyncManager.onLyncReady += LyncReady;
    }

    private void Awake()
    {
        Instance = this;

        login.interactable = false;
        logout.interactable = false;
        mint.interactable = false;
        Application.targetFrameRate = 30;
    }

    private async void LyncReady(LyncManager Lync)
    {
        try
        {
            authBase = await AuthBase.LoadSavedAuth();
            if (authBase.WalletConnected)
            {
                Debug.Log("Saved wallet successfuly loaded");
                OnWalletConnected(authBase);
            }
            else
            {
                login.interactable = true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            logout.interactable = true;
        }

        login.onClick.AddListener(() =>
        {
            Lync.WalletAuth.ConnectWallet(loginUrl, (wallet) =>
            {
                Debug.Log(wallet.WalletConnected);
                Debug.Log(wallet.PublicAddress);
                OnWalletConnected(wallet);
            });
        });

        logout.onClick.AddListener(() =>
        {
            Lync.WalletAuth.Logout();
            login.interactable = true;
            logout.interactable = false;
            mint.interactable = false;
            Populate();
        });

        mint.onClick.AddListener(async () =>
        {
            mint.interactable = false;


            TransactionResult txData = await LyncManager.Instance.TransactionsManager.SendTransaction(mintTxn);
            Debug.Log(txData.hash);
            SuccessfullTransaction(txData.hash, "MINT");
            // await ((FirebaseAuth)authBase).AptosAuthData.UpdateBalance();
            // Populate((FirebaseAuth)authBase);


            // try
            // {
            //     // ServerBasedTransactionFeedback txData = await LyncManager.Instance.TransactionsManager.SendTransaction(TRANSACTIONS.MINT, mintTxn);
            //     // SuccessfullTransaction(txData.data.transactionHash, "MINT");
            //     await ((FirebaseAuth)authBase).AptosAuthData.UpdateBalance();
            //     Populate((FirebaseAuth)authBase);
            // }
            // catch (System.Exception e)
            // {
            //     ErrorTransaction(e.Message, "MINT");
            //     Debug.Log(e);
            // }

            // try
            // {
            //     // ServerBasedTransactionFeedback txData = await LyncManager.Instance.TransactionsManager.SendTransaction(TRANSACTIONS.REFUND);
            //     await ((FirebaseAuth)authBase).AptosAuthData.UpdateBalance();
            //     // SuccessfullTransaction(txData.data.transactionHash, "REFUND");
            //     Populate((FirebaseAuth)authBase);
            // }
            // catch (System.Exception e)
            // {
            //     ErrorTransaction(e.Message, "REFUND");
            //     Debug.Log(e);
            // }

            mint.interactable = true;
        });

    }

    private void OnWalletConnected(AuthBase _authBase)
    {
        EnableAppropriateComponents(AuthBase.AuthType);

        if (AuthBase.AuthType == AUTH_TYPE.FIREBASE)
        {
            Populate(_authBase as FirebaseAuth);
        }

        if (AuthBase.AuthType == AUTH_TYPE.PONTEM)
        {
            pontemPublicAddress.text = "Public address = " + _authBase.PublicAddress;
        }

        login.interactable = false;
        logout.interactable = true;
        mint.interactable = true;
    }

    private void EnableAppropriateComponents(AUTH_TYPE authType)
    {
        if (authType == AUTH_TYPE.FIREBASE)
        {
            aptosContainer.gameObject.SetActive(true);
            pontemContainer.gameObject.SetActive(false);
        }
        if (authType == AUTH_TYPE.PONTEM)
        {
            pontemContainer.gameObject.SetActive(true);
            aptosContainer.gameObject.SetActive(false);
        }
    }

    private void SuccessfullTransaction(string hash, string txnTitle = "")
    {
        var go = Instantiate(transactionResultHolder, transactionResultsParent);
        go.transform.GetComponentInChildren<TMP_Text>().text = (txnTitle != "" ? ("(" + txnTitle + ")") : "") + " Success, hash = " + hash.Substring(0, 5) + "..." + hash.Substring(hash.Length - 5) + "<color=\"green\"> Check on APTOS EXPLORER<color=\"green\">";

        EventTrigger trigger = go.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener((eventData) => { Application.OpenURL("https://explorer.aptoslabs.com/txn/" + hash + "?network=testnet"); });
        trigger.triggers.Add(entry);
    }

    private void ErrorTransaction(string error, string txnTitle = "")
    {
        var go = Instantiate(transactionResultHolder, transactionResultsParent);
        go.transform.GetComponentInChildren<TMP_Text>().text = txnTitle + " ERROR: " + error;
    }

    public void Populate(FirebaseAuth firebaseAuth = null)
    {
        publicKey.text = "Public Key = " + (firebaseAuth == null ? "" : firebaseAuth.AptosAuthData.publicKey.Substring(0, 20) + "...");
        privateKey.text = "Private Key = " + (firebaseAuth == null ? "" : firebaseAuth.AptosAuthData.privateKey.Substring(0, 20) + "...");
        loginDateTxt.text = "Login Date = " + (firebaseAuth == null ? "" : firebaseAuth.LoginDate.ToString());
        balance.text = "Balance = " + (firebaseAuth == null ? "00" : firebaseAuth.AptosAuthData.balance) + " APT";
    }
}
