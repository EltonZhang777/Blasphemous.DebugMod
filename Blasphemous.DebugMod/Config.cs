
namespace Blasphemous.DebugMod;

/// <summary>
/// Stores configuration settings
/// </summary>
public class Config
{
    /// <summary> How many digits after the decimanl to display </summary>
    public int infoPrecision = 2;
    /// <summary> How many seconds in between each update cycle </summary>
    public float hitboxUpdateDelay = 0.2f;
    /// <summary> Speed of the player in noclip mode </summary>
    public float playerSpeed = 15f;
    /// <summary> Speed of the camera in freecam mode </summary>
    public float cameraSpeed = 15f;

    /// <summary> Smallest FOV that the camera can zoom in </summary>
    public float cameraZoomMin = 0.1f;

    /// <summary> Largest FOV that the camera can zoom out </summary>
    public float cameraZoomMax = 64f;

    /// <summary> Speed of camera zooming </summary>
    public float cameraZoomSpeed = 5f;
}
