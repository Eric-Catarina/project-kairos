using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
public override void InstallBindings()
{
    // Diz ao Zenject: "Sempre que alguém pedir InputManager, entregue esta instância da cena"
    Container.Bind<InputManager>().FromComponentInHierarchy().AsSingle();
}
}