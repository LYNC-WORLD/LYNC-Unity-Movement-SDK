using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LYNC;
using UnityEngine.UI;
using LYNC.Wallet;
using UnityEngine.EventSystems;

public class APTOSExample : MonoBehaviour
{
    public TMP_Text publicKey, privateKey, loginDateTxt, balance;
    public Button login, logout, mint;
    public string loginUrl = "http://localhost:5173/test";
    public string backendUrl = "http://localhost:5000";
    public Transform transactionResultsParent;
    public GameObject transactionResultHolder;

    private WalletData walletData = new WalletData();

    public static APTOSExample Instance;

    [Space]
    [Header("Transactions")]
    public CustomTransaction mintTxn;

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
    }

    private async void LyncReady(LyncManager Lync)
    {
        try
        {
            walletData = await WalletData.TryLoadSavedWallet();
            if (walletData.WalletConnected)
            {
                login.interactable = false;
                logout.interactable = true;
                mint.interactable = true;
                Populate(walletData);
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
                login.interactable = false;
                logout.interactable = true;
                mint.interactable = true;
                Populate(wallet);
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

            try
            {
                TransactionData txData = await LyncManager.Instance.TransactionsManager.SendTransaction(TRANSACTIONS.FUND);
                SuccessfullTransaction(txData.data.transactionHash, "FUND");
                await walletData.GetBalance();
                Populate(walletData);
            }
            catch (System.Exception e)
            {
                ErrorTransaction(e.Message, "FUND");
                Debug.Log(e);
            }

            try
            {
                TransactionData txData = await LyncManager.Instance.TransactionsManager.SendTransaction(TRANSACTIONS.MINT, mintTxn);
                SuccessfullTransaction(txData.data.transactionHash, "MINT");
                await walletData.GetBalance();
                Populate(walletData);
            }
            catch (System.Exception e)
            {
                ErrorTransaction(e.Message, "MINT");
                Debug.Log(e);
            }

            try
            {
                TransactionData txData = await LyncManager.Instance.TransactionsManager.SendTransaction(TRANSACTIONS.REFUND);
                await walletData.GetBalance();
                SuccessfullTransaction(txData.data.transactionHash, "REFUND");
                Populate(walletData);
            }
            catch (System.Exception e)
            {
                ErrorTransaction(e.Message, "REFUND");
                Debug.Log(e);
            }

            mint.interactable = true;
        });
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

    public void Populate(WalletData walletData = null)
    {
        publicKey.text = "Public Key = " + (walletData == null ? "" : walletData.AptosWallet.publicKey.Substring(0, 20) + "...");
        privateKey.text = "Private Key = " + (walletData == null ? "" : walletData.AptosWallet.privateKey.Substring(0, 20) + "...");
        loginDateTxt.text = "Login Date = " + (walletData == null ? "" : walletData.loginDate.ToString());
        balance.text = "Balance = " + (walletData == null ? "00" : walletData.AptosWallet.balance) + " APT";
    }
}
