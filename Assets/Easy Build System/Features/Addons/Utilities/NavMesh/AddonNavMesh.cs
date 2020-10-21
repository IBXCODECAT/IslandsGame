﻿using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Managers;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Events;
using EasyBuildSystem.Runtimes.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EasyBuildSystem.Runtimes.Internal.Socket;
using UnityEngine.AI;
using EasyBuildSystem.Runtimes.Internal.Storage;
using EasyBuildSystem.Runtimes.Internal.Group;

[AddOn(ADDON_NAME, ADDON_AUTHOR, ADDON_DESCRIPTION, AddOnTarget.BuildManager)]
public class AddonNavMesh : AddOnBehaviour
{
    #region AddOn Fields

    public const string ADDON_NAME = "Dynamic Nav Mesh";
    public const string ADDON_AUTHOR = "Cryptoz";
    public const string ADDON_DESCRIPTION = "Update the Nav Mesh Surface component when a part is placed or removed.";

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

    public static AddonNavMesh Instance;

    #endregion

    #region Private Fields

    public NavMeshSurface Surface;

    #endregion

    #region Private Methods

    private void OnEnable()
    {
        if (FindObjectOfType<BuildStorage>() != null && FindObjectOfType<BuildStorage>().ExistsStorageFile())
            EventHandlers.OnStorageLoadingDone += OnStorageLoadingDone;

        EventHandlers.OnPlacedPart += OnPlacedPart;
        EventHandlers.OnDestroyedPart += OnDestroyedPart;
    }

    private void OnDisable()
    {
        EventHandlers.OnPlacedPart -= OnPlacedPart;
        EventHandlers.OnDestroyedPart -= OnDestroyedPart;
    }

    private void Awake()
    {
        UpdateMeshData();

        Instance = this;

        if (Surface == null)
            Debug.LogWarning("AddonNavMesh: Please complete empty field to use NavMeshSurface component.");
    }

    private void OnStorageLoadingDone(PartBehaviour[] parts, GroupBehaviour group)
    {
        UpdateMeshData();

        EventHandlers.OnPlacedPart += OnPlacedPart;
        EventHandlers.OnDestroyedPart += OnDestroyedPart;
    }

    private void OnPlacedPart(PartBehaviour part, SocketBehaviour socket)
    {
        UpdateMeshData();
    }

    private void OnDestroyedPart(PartBehaviour part)
    {
        UpdateMeshData();
    }

    #endregion

    #region Public Methods

    public void UpdateMeshData()
    {
        Surface.UpdateNavMesh(Surface.navMeshData);
    }

    #endregion
}