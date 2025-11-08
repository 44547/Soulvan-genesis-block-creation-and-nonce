using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// CameraCinematicController: Spline-based cinematic camera system for heist cutscenes.
/// Features: Spline path following, timing control, camera shake, blend with gameplay camera.
/// Integrates with mission beats for automatic cutscene triggers.
/// </summary>
public class CameraCinematicController : MonoBehaviour
{
    [Header("Spline Configuration")]
    [Tooltip("Spline path nodes")]
    public Transform[] splineNodes;

    [Tooltip("Spline interpolation smoothness (segments per node)")]
    public int interpolationSegments = 10;

    [Tooltip("Use smooth Catmull-Rom interpolation")]
    public bool useCatmullRom = true;

    [Header("Playback")]
    [Tooltip("Cinematic duration in seconds")]
    public float duration = 5f;

    [Tooltip("Animation curve for speed variation")]
    public AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Tooltip("Look at target (null = look along path)")]
    public Transform lookAtTarget;

    [Tooltip("Look ahead distance along path (if no lookAtTarget)")]
    public float lookAheadDistance = 5f;

    [Header("Camera Effects")]
    [Tooltip("Field of view override (0 = no override)")]
    public float fovOverride = 0f;

    [Tooltip("Camera shake intensity")]
    public float shakeIntensity = 0f;

    [Tooltip("Camera shake frequency")]
    public float shakeFrequency = 10f;

    [Header("Blend Settings")]
    [Tooltip("Blend in duration")]
    public float blendInDuration = 1f;

    [Tooltip("Blend out duration")]
    public float blendOutDuration = 1f;

    [Tooltip("Gameplay camera to blend from/to")]
    public Camera gameplayCamera;

    // Internal state
    private Camera _cinematicCamera;
    private List<Vector3> _splinePoints = new List<Vector3>();
    private bool _isPlaying = false;
    private float _playbackTime = 0f;
    private Vector3 _originalCameraPosition;
    private Quaternion _originalCameraRotation;
    private float _originalFOV;

    // Events
    public delegate void CinematicEventHandler();
    public event CinematicEventHandler OnCinematicStart;
    public event CinematicEventHandler OnCinematicEnd;

    void Awake()
    {
        _cinematicCamera = GetComponent<Camera>();
        if (_cinematicCamera == null)
        {
            _cinematicCamera = gameObject.AddComponent<Camera>();
        }

        _cinematicCamera.enabled = false;

        if (gameplayCamera != null)
        {
            _originalFOV = gameplayCamera.fieldOfView;
        }
    }

    void Start()
    {
        if (splineNodes != null && splineNodes.Length > 0)
        {
            GenerateSpline();
        }
    }

    /// <summary>
    /// Generate spline points from node transforms
    /// </summary>
    void GenerateSpline()
    {
        _splinePoints.Clear();

        if (splineNodes.Length < 2)
        {
            Debug.LogWarning("CameraCinematicController: Need at least 2 nodes for spline");
            return;
        }

        if (useCatmullRom && splineNodes.Length >= 4)
        {
            GenerateCatmullRomSpline();
        }
        else
        {
            GenerateLinearSpline();
        }

        Debug.Log($"CameraCinematicController: Generated spline with {_splinePoints.Count} points");
    }

    /// <summary>
    /// Generate Catmull-Rom spline for smooth interpolation
    /// </summary>
    void GenerateCatmullRomSpline()
    {
        for (int i = 0; i < splineNodes.Length - 1; i++)
        {
            Vector3 p0 = i > 0 ? splineNodes[i - 1].position : splineNodes[i].position;
            Vector3 p1 = splineNodes[i].position;
            Vector3 p2 = splineNodes[i + 1].position;
            Vector3 p3 = i < splineNodes.Length - 2 ? splineNodes[i + 2].position : splineNodes[i + 1].position;

            for (int j = 0; j < interpolationSegments; j++)
            {
                float t = (float)j / interpolationSegments;
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                _splinePoints.Add(point);
            }
        }

        // Add final node
        _splinePoints.Add(splineNodes[splineNodes.Length - 1].position);
    }

    /// <summary>
    /// Generate linear spline (simple Lerp between nodes)
    /// </summary>
    void GenerateLinearSpline()
    {
        for (int i = 0; i < splineNodes.Length - 1; i++)
        {
            Vector3 p1 = splineNodes[i].position;
            Vector3 p2 = splineNodes[i + 1].position;

            for (int j = 0; j < interpolationSegments; j++)
            {
                float t = (float)j / interpolationSegments;
                Vector3 point = Vector3.Lerp(p1, p2, t);
                _splinePoints.Add(point);
            }
        }

        _splinePoints.Add(splineNodes[splineNodes.Length - 1].position);
    }

    /// <summary>
    /// Catmull-Rom interpolation
    /// </summary>
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    /// <summary>
    /// Play cinematic
    /// </summary>
    public void PlayCinematic()
    {
        if (_splinePoints.Count == 0)
        {
            Debug.LogWarning("CameraCinematicController: No spline points, cannot play cinematic");
            return;
        }

        StartCoroutine(PlayCinematicCoroutine());
    }

    /// <summary>
    /// Cinematic playback coroutine
    /// </summary>
    IEnumerator PlayCinematicCoroutine()
    {
        _isPlaying = true;
        _playbackTime = 0f;

        OnCinematicStart?.Invoke();

        // Blend in
        yield return BlendCameras(gameplayCamera, _cinematicCamera, blendInDuration);

        // Enable cinematic camera
        if (gameplayCamera != null)
        {
            gameplayCamera.enabled = false;
        }
        _cinematicCamera.enabled = true;

        // Apply FOV override
        if (fovOverride > 0f)
        {
            _cinematicCamera.fieldOfView = fovOverride;
        }

        // Play cinematic
        while (_playbackTime < duration)
        {
            _playbackTime += Time.deltaTime;
            float t = Mathf.Clamp01(_playbackTime / duration);

            // Apply speed curve
            float curveT = speedCurve.Evaluate(t);

            // Get position on spline
            Vector3 position = GetPositionOnSpline(curveT);
            _cinematicCamera.transform.position = position;

            // Apply camera shake
            if (shakeIntensity > 0f)
            {
                Vector3 shake = new Vector3(
                    Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f,
                    Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f,
                    0f
                ) * shakeIntensity;
                _cinematicCamera.transform.position += shake;
            }

            // Calculate rotation
            Quaternion rotation;
            if (lookAtTarget != null)
            {
                rotation = Quaternion.LookRotation(lookAtTarget.position - position);
            }
            else
            {
                // Look ahead along path
                Vector3 lookAheadPosition = GetPositionOnSpline(Mathf.Clamp01(curveT + 0.01f));
                rotation = Quaternion.LookRotation(lookAheadPosition - position);
            }

            _cinematicCamera.transform.rotation = rotation;

            yield return null;
        }

        // Blend out
        yield return BlendCameras(_cinematicCamera, gameplayCamera, blendOutDuration);

        // Disable cinematic camera
        _cinematicCamera.enabled = false;
        if (gameplayCamera != null)
        {
            gameplayCamera.enabled = true;
            gameplayCamera.fieldOfView = _originalFOV;
        }

        _isPlaying = false;

        OnCinematicEnd?.Invoke();

        Debug.Log("CameraCinematicController: Cinematic complete");
    }

    /// <summary>
    /// Get position on spline at normalized time (0-1)
    /// </summary>
    Vector3 GetPositionOnSpline(float t)
    {
        if (_splinePoints.Count == 0)
        {
            return Vector3.zero;
        }

        float indexFloat = t * (_splinePoints.Count - 1);
        int index = Mathf.FloorToInt(indexFloat);
        float fraction = indexFloat - index;

        if (index >= _splinePoints.Count - 1)
        {
            return _splinePoints[_splinePoints.Count - 1];
        }

        return Vector3.Lerp(_splinePoints[index], _splinePoints[index + 1], fraction);
    }

    /// <summary>
    /// Blend between two cameras
    /// </summary>
    IEnumerator BlendCameras(Camera fromCamera, Camera toCamera, float blendDuration)
    {
        if (fromCamera == null || toCamera == null || blendDuration <= 0f)
        {
            yield break;
        }

        float elapsed = 0f;
        Vector3 startPosition = fromCamera.transform.position;
        Quaternion startRotation = fromCamera.transform.rotation;
        float startFOV = fromCamera.fieldOfView;

        Vector3 endPosition = toCamera.transform.position;
        Quaternion endRotation = toCamera.transform.rotation;
        float endFOV = toCamera.fieldOfView;

        while (elapsed < blendDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blendDuration;

            toCamera.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            toCamera.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            toCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, t);

            yield return null;
        }
    }

    /// <summary>
    /// Stop cinematic playback
    /// </summary>
    public void StopCinematic()
    {
        if (_isPlaying)
        {
            StopAllCoroutines();
            _isPlaying = false;

            _cinematicCamera.enabled = false;
            if (gameplayCamera != null)
            {
                gameplayCamera.enabled = true;
                gameplayCamera.fieldOfView = _originalFOV;
            }

            Debug.Log("CameraCinematicController: Cinematic stopped");
        }
    }

    /// <summary>
    /// Check if cinematic is playing
    /// </summary>
    public bool IsPlaying()
    {
        return _isPlaying;
    }

    void OnDrawGizmos()
    {
        // Draw spline path
        if (_splinePoints != null && _splinePoints.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _splinePoints.Count - 1; i++)
            {
                Gizmos.DrawLine(_splinePoints[i], _splinePoints[i + 1]);
            }

            // Draw nodes
            Gizmos.color = Color.yellow;
            foreach (var point in _splinePoints)
            {
                Gizmos.DrawWireSphere(point, 0.5f);
            }
        }

        // Draw spline nodes
        if (splineNodes != null)
        {
            Gizmos.color = Color.red;
            foreach (var node in splineNodes)
            {
                if (node != null)
                {
                    Gizmos.DrawWireSphere(node.position, 1f);
                }
            }
        }
    }
}
