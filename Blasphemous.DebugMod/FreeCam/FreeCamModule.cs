using Blasphemous.Framework.UI;
using Blasphemous.ModdingAPI.Input;
using Gameplay.UI.Others.UIGameLogic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Blasphemous.DebugMod.FreeCam;

/// <summary>
/// Module for allowing the camera to move anywhere
/// </summary>
internal class FreeCamModule(Sprite image, float speed) : BaseModule(keybindName, true)
{
    private readonly Sprite _image = image;
    private readonly float _speed = speed;
    private static readonly float _orthSizeMin = Main.Debugger.ConfigHandler.Load<Config>().cameraZoomMin;
    private static readonly float __orthSizeMax = Main.Debugger.ConfigHandler.Load<Config>().cameraZoomMax;
    private static readonly float _zoomSpeed = Main.Debugger.ConfigHandler.Load<Config>().cameraZoomSpeed;
    internal static readonly string keybindName = "Free_Cam";
    internal static readonly string keybindName_ZoomIn = "Free_Cam_Zoom_In";
    internal static readonly string keybindName_ZoomOut = "Free_Cam_Zoom_Out";
    internal static readonly string keybindName_ZoomReset = "Free_Cam_Zoom_Reset";

    private Image cameraObject;
    private Vector3 cameraPosition;
    private float _vanillaOrthographicSize;

    protected override void OnActivate()
    {
        if (cameraObject == null)
            CreateCameraImage();

        cameraObject?.gameObject.SetActive(true);
        _vanillaOrthographicSize = Camera.main.orthographicSize;
    }

    protected override void OnDeactivate()
    {
        cameraObject?.gameObject.SetActive(false);
        Camera.main.orthographicSize = _vanillaOrthographicSize;
    }

    protected override void OnUpdate()
    {
        if (!IsActive)
        {
            cameraPosition = Camera.main.transform.position;
            return;
        }

        float h = Main.Debugger.InputHandler.GetAxis(AxisCode.MoveRHorizontal, true);
        float v = Main.Debugger.InputHandler.GetAxis(AxisCode.MoveRVertical, true);
        var direction = new Vector3(h, v).normalized;

        cameraPosition += direction * _speed * Time.deltaTime;
        Camera.main.transform.position = cameraPosition;

        if (Main.Debugger.InputHandler.GetKey(keybindName_ZoomIn))
        {
            Camera.main.orthographicSize = Mathf.Max(_orthSizeMin, Camera.main.orthographicSize - _zoomSpeed * Time.deltaTime);
        }
        else if (Main.Debugger.InputHandler.GetKey(keybindName_ZoomOut))
        {
            Camera.main.orthographicSize = Mathf.Min(__orthSizeMax, Camera.main.orthographicSize + _zoomSpeed * Time.deltaTime);
        }

        if (Main.Debugger.InputHandler.GetKeyDown(keybindName_ZoomReset))
        {
            Camera.main.orthographicSize = _vanillaOrthographicSize;
        }
    }

    private void CreateCameraImage()
    {
        Transform parent = Object.FindObjectsOfType<PlayerPurgePoints>().FirstOrDefault(x => x.name == "PurgePoints")?.transform;
        if (parent == null)
            return;

        cameraObject = UIModder.Create(new RectCreationOptions()
        {
            Name = "FreeCam Icon",
            Parent = parent,
            XRange = Vector2.one,
            YRange = Vector2.one,
            Pivot = Vector2.one,
            Position = new Vector2(0, -80),
            Size = new Vector2(24, 24),
        }).AddImage(new ImageCreationOptions()
        {
            Sprite = _image
        });
    }
}
