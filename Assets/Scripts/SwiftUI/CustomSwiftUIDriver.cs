using System;
using System.Collections.Generic;
using AOT;
using Unity.PolySpatial;
using PolySpatial.Samples;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_VISIONOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace VDOTModule.XR.UI
{
  // This is a driver MonoBehaviour that connects to SwiftUISamplePlugin.swift via
  // C# DllImport. See SwiftUISamplePlugin.swift for more information.
  public class CustomSwiftUIDriver : MonoBehaviour
  {
    [SerializeField]
    CustomSwiftUIButton m_Button;

    [SerializeField]
    List<GameObject> m_ObjectsToSpawn;

    [SerializeField]
    Transform m_SpawnPosition;

    [SerializeField]
    SwiftFPSCounter m_FPSCounter;

    bool m_SwiftUIWindowOpen = false;
    int m_CubeCount = 0;
    int m_SphereCount = 0;

    void OnEnable()
    {
      m_Button.WasPressed += WasPressed;
      SetNativeCallback(CallbackFromNative);
    }

    void OnDisable()
    {
      SetNativeCallback(null);
      CloseSwiftUIWindow("ControlPanel");
    }
    private string Escape(string s) { return s.Replace(":", "\\:"); }

    // NOTE: ADD ShowInteractableInfo
    public void ShowInteractableInfo(string moduleId, string text)
    {
      if (!m_SwiftUIWindowOpen)
      {
        OpenSwiftUIWindow("InteractableInfo");
        m_SwiftUIWindowOpen = true;
      }

      string msg = $"ui:open:{moduleId}:{Escape(text)}";
      SendSwiftUIMessage(msg);
    }

    void WasPressed(string buttonText, MeshRenderer _)
    {
      Debug.LogWarning("Opening Swift UI -[pressed]");
      if (m_SwiftUIWindowOpen)
      {
        CloseSwiftUIWindow("ControlPanel");
        m_SwiftUIWindowOpen = false;
      }
      else
      {
        OpenSwiftUIWindow("ControlPanel");
        m_SwiftUIWindowOpen = true;
      }

      m_FPSCounter.enabled = m_SwiftUIWindowOpen;
    }

    public void ForceCloseWindow()
    {
      CloseSwiftUIWindow("ControlPanel");
      m_SwiftUIWindowOpen = false;
    }

    delegate void CallbackDelegate(string command, int value);

    // This attribute is required for methods that are going to be called from
    // native code via a function pointer.
    [MonoPInvokeCallback(typeof(CallbackDelegate))]
    static void CallbackFromNative(string command, int value)
    {
      // MonoPInvokeCallback methods will leak exceptions and cause crashes;
      // always use a try/catch in these methods
      try
      {
        Debug.Log($"Callback from native: {command} {value}");

        // This could be stored in a static field or a singleton.
        // If you need to deal with multiple windows and need to distinguish
        // between them, you could add an ID to this callback and use that to
        // distinguish windows.
        var self = FindFirstObjectByType<CustomSwiftUIDriver>();

        if (command == "closed")
        {
          self.m_SwiftUIWindowOpen = false;
          return;
        }

        if (command == "spawn red")
        {
          self.Spawn(Color.red);
        }
        else if (command == "spawn green")
        {
          self.Spawn(Color.green);
        }
        else if (command == "spawn blue")
        {
          self.Spawn(Color.blue);
        }
        else if (command == "recolor")
        {
          var thing =
              PolySpatialObjectUtils.GetGameObjectForPolySpatialIdentifier(
                  (ulong)value);
          thing.GetComponent<MeshRenderer>().material.color = Color.magenta;
        }
      }
      catch (Exception exception)
      {
        Debug.LogException(exception);
      }
    }

    void Spawn(Color color)
    {
      var randomObject = Random.Range(0, m_ObjectsToSpawn.Count);
      var thing = Instantiate(m_ObjectsToSpawn[randomObject],
                              m_SpawnPosition.position, Quaternion.identity);
      thing.GetComponent<MeshRenderer>().material.color = color;

      SetLastObjectInstanceID(thing.GetInstanceID());

      if (randomObject == 0)
      {
        m_CubeCount++;
        SetCubeCount(m_CubeCount);
      }
      else
      {
        m_SphereCount++;
        SetSphereCount(m_SphereCount);
      }
    }

#if UNITY_VISIONOS && !UNITY_EDITOR
  [DllImport("__Internal")]
  static extern void SendSwiftUIMessage(string message);

  [DllImport("__Internal")]
  static extern void SetNativeCallback(CallbackDelegate callback);

  [DllImport("__Internal")]
  static extern void OpenSwiftUIWindow(string name);

  [DllImport("__Internal")]
  static extern void CloseSwiftUIWindow(string name);

  [DllImport("__Internal")]
  static extern void SetCubeCount(int count);

  [DllImport("__Internal")]
  static extern void SetSphereCount(int count);

  [DllImport("__Internal")]
  static extern void SetLastObjectInstanceID(int instanceId);

#else
    static void SendSwiftUIMessage(string message) { }
    static void SetNativeCallback(CallbackDelegate callback) { }
    static void OpenSwiftUIWindow(string name) { }
    static void CloseSwiftUIWindow(string name) { }

    static void SetCubeCount(int count) { }

    static void SetSphereCount(int count) { }

    static void SetLastObjectInstanceID(int instanceId) { }
#endif
  }
}
