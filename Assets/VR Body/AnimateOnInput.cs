using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[System.Serializable]
public class AnimationInput
{
    public string animationPropertyName;
    public InputActionProperty action;
}

public class AnimateOnInput : NetworkBehaviour
{
    public List<AnimationInput> animationInputs;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        foreach (var item in animationInputs)
        {
            float actionValue = item.action.action.ReadValue<float>();
            animator.SetFloat(item.animationPropertyName, actionValue);
        }
    }
}
