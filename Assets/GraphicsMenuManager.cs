using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsMenuManager : MonoBehaviour
{    
    private Vector2Int _screenDimensions;
    private FullScreenMode _currentFullscreenMode;
    private List<Vector2Int> _resolutionOptions;

    // Start is called before the first frame update
    void Start()
    {
        _resolutionOptions.Add(new Vector2Int(1024, 576));
        _resolutionOptions.Add(new Vector2Int(1152, 648));
        _resolutionOptions.Add(new Vector2Int(1024, 576));
        _resolutionOptions.Add(new Vector2Int(1024, 576));
        _resolutionOptions.Add(new Vector2Int(1024, 576));
        _resolutionOptions.Add(new Vector2Int(1024, 576));

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnFullscreenToggled()
    {
        switch (_currentFullscreenMode)
        {
            case FullScreenMode.FullScreenWindow:
                _currentFullscreenMode = FullScreenMode.MaximizedWindow;
                break;
            case FullScreenMode.MaximizedWindow:
                _currentFullscreenMode = FullScreenMode.Windowed;
                break;
            case FullScreenMode.Windowed:
                _currentFullscreenMode = FullScreenMode.MaximizedWindow;
                break;
            default:
                Debug.LogError("Shouldn't be here!");
                break;
        }
    }

    public void ApplyChanges()
    {
        Screen.fullScreenMode = _currentFullscreenMode;
    }
    
    public void OnSizeToggled()
    {

    }
}
