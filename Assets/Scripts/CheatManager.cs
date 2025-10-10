// Local: Assets/Scripts/Core/CheatManager.cs

using UnityEngine;

public class CheatManager : MonoBehaviour
{
    public static CheatManager Instance { get; private set; }

    [Header("Configuração de Cheats")]
    [Tooltip("Habilita ou desabilita globalmente os cheats no Editor.")]
    [SerializeField] private bool enableCheats = true;

    public bool IsInfiniteDoubleJumpActive { get; private set; }
    public bool IsInfiniteGrappleCooldownActive { get; private set; }
    public bool IsInfiniteGrappleDurationActive { get; private set; }
    public bool IsInfiniteTimeStopActive { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnToggleInfiniteJumps += ToggleInfiniteJumps;
            InputManager.Instance.OnToggleInfiniteGrappleCooldown += ToggleInfiniteGrappleCooldown;
            InputManager.Instance.OnToggleInfiniteGrappleDuration += ToggleInfiniteGrappleDuration;
            InputManager.Instance.OnToggleInfiniteTimeStop += ToggleInfiniteTimeStop;
            InputManager.Instance.OnToggleAllCheats += ToggleAllCheats;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnToggleInfiniteJumps -= ToggleInfiniteJumps;
            InputManager.Instance.OnToggleInfiniteGrappleCooldown -= ToggleInfiniteGrappleCooldown;
            InputManager.Instance.OnToggleInfiniteGrappleDuration -= ToggleInfiniteGrappleDuration;
            InputManager.Instance.OnToggleInfiniteTimeStop -= ToggleInfiniteTimeStop;
            InputManager.Instance.OnToggleAllCheats -= ToggleAllCheats;
        }
    }

    private void ToggleInfiniteJumps()
    {
        if (!enableCheats) return;
        IsInfiniteDoubleJumpActive = !IsInfiniteDoubleJumpActive;
        Debug.Log($"<color=orange>CHEAT: Double Jumps Infinitos -> {IsInfiniteDoubleJumpActive}</color>");
    }

    private void ToggleInfiniteGrappleCooldown()
    {
        if (!enableCheats) return;
        IsInfiniteGrappleCooldownActive = !IsInfiniteGrappleCooldownActive;
        Debug.Log($"<color=orange>CHEAT: Grapple Sem Cooldown -> {IsInfiniteGrappleCooldownActive}</color>");
    }

    private void ToggleInfiniteGrappleDuration()
    {
        if (!enableCheats) return;
        IsInfiniteGrappleDurationActive = !IsInfiniteGrappleDurationActive;
        Debug.Log($"<color=orange>CHEAT: Grapple com Duração Infinita -> {IsInfiniteGrappleDurationActive}</color>");
    }

    private void ToggleInfiniteTimeStop()
    {
        if (!enableCheats) return;
        IsInfiniteTimeStopActive = !IsInfiniteTimeStopActive;
        Debug.Log($"<color=orange>CHEAT: Time Stop Infinito -> {IsInfiniteTimeStopActive}</color>");
    }

    private void ToggleAllCheats()
    {
        if (!enableCheats) return;
        IsInfiniteDoubleJumpActive = !IsInfiniteDoubleJumpActive;
        IsInfiniteGrappleCooldownActive = !IsInfiniteGrappleCooldownActive;
        IsInfiniteGrappleDurationActive = !IsInfiniteGrappleDurationActive;
        IsInfiniteTimeStopActive = !IsInfiniteTimeStopActive;
        Debug.Log($"<color=orange>CHEAT: Todos os Cheats -> {(IsInfiniteDoubleJumpActive && IsInfiniteGrappleCooldownActive && IsInfiniteGrappleDurationActive && IsInfiniteTimeStopActive)}</color>");
    }
#endif
}