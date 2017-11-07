using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SceneScript : MonoBehaviour {

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MyDelegate(string str);

    public GameObject publisher;
    public GameObject subscriber;

    private OpenTokSession ot;
	
	void Start () {
        Debug.LogFormat("Starting OpenTok sample");
        SetLogCallback();
        StartCoroutine(ConnectAfter());
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnApplicationQuit()
    {
        if (ot != null && ot.Connected)
        {
            ot.Stop();

            while(ot.Connected)
            {
                // Wait until session is disconnected
                new WaitForSeconds(1);
            }
        }
    }

    public void StopSession()
    {
        if (ot != null && ot.Connected)
        {
            ot.Stop();            
        }
    }

    IEnumerator ConnectAfter()
    {
        yield return new WaitForSeconds(2);

        ot = new OpenTokSession(publisher, subscriber);
        ot.Connect();
    }

    void SetLogCallback()
    {
        MyDelegate callback_delegate = new MyDelegate(CallBackFunction);
        IntPtr intptr_delegate = Marshal.GetFunctionPointerForDelegate(callback_delegate);
        RenderPlugin.SetLogFunction(intptr_delegate);
    }

    static void CallBackFunction(string str)
    {
        Debug.Log("<RenderPlugin> " + str);
    }
}
