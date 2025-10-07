// Local: Assets/Scripts/VFX/AnimationStateNotifier.cs

using UnityEngine;

public class AnimationStateNotifier : StateMachineBehaviour
{
    // OnStateEnter é chamado quando uma transição começa e a máquina de estados começa a avaliar este estado
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AnimationEventListener>()?.PlayEnterSfx();
    }

    // OnStateExit é chamado quando uma transição termina e a máquina de estados para de avaliar este estado
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<AnimationEventListener>()?.PlayExitSfx();
    }
}