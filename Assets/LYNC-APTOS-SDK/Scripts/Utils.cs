namespace LYNC
{
    public class Utils { }

    [System.Serializable]
    public class AptosServerResponse
    {
        public string message;
        public int status;
        public bool success;
        public AptosWallet data;
    }

    [System.Serializable]
    public class AptosWallet
    {
        public bool isFunded;
        public string mintingHash;
        public string fundingHash;
        public string _id;
        public string firebaseUid;
        public string email;
        public string name;
        public string avatar;
        public string lastLoginAt;
        public string providerId;
        public string createdAt;
        public string updatedAt;
        public string privateKey;
        public string publicKey;
        public float balance;
    }

    public class AptosProfileData
    {
        public string email;
        public string firebaseUid;

        public AptosProfileData(string email, string firebaseUid)
        {
            this.email = email;
            this.firebaseUid = firebaseUid;
        }
    }
}