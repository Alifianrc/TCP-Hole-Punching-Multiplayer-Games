using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance;
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private TcpClient                       m_tcpClient;
    private NetworkStream                   m_networkStream;
    private IPAddress                       m_ipAddress;
    [SerializeField] private int            m_port;
    [SerializeField] private string         m_ipAddressStr;

    private bool                            m_isConnected;
    private bool                            m_isVerified;

    private EncryptionRSA                   m_rsaEncrption;
    private EncryptionAES                   m_aesEncrption;
    [SerializeField] private string         m_PublicKeyStr;

    private void Start()
    {
        m_tcpClient     = new TcpClient();
        m_ipAddress     = IPAddress.Parse(m_ipAddressStr);
        m_rsaEncrption  = new EncryptionRSA();
        m_aesEncrption  = new EncryptionAES();

        m_rsaEncrption.PublicKey = m_rsaEncrption.ConvertStringToKey(m_PublicKeyStr);

        StartCoroutine(StartConnecting());
    }
    private IEnumerator StartConnecting()
    {
        int count = 1;
        while (!m_tcpClient.Connected)
        {
            try
            {
                m_tcpClient.Connect(m_ipAddress, m_port);
                m_networkStream = m_tcpClient.GetStream();
                //PrepareEncryption();

                Debug.Log("Connected to server!");
            }
            catch (Exception e)
            {
                Debug.Log("Try connecting-" + count + " error : " + e.Message);
                count++;
            }
            yield return new WaitForSeconds(1);
        }


    }
    private bool StartVerification()
    {
        return false;
    }
}
