using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlowableRigidbody : MonoBehaviour, ITimeSlowable, IGrappleable
{
    [Header("Grapple Settings")]
    [Tooltip("If checked, the object can be grappled even when moving at normal speed. If unchecked, it can only be grappled when time is slowed.")]
    [SerializeField] private bool canBeGrappledWhileMoving = true;
    [Tooltip("The name of the layer the object will be moved to when it becomes grappleable.")]
    [SerializeField] private string grappleableLayerName = "Grappleable";
    
    [Header("Visual Feedback")]
    [SerializeField] private Color slowDownColor = Color.cyan;

    private Rigidbody _rb;
    private Renderer objectRenderer;
    private Color _originalColor;

    private Vector3 _savedVelocity;
    private Vector3 _savedAngularVelocity;

    private bool _isSlowed = false;
    private float _slowFactor;

    private int _grappleableLayerIndex;
    private int _originalLayerIndex;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null && objectRenderer.material != null)
        {
            _originalColor = objectRenderer.material.color;
        }

        _originalLayerIndex = gameObject.layer;
        _grappleableLayerIndex = LayerMask.NameToLayer(grappleableLayerName);
        if (_grappleableLayerIndex == -1)
        {
            Debug.LogError($"Layer '{grappleableLayerName}' not found. Please check your project's Layer settings.", this);
            _grappleableLayerIndex = _originalLayerIndex;
        }
    }

    private void Start()
    {
        TimeManipulationManager.Instance.Register(this);
        UpdateGrappleableLayer();
    }

    private void OnDisable()
    {
        if (_isSlowed)
        {
            RestoreNormalTime();
        }
        
        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.Unregister(this);
        }
    }

    private void FixedUpdate()
    {
        if (!_isSlowed) return;
        if (_slowFactor <= 0f) return;

        Vector3 newPosition = _rb.position + (_savedVelocity * _slowFactor * Time.fixedDeltaTime);
        _rb.MovePosition(newPosition);

        Quaternion deltaRotation = Quaternion.Euler(_savedAngularVelocity * _slowFactor * Time.fixedDeltaTime);
        _rb.MoveRotation(_rb.rotation * deltaRotation);
    }

    public void SlowDown(float slowPercentage)
    {
        if (_isSlowed) return;
        _isSlowed = true;

        _savedVelocity = _rb.linearVelocity;
        _savedAngularVelocity = _rb.angularVelocity;
        _slowFactor = 1.0f - (slowPercentage / 100.0f);
        _rb.isKinematic = true;

        if (objectRenderer != null)
        {
            objectRenderer.material.color = slowDownColor;
        }
        UpdateGrappleableLayer();
    }

    public void RestoreNormalTime()
    {
        if (!_isSlowed) return;
        _isSlowed = false;

        _rb.isKinematic = false;
        _rb.linearVelocity = _savedVelocity;
        _rb.angularVelocity = _savedAngularVelocity;

        if (objectRenderer != null)
        {
            objectRenderer.material.color = _originalColor;
        }
        UpdateGrappleableLayer();
    }
    
    public void SetSlowDownColor(Color newColor)
    {
        slowDownColor = newColor;
    }

    private void UpdateGrappleableLayer()
    {
        gameObject.layer = CanBeGrappledNow() ? _grappleableLayerIndex : _originalLayerIndex;
    }

    #region IGrappleable Implementation
    public bool CanBeGrappledNow()
    {
        return canBeGrappledWhileMoving || _isSlowed;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
    #endregion
}