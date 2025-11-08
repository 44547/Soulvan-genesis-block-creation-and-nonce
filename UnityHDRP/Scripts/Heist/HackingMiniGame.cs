using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// HackingMiniGame: Pattern-based hacking mechanic for vault breaches and security bypasses.
/// Features: Pattern routing puzzles, time pressure, heat generation on failure.
/// Integrates with MissionController for module activation and heat tracking.
/// </summary>
public class HackingMiniGame : MonoBehaviour
{
    [Header("UI References")]
    public GameObject hackingPanel;
    public GridLayoutGroup nodeGrid;
    public GameObject nodePrefab;
    public Text timerText;
    public Text instructionText;
    public Image progressBar;
    public Button submitButton;
    public Button cancelButton;

    [Header("Game Configuration")]
    [Tooltip("Number of nodes in the grid (e.g., 16 for 4x4)")]
    public int gridSize = 16;

    [Tooltip("Pattern length to match")]
    public int patternLength = 5;

    [Tooltip("Time limit in seconds")]
    public float timeLimit = 30f;

    [Tooltip("Heat penalty for failure")]
    public float failureHeatPenalty = 15f;

    [Tooltip("Heat reduction for success")]
    public float successHeatReduction = 5f;

    [Header("Difficulty Scaling")]
    [Tooltip("Role-based hack speed multiplier (Systems role gets bonus)")]
    public float roleSpeedMultiplier = 1.5f;

    // Internal state
    private List<int> _targetPattern = new List<int>();
    private List<int> _playerPattern = new List<int>();
    private List<HackNode> _nodes = new List<HackNode>();
    private float _remainingTime;
    private bool _isActive = false;
    private MissionController _mc;
    private RoleManager _roleManager;
    private string _currentPlayerId;
    private PlayerRole _currentPlayerRole;

    // Events
    public delegate void HackingResultHandler(bool success, float timeBonus);
    public event HackingResultHandler OnHackingComplete;

