using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using AOT;

public class Jetfire
{

    public static Queue<byte[]> ByteQueue = new Queue<byte[]>();
    public static String curr_message = "";

    public static Queue<byte[]> ByteQueue2 = new Queue<byte[]>();
    public static String curr_message2 = "";

    delegate void JetfireConnectCallback();
    [MonoPInvokeCallback(typeof(JetfireConnectCallback))]
    private static void ConnectCallback()
    {
        //Debug.Log("connect");
    }



    delegate void JetfireDisConnectCallback(string message);
    [MonoPInvokeCallback(typeof(JetfireDisConnectCallback))]
    private static void DisConnectCallback(string message)
    {
        //Debug.Log("disconnect: " + message);
    }

    delegate void JetfireReceiveMessageCallback(string message);
    [MonoPInvokeCallback(typeof(JetfireReceiveMessageCallback))]
    private static void ReceiveMessageCallback(string message)
    {
        //Debug.Log("msg: " + message);
        curr_message = message;
    }

    delegate void JetfireReceiveDataCallback(IntPtr pnt, ulong size);
    [MonoPInvokeCallback(typeof(JetfireReceiveDataCallback))]
    private static void ReceiveDataCallback(IntPtr pnt, ulong size)
    {
        if (ByteQueue.Count < 1)
        {
            byte[] bytes = new byte[size];
            Marshal.Copy(pnt, bytes, 0, (int)size);
            ByteQueue.Enqueue(bytes);
        }
    }

    //second connection
    delegate void JetfireConnectCallback2();
    [MonoPInvokeCallback(typeof(JetfireConnectCallback2))]
    private static void ConnectCallback2()
    {
        //Debug.Log("connect");
    }



    delegate void JetfireDisConnectCallback2(string message);
    [MonoPInvokeCallback(typeof(JetfireDisConnectCallback2))]
    private static void DisConnectCallback2(string message)
    {
        //Debug.Log("disconnect: " + message);
    }

    delegate void JetfireReceiveMessageCallback2(string message);
    [MonoPInvokeCallback(typeof(JetfireReceiveMessageCallback2))]
    private static void ReceiveMessageCallback2(string message)
    {
        //Debug.Log("msg: " + message);
        curr_message2 = message;
    }

    delegate void JetfireReceiveDataCallback2(IntPtr pnt, ulong size);
    [MonoPInvokeCallback(typeof(JetfireReceiveDataCallback2))]
    private static void ReceiveDataCallback2(IntPtr pnt, ulong size)
    {
        if (ByteQueue2.Count < 1)
        {
            byte[] bytes = new byte[size];
            Marshal.Copy(pnt, bytes, 0, (int)size);
            ByteQueue2.Enqueue(bytes);
        }
    }



#if UNITY_IOS && !UNITY_EDITOR
       private const string DllName = "__Internal";
#else
    private const string DllName = "libJetfire";
    #endif



    // [DllImport("__Internal")]
    // private static extern void JetfireConnect(JetfireCallBackDelegate callback,string path);

    [DllImport(DllName)]

    private static extern void JetfireOpen(
        string path,
        JetfireConnectCallback _connectCallback,
        JetfireDisConnectCallback _disConnectCallback,
        JetfireReceiveMessageCallback _receiveMessageCallback,
        JetfireReceiveDataCallback _receiveDataCallback
    );



    [DllImport(DllName)]
    private static extern void JetfireConnect();

    [DllImport(DllName)]
    private static extern void JetfireClose();

    [DllImport(DllName)]
    private static extern void JetfirePing();

    [DllImport(DllName)]
    private static extern void JetfireSendMsg(string msg);

    [DllImport(DllName)]
    private static extern void JetfireSendData(byte[] bytes, int size);

    [DllImport(DllName)]
    private static extern bool JetfireIsConnected();
    
    public static void Open(string path)
    {

        JetfireOpen(path,
            ConnectCallback,
            DisConnectCallback,
            ReceiveMessageCallback,
            ReceiveDataCallback);
    }

    public static void Connect()
    {
        JetfireConnect();
    }



    public static void Close()
    {
        JetfireClose();
    }



    public static void Ping()
    {
        JetfirePing();
    }



    public static void SendMsg(string msg)
    {
        JetfireSendMsg(msg);
    }



    public static void SendData(byte[] bytes)
    {
        JetfireSendData(bytes, bytes.Length);
    }



    public static bool IsConnected()
    {
        return JetfireIsConnected();
    }



    // second connection
    [DllImport(DllName)]

    private static extern void JetfireOpen2(
        string path,
        JetfireConnectCallback2 _connectCallback,
        JetfireDisConnectCallback2 _disConnectCallback,
        JetfireReceiveMessageCallback2 _receiveMessageCallback,
        JetfireReceiveDataCallback2 _receiveDataCallback
    );



    [DllImport(DllName)]
    private static extern void JetfireConnect2();

    [DllImport(DllName)]
    private static extern void JetfireClose2();

    [DllImport(DllName)]
    private static extern void JetfirePing2();

    [DllImport(DllName)]
    private static extern void JetfireSendMsg2(string msg);

    [DllImport(DllName)]
    private static extern void JetfireSendData2(byte[] bytes, int size);

    [DllImport(DllName)]
    private static extern bool JetfireIsConnected2();

    public static void Open2(string path)
    {

        JetfireOpen2(path,
            ConnectCallback,
            DisConnectCallback,
            ReceiveMessageCallback,
            ReceiveDataCallback);
    }

    public static void Connect2()
    {
        JetfireConnect2();
    }



    public static void Close2()
    {
        JetfireClose2();
    }



    public static void Ping2()
    {
        JetfirePing2();
    }



    public static void SendMsg2(string msg)
    {
        JetfireSendMsg2(msg);
    }



    public static void SendData2(byte[] bytes)
    {
        JetfireSendData2(bytes, bytes.Length);
    }



    public static bool IsConnected2()
    {
        return JetfireIsConnected2();
    }

}