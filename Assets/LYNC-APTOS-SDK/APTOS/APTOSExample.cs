using UnityEngine;
using TMPro;
using LYNC;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class APTOSExample : MonoBehaviour
{
    [Header("General settings")]
    public Button login, logout, mint;

    [Space]
    [Header("Firebase")]
    public Transform aptosContainer;
    public TMP_Text publicKey, privateKey, loginDateTxt, balance;

    [Space]
    [Header("Pontem")]
    public Transform pontemContainer;
    public TMP_Text pontemPublicAddress;

    [Space]
    [Header("Pontem")]
    public Transform keylessContainer;
    public TMP_Text accountAddress, keylessPublicKey, keylessPrivateKey, keylessLoginDate;

    [Space]
    [Header("Transactions")]
    public Transform transactionResultsParent;
    public GameObject transactionResultHolder;
    public Transaction mintTxn;

    private AuthBase authBase;
    public static APTOSExample Instance;

    private void OnEnable()
    {
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
                Debug.Log("Saved wallet successfully loaded");
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
            Lync.WalletAuth.ConnectWallet((wallet) =>
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
            // mint.interactable = false;

            TransactionResult txData = await LyncManager.Instance.TransactionsManager.SendTransaction(mintTxn);
            if (txData.success)
                SuccessfullTransaction(txData.hash, "MINT");
            else
                ErrorTransaction(txData.error);

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

        if (AuthBase.AuthType == AUTH_TYPE.KEYLESS)
        {
            var authData = _authBase as KeylessAuth;
            accountAddress.text = authData.PublicAddress;
            keylessPublicKey.text = authData.KeyPairPublicKey;
            keylessPrivateKey.text = authData.KeyPairPrivateKey;
            keylessLoginDate.text = authBase.LoginDate.ToString();
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
            keylessContainer.gameObject.SetActive(false);
            Debug.Log("FIREBASE auth");
        }
        if (authType == AUTH_TYPE.PONTEM)
        {
            pontemContainer.gameObject.SetActive(true);
            aptosContainer.gameObject.SetActive(false);
            keylessContainer.gameObject.SetActive(false);
            Debug.Log("PONTEM auth");
        }
        if (authType == AUTH_TYPE.KEYLESS)
        {
            pontemContainer.gameObject.SetActive(false);
            aptosContainer.gameObject.SetActive(false);
            keylessContainer.gameObject.SetActive(true);
            Debug.Log("KEYLESS auth");
        }
    }

    private void SuccessfullTransaction(string hash, string txnTitle = "")
    {
        var go = Instantiate(transactionResultHolder, transactionResultsParent);

        if (!string.IsNullOrEmpty(hash))
        {
            go.transform.GetComponentInChildren<TMP_Text>().text = (txnTitle != "" ? ("(" + txnTitle + ")") : "") + " Success, hash = " + hash.Substring(0, 5) + "..." + hash.Substring(hash.Length - 5) + "<color=\"green\"> Check on APTOS EXPLORER<color=\"green\">";
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener((eventData) => { Application.OpenURL("https://explorer.aptoslabs.com/txn/" + hash + "?network=testnet"); });
            trigger.triggers.Add(entry);
        }
        else
        {
            // Pontem mobile transactions doesnt contain a hash
            go.transform.GetComponentInChildren<TMP_Text>().text = (txnTitle != "" ? ("(" + txnTitle + ")") : "") + " Successfull transaction";
        }
    }

    private void ErrorTransaction(string error, string txnTitle = "")
    {
        var go = Instantiate(transactionResultHolder, transactionResultsParent);
        go.transform.GetComponentInChildren<TMP_Text>().text = txnTitle + " <color=\"red\">TXN ERROR:</color=\"red\"> " + error;
    }

    public void Populate(FirebaseAuth firebaseAuth = null)
    {
        publicKey.text = "Public Key = " + (firebaseAuth == null ? "" : firebaseAuth.AptosFirebaseAuthData.publicKey.Substring(0, 20) + "...");
        privateKey.text = "Private Key = " + (firebaseAuth == null ? "" : firebaseAuth.AptosFirebaseAuthData.privateKey.Substring(0, 20) + "...");
        loginDateTxt.text = "Login Date = " + (firebaseAuth == null ? "" : firebaseAuth.LoginDate.ToString());
        balance.text = "Balance = " + (firebaseAuth == null ? "00" : firebaseAuth.AptosFirebaseAuthData.balance) + " APT";
    }
}
