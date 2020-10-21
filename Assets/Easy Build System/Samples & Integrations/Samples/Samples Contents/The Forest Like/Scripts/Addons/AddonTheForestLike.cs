﻿using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Managers;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Events;
using EasyBuildSystem.Runtimes.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EasyBuildSystem.Runtimes;

[AddOn(ADDON_NAME, ADDON_AUTHOR, ADDON_DESCRIPTION, AddOnTarget.PartBehaviour)]
public class AddonTheForestLike : AddOnBehaviour
{
    #region AddOn Fields

    public const string ADDON_NAME = "(W.I.P) The Forest Like";
    public const string ADDON_AUTHOR = "Cryptoz";
    public const string ADDON_DESCRIPTION = "Allow to create an preview and can be upgraded.";

    [HideInInspector]
    public string _Name = ADDON_NAME;

    public override string Name
    {
        get
        {
            return _Name;
        }

        protected set
        {
            _Name = value;
        }
    }

    [HideInInspector]
    public string _Author = ADDON_AUTHOR;

    public override string Author
    {
        get
        {
            return _Author;
        }

        protected set
        {
            _Author = value;
        }
    }

    [HideInInspector]
    public string _Description = ADDON_DESCRIPTION;
     
    public override string Description
    {
        get
        {
            return _Description;
        }

        protected set
        {
            _Description = value;
        }
    }

    #endregion AddOn Fields

    #region Public Fields

    [Header("The Forest Like Settings")]

    [Tooltip("Define here all the transforms to active at each upgrade interaction (works by order).")]
    public Transform[] Elements;

    #endregion

    #region Private Fields

    private PartBehaviour Part;

    private GameObject Preview;

    private List<Renderer> CacheRenderers;

    #endregion

    #region Private Methods

    private void Awake()
    {
        Part = GetComponent<PartBehaviour>();
        Part.AppearanceIndex = 0;

        if (BuildManager.Instance != null)
            BuildManager.Instance.DefaultState = StateType.Queue;
    }

    private void Start()
    {
        if (Part.CurrentState != StateType.Queue)
            return;

        if (Preview == null)
        {
            Preview = Instantiate(Part.gameObject, transform.position, transform.rotation);

            Preview.GetComponent<PartBehaviour>().EnableAllColliders();

            Preview.GetComponent<PartBehaviour>().ActiveAllTriggers();

            CacheRenderers = Preview.GetComponentsInChildren<Renderer>(true).ToList();

            gameObject.ChangeAllMaterialsInChildren(CacheRenderers.ToArray(), Part.PreviewMaterial);
            Preview.ChangeAllMaterialsColorInChildren(CacheRenderers.ToArray(), Part.PreviewAllowedColor);

            GameObject OnlyPreview = new GameObject("Preview");

            for (int i = 0; i < CacheRenderers.Count; i++)
                CacheRenderers[i].transform.SetParent(OnlyPreview.transform);

            OnlyPreview.transform.localScale += OnlyPreview.transform.localScale * 0.001f;

            Destroy(Preview.gameObject);

            OnlyPreview.transform.SetParent(transform);

            Preview = OnlyPreview;

            GameObject OnlyInteractionPreview = Instantiate(OnlyPreview);

            OnlyInteractionPreview.name = "Interaction Preview";

            OnlyInteractionPreview.transform.SetParent(transform, false);

            for (int i = 0; i < OnlyInteractionPreview.GetComponentsInChildren<Renderer>().Length; i++)
                Destroy(OnlyInteractionPreview.GetComponentsInChildren<Renderer>()[i]);

            OnlyInteractionPreview.SetLayerRecursively(LayerMask.GetMask(Constants.LAYER_INTERACTION));

            for (int i = 0; i < Elements.Length; i++)
                Elements[i].gameObject.SetActive(false);

            EventHandlers.OnChangePartState += OnChangePartState;

            EventHandlers.OnDestroyedPart += OnDestroyedPart;
        }
    }

    private void OnChangePartState(PartBehaviour part, StateType state)
    {
        if (part == Part)
        {
            if (state == StateType.Queue)
            {
                if (Preview == null)
                    return;

                gameObject.ChangeAllMaterialsInChildren(Part.Renderers.ToArray(), Part.InitialsRenders);
                Preview.ChangeAllMaterialsColorInChildren(CacheRenderers.ToArray(), Part.PreviewAllowedColor);
            }
        }
    }

    private void OnDestroyedPart(PartBehaviour part)
    {
        if (Part != part)
            return;

        Destroy(gameObject);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// This method allows to upgrade the base part.
    /// </summary>
    public void Upgrade(string tag)
    {
        Part.AppearanceIndex++;

        gameObject.ChangeAllMaterialsInChildren(Part.Renderers.ToArray(), Part.InitialsRenders);

        Elements.FirstOrDefault(x => !x.gameObject.activeSelf && x.tag == tag).gameObject.SetActive(true);

        PickableController.Instance.TempElements.Remove(tag);

        if (IsCompleted())
        {
            Part.DisableAllTriggers();

            Destroy(Preview);

            for (int i = 0; i < Elements.Length; i++)
                if (Part != null)
                    Elements[i].gameObject.SetActive(true);

            Part.ChangeState(StateType.Placed);

            if (Part.UseConditionalPhysics)
                if (!Part.CheckStability())
                    Part.ApplyPhysics();
        }
    }

    /// <summary>
    /// This method allows to get the current upgrade progression.
    /// </summary>
    public int GetCurrentProgression()
    {
        return Part.AppearanceIndex;
    }

    /// <summary>
    /// This method allows to check if the part progression is complete.
    /// </summary>
    public bool IsCompleted()
    {
        if (Part == null)
            return false;

        return Elements.Length <= Part.AppearanceIndex;
    }

    #endregion
}