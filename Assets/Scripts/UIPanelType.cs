// Local: Assets/Scripts/UI/UIPanelType.cs

[System.Serializable]
public enum UIPanelType
{
    // Painéis que bloqueiam o input do jogador
    Settings,
    PauseMenu,
    // Adicione outros painéis aqui (ex: Inventory, Map)

    // Painéis que NÃO bloqueiam o input (HUD)
    HUD,
    MainMenu
}