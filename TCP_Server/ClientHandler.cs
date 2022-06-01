using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ServerClientLib;

namespace TCP_Server
{
    enum ClientState { ONLINE, ROOM };

    class ClientHandler
    {
        public string m_name;
        public string m_id;

        public TcpClient m_tcp;
        private Server m_server;

        public NetworkStream m_networkStream;
        private Room m_room;
        private bool m_isMaster;
        private bool m_isOnline;

        private int m_defaultCheckTime;
        private int m_checkTime;

        private ClientState m_state;

        EncryptionAES m_aesEcryption;
        EncryptionRSA m_rsaEncrytion;

        #region Initialization
        public ClientHandler(TcpClient tcpClient, Server server)
        {
            m_tcp = tcpClient;
            m_server = server;

            m_networkStream = m_tcp.GetStream();

            m_aesEcryption = new EncryptionAES();
            m_rsaEncrytion = new EncryptionRSA();

            string strRsaKey = File.ReadAllText("Private-Key.txt");
            m_rsaEncrytion.PrivateKey = m_rsaEncrytion.ConvertStringToKey(strRsaKey);

            bool isVerified = Verification();
            if (!isVerified)
            {
                Console.WriteLine("Verification Failed!");
                m_networkStream.Close();
                return;
            }

            m_server.Clients.Add(this);
            m_state     = ClientState.ONLINE;
            m_isOnline  = true;

            Console.WriteLine("--> Client " + m_id + " " + m_name + "connected!");
            Console.WriteLine("--> Online Player Count : " + server.Clients.Count);

            StartCommunication();
        }
        private bool Verification()
        {
            // Step 1
            BinaryFormatter formatter = new BinaryFormatter();
            string dataIn = formatter.Deserialize(m_networkStream) as string;

            string newRsaKey = m_rsaEncrytion.Decrypt(dataIn, m_rsaEncrytion.PrivateKey);
            if (newRsaKey == null)
            {
                return false;
            }
            m_rsaEncrytion.PublicKey = m_rsaEncrytion.ConvertStringToKey(newRsaKey);

            m_aesEcryption.GenerateNewKey();

            string newAesKey = m_aesEcryption.ConvertKeyToString(m_aesEcryption.Key);
            string encryptedAesKey = m_rsaEncrytion.Encrypt(newAesKey, m_rsaEncrytion.PublicKey);
            formatter.Serialize(m_networkStream, encryptedAesKey);

            // Step 2
            dataIn = formatter.Deserialize(m_networkStream) as string;
            string decrptedDataIn = m_aesEcryption.Decrypt(dataIn);
            if (decrptedDataIn == null)
            {
                return false;
            }

            string[] info = dataIn.Split("|");
            m_id = info[0];
            m_name = info[1];

            return true;
        }
        private void StartCommunication()
        {
            Thread recieveThread = new Thread(ReceiveMessage);
            //Thread checkConnectionThread    = new Thread(CheckConnection);

            recieveThread.Start();
            //checkConnectionThread.Start();
        }
        #endregion

        #region Receiving Message
        private void ReceiveMessage()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            while (m_isOnline)
            {
                if (m_networkStream.DataAvailable)
                {
                    // Format Received Message : TargetMessage|Header|Param1|Param2...
                    string dataIn = formatter.Deserialize(m_networkStream) as string;
                    string dataDecrpted = m_aesEcryption.Decrypt(dataIn);
                    string[] info = dataDecrpted.Split("|");

                    string message = ServerClient.TakeMessageInBack(dataDecrpted);
                    TargetHandle(info[0], message);
                }
            }
        }
        private void TargetHandle(string strTarget, string message)
        {
            TargetMsg targetMsg;
            try
            {
                targetMsg = (TargetMsg)int.Parse(strTarget);
            }
            catch (Exception e)
            {
                SendMessage(strTarget, message);
                return;
            }


            if (targetMsg == TargetMsg.SERVER)
            {
                HandleClientRequest(message);
            }
            else if (targetMsg == TargetMsg.ALL || targetMsg == TargetMsg.ALLES)
            {
                SendMessage(strTarget, message);
            }
        }
        #endregion

        #region Send Message
        private void SendMessage(string message)
        {
            SendMessage(null, message);
        }
        private void SendMessage(string[] message)
        {
            SendMessage(null, message);
        }
        private void SendMessage(string target, string message)
        {
            string[] data = new string[1];
            data[0] = message;
            SendMessage(target, data);
        }
        private void SendMessage(string target, string[] message)
        {
            // Massage format : Sender|Header|Param1|Param2...
            if (m_isOnline)
            {
                string data;

                if(target == null)
                {
                    data = "Server";
                    foreach (string x in message)
                    {
                        data += "|" + x;
                    }
                    SendEncryptedSerializationDataHandler(this, data);

                    return;
                }

                if(m_state == ClientState.ONLINE || m_room == null)
                {
                    Console.WriteLine("Send Message Error : Client is not in room!");
                    return;
                }

                TargetMsg targetMsg = (TargetMsg)int.Parse(target);
                data = m_id + m_name;
                foreach(string msg in message)
                {
                    data += "|" + msg;
                }

                if (targetMsg == TargetMsg.ALL)
                {
                    foreach(ClientHandler client in m_room.Clients)
                    {
                        SendSerializationDataHandler(client, data);
                    }    
                }
                else if (targetMsg == TargetMsg.ALLES)
                {
                    foreach (ClientHandler client in m_room.Clients)
                    {
                        if(client != this)
                        {
                            SendSerializationDataHandler(client, data);
                        }
                    }
                }
            }
        }
        private void SendSerializationDataHandler(ClientHandler client, string theData)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(client.NetworkStream, theData);
            }
            catch (Exception e)
            {
                Console.WriteLine("Send massage error from " + m_name + " , to " + client.Name + " : " + e.Message);
            }
        }
        private void SendEncryptedSerializationDataHandler(ClientHandler client, string theData)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                string encrptedData = m_aesEcryption.Encrypt(theData);

                formatter.Serialize(client.NetworkStream, encrptedData);
            }
            catch (Exception e)
            {
                Console.WriteLine("Send massage error from " + m_name + " , to " + client.Name + " : " + e.Message);
            }
        }
        #endregion

        #region Handle Client Request
        private void HandleClientRequest(string message)
        {
            string[] info           = message.Split("|");
            HeaderRequest header    = (HeaderRequest)int.Parse(info[0]);

            if(header == HeaderRequest.MATCHMAKING)
            {

            }
            else if(header == HeaderRequest.CREATE_ROOM)
            {

            }
            else if(header == HeaderRequest.JOIN_ROOM)
            {

            }
            else if(header == HeaderRequest.START_TCP_HOLE_PUNCHING)
            {

            }
        }
        private void Matchmaking()
        {

        }
        private void CreateRoom()
        {

        }
        private void CreateRoom(int maxPlayer, bool isVisible, string roomName)
        {

        }
        private void JoinRoom(string roomName)
        {

        }
        #endregion

        #region Get Private Variable
        public NetworkStream NetworkStream
        {
            get         => m_networkStream;
            private set => m_networkStream = value;
        }
        public string Name
        {
            get         => m_name;
            private set => m_name = value;
        }
        #endregion
    }
}
