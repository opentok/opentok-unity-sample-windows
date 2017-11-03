using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTok;

public class OpenTokSession {
    string SESSION_ID = "1_MX40NTMyODc3Mn5-MTUwOTYxOTY2MDY0NX4vNkpZckxHVzc0MDV6emJIQnYrbk12dEx-fg";
    string API_KEY = "45328772";
    string TOKEN = "T1==cGFydG5lcl9pZD00NTMyODc3MiZzaWc9N2Y2MDFjNDdlNzI3YzE3ZDkzZTBlZDdmZDI2ZGRkZTBhMWM0Mjk0MTpzZXNzaW9uX2lkPTFfTVg0ME5UTXlPRGMzTW41LU1UVXdPVFl4T1RZMk1EWTBOWDR2TmtwWmNreEhWemMwTURWNmVtSklRbllyYmsxMmRFeC1mZyZjcmVhdGVfdGltZT0xNTA5NzAwMzY1Jm5vbmNlPTAuNDU1MDcxNDg3MDgwMjg5NjYmcm9sZT1wdWJsaXNoZXImZXhwaXJlX3RpbWU9MTUwOTc4Njc2NSZjb25uZWN0aW9uX2RhdGE9JTdCJTIydXNlck5hbWUlMjIlM0ElMjJBbm9ueW1vdXMlMjBVc2VyMjQwOCUyMiU3RCZpbml0aWFsX2xheW91dF9jbGFzc19saXN0PQ==";

    Session session;
    Publisher publisher;
    Subscriber subscriber;

    int publisherRenderId;
    int subscriberRenderId;

    VideoRender subscriberRender;
    VideoRender publisherRender;

    public OpenTokSession(int publisherId, int subscriberId)
    {
        session = new Session(Context.Instance, API_KEY, SESSION_ID);
        session.Connected += Session_Connected;
        session.Disconnected += Session_Disconnected;
        session.StreamReceived += Session_StreamReceived;

        this.publisherRenderId = publisherId;
        this.subscriberRenderId = subscriberId;       
    }

    private void Session_Disconnected(object sender, System.EventArgs e)
    {
        Debug.Log("Session Disconnected");
        if (subscriber != null)
        {
            subscriber.Dispose();
            subscriber = null;
        }
    }

    public void Connect()
    {
        session.Connect(TOKEN);
    }
    private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
    {
        Debug.LogFormat("Stream received {0}", e.Stream.Id);       
        subscriberRender = new VideoRender(subscriberRenderId);

        subscriber = new Subscriber(Context.Instance, e.Stream, subscriberRender);
        session.Subscribe(subscriber);
    }

    private void Session_Connected(object sender, System.EventArgs e)
    {
        Debug.Log("Session Connected");
    }


    public void Stop()
    {
        Debug.Log("Stopping OT");
        session.Disconnect();

        Context.Instance.Dispose();
    }
}