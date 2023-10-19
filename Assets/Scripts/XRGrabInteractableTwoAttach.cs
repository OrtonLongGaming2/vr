using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabInteractableTwoAttach : XRGrabInteractable
{
    public Transform leftAttachPoint;
    public Transform rightAttachPoint;

    protected override void OnHoverEntered(HoverEnterEventArgs args) // OnSelectEntered
    {
        if (args.interactorObject.transform.CompareTag("LeftHand"))
        {
            attachTransform = leftAttachPoint;
        }
        else
        {
            attachTransform = rightAttachPoint;
        }

        base.OnHoverEntered(args);
    }
}
