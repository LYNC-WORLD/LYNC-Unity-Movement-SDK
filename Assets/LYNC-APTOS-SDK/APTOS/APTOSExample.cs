using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LYNC;
using UnityEngine.UI;
using LYNC.Wallet;

public class APTOSExample : MonoBehaviour
{
    public TMP_Text publicKey, privateKey, loginDateTxt, balance, messageTxt;
    public Button login, logout, mint;
    public string loginUrl = "http://localhost:5173/test";
    public string backendUrl = "http://localhost:5000";

    private WalletData walletData = new WalletData();

    public static APTOSExample Instance;

    [Space]
    [Header("Transaction")]
    public CustomTransaction customTransaction;

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
                TransactionData txData = await LyncManager.Instance.TransactionsManager.SendTransaction(TRANSACTIONS.FUND, customTransaction);
                messageTxt.text += "\nFund success, hash = " + txData.data.transactionHash;
                await walletData.GetBalance();
                Populate(walletData);
            }
            catch (System.Exception e)
            {
                messageTxt.text += "Fund error: " + e.Message;
                Debug.Log(e);
            }

            try
            {
                TransactionData txData = await LyncManager.Instance.TransactionsManager.SendTransaction(TRANSACTIONS.MINT, customTransaction);
                messageTxt.text += "\nTransaction success, hash = " + txData.data.transactionHash;
                await walletData.GetBalance();
                Populate(walletData);
            }
            catch (System.Exception e)
            {
                messageTxt.text += "\nMint error: " + e.Message;
                Debug.Log(e);
            }

            try
            {
                TransactionData txData = await LyncManager.Instance.TransactionsManager.SendTransaction(TRANSACTIONS.REFUND, customTransaction);
                messageTxt.text += "\nRefund success, hash = " + txData.data.transactionHash + "\n\n";
                await walletData.GetBalance();
                Populate(walletData);
            }
            catch (System.Exception e)
            {
                messageTxt.text += "\nRefund error: " + e.Message + "\n\n";
                Debug.Log(e);
            }

            mint.interactable = true;
        });
    }

    public void Populate(WalletData walletData = null)
    {
        publicKey.text = "Public Key = " + (walletData == null ? "" : walletData.AptosWallet.publicKey.Substring(0, 20) + "...");
        privateKey.text = "Private Key = " + (walletData == null ? "" : walletData.AptosWallet.privateKey.Substring(0, 20) + "...");
        loginDateTxt.text = "Login Date = " + (walletData == null ? "" : walletData.loginDate.ToString());
        balance.text = "Balance = " + (walletData == null ? "00" : walletData.AptosWallet.balance) + " APT";
    }
}
