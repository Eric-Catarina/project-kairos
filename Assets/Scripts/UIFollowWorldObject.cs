// Assets/Scripts/UI/UIFollowWorldObject.cs

using UnityEngine;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class UIFollowWorldObject : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Um deslocamento opcional no espaço da tela (em pixels).")]
    [SerializeField] private Vector2 screenOffset;

    private RectTransform _uiElement;
    private CanvasGroup _canvasGroup;
    private Camera _mainCamera;
    private Transform _targetToFollow;
    private Canvas _rootCanvas;

    private void Awake()
    {
        _uiElement = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _mainCamera = Camera.main;
        _rootCanvas = GetComponentInParent<Canvas>();

        _canvasGroup.alpha = 0;
    }

    private void LateUpdate()
    {
        if (_targetToFollow == null || !_targetToFollow.gameObject.activeInHierarchy)
        {
            if (_canvasGroup.alpha > 0)
            {
                _canvasGroup.alpha = 0;
            }
            return;
        }

        if (_canvasGroup.alpha < 1)
        {
            _canvasGroup.alpha = 1;
        }

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 worldPosition = _targetToFollow.position;
        Vector3 screenPoint = _mainCamera.WorldToScreenPoint(worldPosition);

        // Verifica se o ponto está na frente da câmera
        if (screenPoint.z < 0)
        {
            _canvasGroup.alpha = 0;
            return;
        }

        // Aplica o offset diretamente na posição de tela
        screenPoint.x += screenOffset.x;
        screenPoint.y += screenOffset.y;

        // --- MUDANÇA CRÍTICA AQUI ---
        // Em vez de usar RectTransformUtility, definimos a posição diretamente,
        // o que é mais confiável em diferentes modos de Canvas.
        
        // Para Canvas Screen Space - Overlay, a screenPoint já é a posição correta.
        if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            _uiElement.position = screenPoint;
        }
        else // Para Screen Space - Camera ou World Space, o RectTransformUtility ainda é necessário.
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform, 
                screenPoint, 
                _rootCanvas.worldCamera, 
                out Vector2 localPoint
            );
            _uiElement.anchoredPosition = localPoint;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        _targetToFollow = newTarget;
    }
}