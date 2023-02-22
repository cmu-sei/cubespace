/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIReveal : MaskableGraphic
{
    /// <summary>
    /// The amount to shift the popup left.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float percentageLeft = 1;
    /// <summary>
    /// The amount to shift the popup right.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float percentageRight = 0;
    /// <summary>
    /// The amount to shift the popup on the bottom.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float percentageBottom = 1;

    /// <summary>
    /// The Animator reference on this object.
    /// </summary>
    private Animator anim;

    /// <summary>
    /// Whether the animation cycle has been fully completed.
    /// </summary>
    public bool fullCycleCompleted;

    /// <summary>
    /// How long the text should stay on screen.
    /// </summary>
    public float revealTime = 5f;

    /// <summary>
    /// How much to advance the animation.
    /// </summary>
    private int animAdvance = 0;

    /// <summary>
    /// Gets the Animator component of this popup object.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        fullCycleCompleted = true;
    }

    /// <summary>
    /// Gets a reference to the larger rect transform and sets alignment.
    /// </summary>
    /// <param name="vertexHelper"></param>
    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        var rectTransform = GetComponent<RectTransform>();
        float width = rectTransform.sizeDelta.x;
        float height = rectTransform.sizeDelta.y;

        Mathf.Clamp(percentageLeft, 0, 1);
        Mathf.Clamp(percentageRight, 0, 1);
        Mathf.Clamp(percentageBottom, 0, 1);

        vertexHelper.Clear();
        
        Vector3 vec_00 = new Vector3((width*percentageRight),   0);
        Vector3 vec_01 = new Vector3((width*percentageRight),  height*percentageBottom);
        Vector3 vec_10 = new Vector3(width*percentageLeft,  0);
        Vector3 vec_11 = new Vector3(width*percentageLeft, height*percentageBottom);

        vertexHelper.AddUIVertexQuad(new UIVertex[]{
            new UIVertex { position = vec_00, color = Color.green },
            new UIVertex { position = vec_01, color = Color.green },
            new UIVertex { position = vec_11, color = Color.green },
            new UIVertex { position = vec_10, color = Color.green },
        });
    }

    /// <summary>
    /// Marks vertices as needing to be rebuilt every frame.
    /// </summary>
    void Update()
    {
        SetVerticesDirty();
    }

    /// <summary>
    /// Toggles the popup to reveal information horizontally.
    /// </summary>
    public void ToggleHorizontal()
    {
        anim.SetTrigger("Toggle Horizontal");
    }

    /// <summary>
    /// Reveals information horizontally.
    /// </summary>
    public void RevealHorizontally()
    {
        StartCoroutine(RevealHorizontallyCoroutine());
    }

    /// <summary>
    /// Advances the animation.
    /// </summary>
    public void AdvanceAnim()
    {
        animAdvance++;
    }

    /// <summary>
    /// Gradually reveals the information horizontally.
    /// </summary>
    /// <returns>A yield statement while waiting for the animation to finish.</returns>
    private IEnumerator RevealHorizontallyCoroutine()
    {
        fullCycleCompleted = false;
        animAdvance = 0;
        ToggleHorizontal();

        // Wait for the animation to be over
        while (animAdvance != 1)
        {
            yield return null;
        }

        yield return new WaitForSeconds(revealTime);

        ToggleHorizontal();
        Debug.Log(anim.GetCurrentAnimatorStateInfo(0).normalizedTime);

        // Wait for the animation to be over
        while (animAdvance != 2)
        {
            yield return null;
        }

        fullCycleCompleted = true;
    }
}
