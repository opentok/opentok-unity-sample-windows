using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTok;

public class OpenTokSession {
    string SESSION_ID = "";
    string API_KEY = "";
    string TOKEN = "";

    public bool Connected;

    Session session;
    Publisher publisher;
    Subscriber subscriber;

    GameObject publisherGameObject;
    GameObject subscriberGameObject;    

    OpenTokRenderer subscriberRenderer;
    OpenTokRenderer publisherRenderer;

    VideoRender subscriberRender;
    VideoRender publisherRender;

    public OpenTokSession(GameObject publisherGo, GameObject subscriberGo)
    {
        Debug.LogFormat("Creating session");
        session = new Session(Context.Instance, API_KEY, SESSION_ID);
        session.Connected += Session_Connected;
        session.Disconnected += Session_Disconnected;
        session.StreamReceived += Session_StreamReceived;        

        publisherGameObject = publisherGo;
        subscriberGameObject = subscriberGo;
        subscriberRenderer = subscriberGameObject.GetComponent<OpenTokRenderer>();
        publisherRenderer = publisherGameObject.GetComponent<OpenTokRenderer>();        
    }

    private void Session_Disconnected(object sender, System.EventArgs e)
    {
        Debug.Log("Session Disconnected");              
        Connected = false;
        if (subscriber != null)
        {
            subscriber.Dispose();
            subscriber = null;

            subscriberRender.Dispose();
            subscriberRender = null;

            subscriberRenderer.Enabled = false;
        }

        if (publisher != null)
        {
            publisher.Dispose();
            publisher = null;

            publisherRender.Dispose();
            publisherRender = null;

            publisherRenderer.Enabled = false;
        }

        Context.Instance.Dispose();

        Debug.Log("Object disposed");
    }

    public void Connect()
    {
        Debug.LogFormat("Connecting...");     
        session.Connect(TOKEN);
    }
    private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
    {
        if (subscriber != null)
        {
            return; // This sample can only handle one subscriber
        }
        Debug.LogFormat("Stream received {0}", e.Stream.Id);        
        subscriberRender = new VideoRender(subscriberRenderer.rendererId);

        subscriber = new Subscriber(Context.Instance, e.Stream, subscriberRender);       
        session.Subscribe(subscriber);

        subscriberRenderer.Enabled = true;        
    }

    private void Session_Connected(object sender, System.EventArgs e)
    {
        Debug.Log("Session Connected");        
        Connected = true;

        Debug.Log("Creating Publisher");
        publisherRender = new VideoRender(publisherRenderer.rendererId);
        publisher = new Publisher(Context.Instance, renderer: publisherRender);
        publisher.StreamCreated += Publisher_StreamCreated;
        session.Publish(publisher);

        publisherRenderer.Enabled = true;
    }

    private void Publisher_StreamCreated(object sender, Publisher.StreamEventArgs e)
    {
        Debug.Log("Publisher Stream Created");
    }

    public void Stop()
    {
        Debug.Log("Stopping OT");        
        session.Disconnect();

        var otRenderer = subscriberGameObject.GetComponent<OpenTokRenderer>();
        otRenderer.Enabled = false;
    }
}