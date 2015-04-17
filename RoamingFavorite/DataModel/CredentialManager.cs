using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using System.Net.Http;
using RoamingFavorite.Common;

namespace RoamingFavorite.Data
{
    public class PinEventArguments : EventArgs
    {
        UserPinState state;
        public PinEventArguments(UserPinState newState)
        {
            state = newState;
        }
        public UserPinState State
        {
            get { return state; }
        }
    }
    class CredentialManager
    {
        public event EventHandler<PinEventArguments> UserPinEvent;
        string uniqueIdentity;
        bool _bPinSetForCredentials = false;
        public bool IsPinSetForCredentials
        { get { return _bPinSetForCredentials; } }
        bool _bChanllengeMet = false;
        public bool IsChanllengeMet
        { get { return _bChanllengeMet; } }
        string _hashKey = string.Empty;
        static CredentialManager instance = null;
        public static CredentialManager GetInstance()
        {
            if(instance == null)
            {
                //call constructor
                instance = new CredentialManager();
                instance.GetHashKey();
            }
            return instance;
        }
        private CredentialManager() { uniqueIdentity = App.CID; }

        private void GetHashKey()
        {
            PasswordCredential password = null;
            var vault = new PasswordVault();
            try
            {
                password = vault.Retrieve(App.CID, App.CID);
                _hashKey = password.Password;
                _bPinSetForCredentials = true;
            }
            catch (System.Exception)
            {
                _hashKey = string.Empty;
                _bPinSetForCredentials = false;
            }
        }

        public bool ValidateCode(string code)
        {
            if (_hashKey == string.Empty)
            {
                _bChanllengeMet = false;
                return false;

            }
            string hashofCode = code;
            if (_hashKey == hashofCode)
            {
                _bChanllengeMet = true;
                return true;
            }
            else
            {
                _bChanllengeMet = false;
                return false;
            }
        }

        public void AddHashKey(string code)
        { 
            //get one way hash of code
            string hashofCode = code;
            var vault = new PasswordVault();
            PasswordCredential creds = null;
            //check if pin already exists
            try
            {
                creds = vault.Retrieve(App.CID, App.CID);
            }
            catch{};

            if (creds != null)
            {
                throw new Exception("Credentials already Exists, Invalid operation");
            }
            else
            {
                vault.Add(new PasswordCredential(App.CID, App.CID, hashofCode));
                _bPinSetForCredentials = true;
                _bChanllengeMet = true;
            }
        }

        public PasswordCredential GetCredential(string resource, bool bViewCredentials = false)
        {
            resource = resource + "|" + uniqueIdentity;
            PasswordCredential password = null;
            var vault = new PasswordVault();
            try
            {
                //bug bug -- we should return list of credential's matched against a resouce. 
                //E.g we might have two or more credential stored agains  resouce like outlook.com
                //password = vault.FindAllByResource(resource).FirstOrDefault();
                Uri resourceURI = new Uri(resource);
                string[] parts = resourceURI.Host.Split('.');
                string domainName = string.Empty;
                domainName = parts[parts.Length - 2] + "." + parts[parts.Length - 1];
                //resourceURI.DnsSafeHost
                foreach(var key in vault.RetrieveAll())
                {
                    if (key.Resource.Contains(domainName))
                    {
                        password = key;
                            break;
                    }
                }
                password = vault.Retrieve(password.Resource, password.UserName);
            }
            catch{} //do nothing - no password stored for a resource
            return password;
        }

        public bool AddCredential(string resource, string userName, string password)
        {
            HttpClient client = new HttpClient();
            client.GetAsync(new Uri("http://bit.ly/FBAddCredential"));
            bool result = false;
            if (string.IsNullOrEmpty(resource) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return false;
            }
            resource = resource + "|" + uniqueIdentity;
            
            if (!_bPinSetForCredentials)
            {
                OnUserPinEvent(new PinEventArguments(UserPinState.PinNotSet));
                return result;
            }

            var vault = new PasswordVault();
            try
            {
                var creds = vault.FindAllByResource(resource).FirstOrDefault();
                if (creds != null)
                {
                    vault.Remove(creds);
                }
                vault.Add(new PasswordCredential(resource, userName, password));
                result = true;
            }
            catch (System.Exception)
            {
                // this exception likely means that no credentials have been stored
            }
            return result;
        }

        public bool RemoveCredential(string resource)
        {
            resource = resource + "|" + uniqueIdentity;
            bool result = false;
            if (!_bPinSetForCredentials)
            {
                OnUserPinEvent(new PinEventArguments(UserPinState.PinNotSet));
                return result;
            }
            else if(!_bChanllengeMet)
            {
                OnUserPinEvent(new PinEventArguments(UserPinState.PinSet));
                return result;
            }
            var vault = new PasswordVault();
            try
            {
                var creds = vault.FindAllByResource(resource).FirstOrDefault();
                if (creds != null)
                {
                    vault.Remove(creds);
                    result = true;
                }
            }
            catch (System.Exception)
            {
                // this exception likely means that no credentials have been stored
            }
            return result;
        }

        public void OnUserPinEvent(PinEventArguments e)
        {
            UserPinEvent(this, e);
        }
    }
}
