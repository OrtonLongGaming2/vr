using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabInteractableTwoAttach : XRGrabInteractable
{
    public Transform leftAttachPoint;
    public Transform rightAttachPoint;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject.transform.CompareTag("LeftHand"))
        {
            attachTransform = leftAttachPoint;
        }
        else
        {
            attachTransform = rightAttachPoint;
        }

        base.OnSelectEntered(args);
    }
}
