# Unity Windows Rendering sample

<img src="https://assets.tokbox.com/img/vonage/Vonage_VideoAPI_black.svg" height="48px" alt="Tokbox is now known as Vonage" />

In this repo, you will find a sample of a Unity integration with the [Opentok Windows SDK](https://tokbox.com/developer/sdks/windows/).

Please note that this sample is **only compatible with Windows**.

The sample has been tested in **Unity 2017.1 and 2017.2**. Although we haven't tested it, this should also work with older versions of Unity where the native rendering API is the same we are using.

# Projects structure

The repo is formed by two projects, the Unity native plugin for rendering and the Unity project that uses the native plugin to display the video stream of the OpenTok session participants.

The big picture of the sample can be illustrated with the following diagram:

```
+-------------------------+       +--------------------------+
| UnityProject (C#)       |       | Native Plugin (C++)      |
|                         |       |                          |
|                         |       |                          |
|                         |       |                          |
|  +-------------------+  |       |  +--------------------+  |
|  |                   |  |       |  |                    |  |
|  | Unity's Texture2D +--+-------+--> D3D9/11 Handle     |  |
|  |                   | SetTexture()|                    |  |
|  +-------------------+  |       |  +---------^----------+  |
|                         |       |            |             |
|                         |       |            |RenderTexture()
|  +-------------------+  |       |  +--------------------+  |
|  |                   |  |       |  |                    |  |
|  | OT Custom Render  +--+-------+--> RenderLoop         |  |
|  |                   | SetFrame()  |                    |  |
|  +-------------------+  |       |  +--------------------+  |
|                         |       |                          |
+----------+--------------+       +--------------------------+
           ^
           |
           | VideoFrames
           |
+----------+--------------+
|Opentok session          |
|                         |
|                         |
+-------------------------+
```

The Unity project uses the OpenTok Windows SDK to create a custom renderer which, on each frame, will forward it to the native plugin.
The native plugin will render the frames using DirectX using the texture which was previously created and set by the Unity project.

Please refer to both README.md for more details of each project.

1. [Native Plugin README](NativePlugin/RenderPlugin/README.md)
2. [Unity project README](UnityProject/OpenTok/README.md)

## Obtaining OpenTok Credentials

To use the OpenTok platform you need a session ID, token, and API key.
You can get these values by creating a project on your [OpenTok Account
Page](https://tokbox.com/account/) and scrolling down to the Project Tools
section of your Project page. For production deployment, you must generate the
session ID and token values using one of the [OpenTok Server
SDKs](https://tokbox.com/developer/sdks/server/).

## Development and Contributing

Interested in contributing? We :heart: pull requests! See the
[Contribution](CONTRIBUTING.md) guidelines.

## Getting Help

We love to hear from you so if you have questions, comments or find a bug in the project, let us know! You can either:

- Open an issue on this repository
- See <https://support.tokbox.com/> for support options
- Tweet at us! We're [@VonageDev](https://twitter.com/VonageDev) on Twitter
- Or [join the Vonage Developer Community Slack](https://developer.nexmo.com/community/slack)

## Further Reading

- Check out the Developer Documentation at <https://tokbox.com/developer/>
