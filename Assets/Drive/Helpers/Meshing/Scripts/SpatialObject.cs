// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;
using System;

namespace MagicKit
{
    /// <summary>
    /// SpatialObjects are used by the MLSpatialMapperController to encapsulate objects that require world mesh
    /// </summary>
    [Serializable]
    public class SpatialObject
    {
        
        //----------- Public Members -----------

        public Transform objectTransform;
        public Vector3 boundsSize;
        public Bounds bounds
        {
            get
            {
                _bounds.center = objectTransform.position;
                _bounds.size = boundsSize;
                return _bounds;
            }
        }

        //----------- Private Members -----------

        private Bounds _bounds;

        public SpatialObject(Transform obj, Vector3 extents)
        {
            objectTransform = obj;
            boundsSize = extents;
            _bounds = new Bounds(objectTransform.position, extents);
        }
    }

}