    void Awake()
    {
        _mc = FindObjectOfType<MissionController>();
        _roleManager = FindObjectOfType<RoleManager>();

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitPattern);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelHacking);
        }

        if (hackingPanel != null)
        {
            hackingPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Start hacking mini-game
    /// </summary>
    public void StartHacking(string playerId)
    {
        _currentPlayerId = playerId;
        _currentPlayerRole = _roleManager != null ? _roleManager.GetPlayerRole(playerId) : PlayerRole.None;

        // Apply role bonus for Systems specialists
        float timeLimitAdjusted = timeLimit;
        if (_currentPlayerRole == PlayerRole.Systems)
        {
            timeLimitAdjusted /= roleSpeedMultiplier;
            Debug.Log($"HackingMiniGame: Systems role bonus applied, time limit: {timeLimitAdjusted}s");
        }

        _remainingTime = timeLimitAdjusted;
        _isActive = true;

        GeneratePattern();
        InitializeGrid();
        ShowPattern();

        if (hackingPanel != null)
        {
            hackingPanel.SetActive(true);
        }

        StartCoroutine(TimerCoroutine());

        Debug.Log($"HackingMiniGame: Started for player {playerId} with pattern length {patternLength}");
    }

    /// <summary>
    /// Generate random pattern for player to match
    /// </summary>
    void GeneratePattern()
    {
        _targetPattern.Clear();
        for (int i = 0; i < patternLength; i++)
        {
            int randomNode = Random.Range(0, gridSize);
            _targetPattern.Add(randomNode);
        }
    }

    /// <summary>
    /// Initialize node grid
    /// </summary>
    void InitializeGrid()
    {
        // Clear existing nodes
        foreach (var node in _nodes)
        {
            if (node != null)
            {
                Destroy(node.gameObject);
            }
        }
        _nodes.Clear();
        _playerPattern.Clear();

        // Create new nodes
        if (nodePrefab != null && nodeGrid != null)
        {
            for (int i = 0; i < gridSize; i++)
            {
                GameObject nodeObj = Instantiate(nodePrefab, nodeGrid.transform);
                HackNode node = nodeObj.GetComponent<HackNode>();
                if (node == null)
                {
                    node = nodeObj.AddComponent<HackNode>();
                }

                node.Initialize(i, this);
                _nodes.Add(node);
            }
        }
    }

    /// <summary>
    /// Show target pattern briefly
    /// </summary>
    void ShowPattern()
    {
        StartCoroutine(ShowPatternCoroutine());
    }

    IEnumerator ShowPatternCoroutine()
    {
        if (instructionText != null)
        {
            instructionText.text = "MEMORIZE PATTERN";
        }

        // Highlight target nodes in sequence
        for (int i = 0; i < _targetPattern.Count; i++)
        {
            int nodeIndex = _targetPattern[i];
            _nodes[nodeIndex].Highlight(Color.cyan);
            yield return new WaitForSeconds(0.5f);
            _nodes[nodeIndex].ResetColor();
            yield return new WaitForSeconds(0.2f);
        }

        if (instructionText != null)
        {
            instructionText.text = "REPLICATE PATTERN";
        }
    }

    /// <summary>
    /// Player clicks node
    /// </summary>
    public void OnNodeClicked(int nodeIndex)
    {
        if (!_isActive) return;

        _playerPattern.Add(nodeIndex);
        _nodes[nodeIndex].Highlight(Color.green);

        // Update progress bar
        if (progressBar != null)
        {
            progressBar.fillAmount = (float)_playerPattern.Count / patternLength;
        }

        // Check if pattern is complete
        if (_playerPattern.Count >= patternLength)
        {
            CheckPattern();
        }
    }

    /// <summary>
    /// Submit pattern button
    /// </summary>
    void OnSubmitPattern()
    {
        if (!_isActive) return;
        CheckPattern();
    }

    /// <summary>
    /// Cancel hacking button
    /// </summary>
    void OnCancelHacking()
    {
        if (!_isActive) return;

        _isActive = false;
        StopAllCoroutines();

        if (hackingPanel != null)
        {
            hackingPanel.SetActive(false);
        }

        Debug.Log("HackingMiniGame: Cancelled by player");
    }

    /// <summary>
    /// Check if player pattern matches target
    /// </summary>
    void CheckPattern()
    {
        _isActive = false;
        StopAllCoroutines();

        bool success = PatternMatches();
        float timeBonus = _remainingTime / timeLimit;

        if (success)
        {
            OnHackSuccess(timeBonus);
        }
        else
        {
            OnHackFailure();
        }

        OnHackingComplete?.Invoke(success, timeBonus);

        StartCoroutine(CloseHackingPanel(2f));
    }

    /// <summary>
    /// Check if patterns match
    /// </summary>
    bool PatternMatches()
    {
        if (_playerPattern.Count != _targetPattern.Count)
        {
            return false;
        }

        for (int i = 0; i < _targetPattern.Count; i++)
        {
            if (_playerPattern[i] != _targetPattern[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Handle successful hack
    /// </summary>
    void OnHackSuccess(float timeBonus)
    {
        Debug.Log($"HackingMiniGame: SUCCESS! Time bonus: {timeBonus:F2}");

        if (instructionText != null)
        {
            instructionText.text = "ACCESS GRANTED";
            instructionText.color = Color.green;
        }

        // Highlight all nodes green
        foreach (var node in _nodes)
        {
            node.Highlight(Color.green);
        }

        // Reduce heat for successful hack
        if (_mc != null)
        {
            _mc.state.metadata["lastHackSuccess"] = true;
            // Note: Heat reduction happens in module activation
        }
    }

    /// <summary>
    /// Handle failed hack
    /// </summary>
    void OnHackFailure()
    {
        Debug.Log("HackingMiniGame: FAILURE");

        if (instructionText != null)
        {
            instructionText.text = "ACCESS DENIED";
            instructionText.color = Color.red;
        }

        // Highlight incorrect nodes red
        for (int i = 0; i < _playerPattern.Count; i++)
        {
            if (i >= _targetPattern.Count || _playerPattern[i] != _targetPattern[i])
            {
                _nodes[_playerPattern[i]].Highlight(Color.red);
            }
        }

        // Add heat penalty
        if (_mc != null)
        {
            _mc.RegisterModuleActivation($"breach:failed");
            _mc.state.metadata["lastHackSuccess"] = false;
        }
    }

    /// <summary>
    /// Timer coroutine
    /// </summary>
    IEnumerator TimerCoroutine()
    {
        while (_remainingTime > 0 && _isActive)
        {
            _remainingTime -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = $"TIME: {Mathf.CeilToInt(_remainingTime)}s";
            }

            yield return null;
        }

        // Time expired
        if (_isActive)
        {
            OnHackFailure();
            OnHackingComplete?.Invoke(false, 0f);
            StartCoroutine(CloseHackingPanel(2f));
        }
    }

    /// <summary>
    /// Close hacking panel after delay
    /// </summary>
    IEnumerator CloseHackingPanel(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hackingPanel != null)
        {
            hackingPanel.SetActive(false);
        }
    }
}

/// <summary>
/// Individual hack node UI element
/// </summary>
public class HackNode : MonoBehaviour
{
    private int _nodeIndex;
    private HackingMiniGame _hackingGame;
    private Image _image;
    private Button _button;
    private Color _defaultColor = new Color(0.2f, 0.2f, 0.3f);

    public void Initialize(int index, HackingMiniGame hackingGame)
    {
        _nodeIndex = index;
        _hackingGame = hackingGame;

        _image = GetComponent<Image>();
        if (_image == null)
        {
            _image = gameObject.AddComponent<Image>();
        }
        _image.color = _defaultColor;

        _button = GetComponent<Button>();
        if (_button == null)
        {
            _button = gameObject.AddComponent<Button>();
        }
        _button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        _hackingGame.OnNodeClicked(_nodeIndex);
    }

    public void Highlight(Color color)
    {
        if (_image != null)
        {
            _image.color = color;
        }
    }

    public void ResetColor()
    {
        if (_image != null)
        {
            _image.color = _defaultColor;
        }
    }
}
