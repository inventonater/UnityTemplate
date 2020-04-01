// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicKit
{

    ///<summary>
    /// Manages the presentation properties of the indicator SpriteRenderers assigned in the array.
    ///</summary>
    public class IndicatorProperties : MonoBehaviour
    {

        //----------- Private Members -----------

        [SerializeField] private Material _material;
        private const float MinimumAlpha = 0.09f;

        //----------- Public Methods -----------

        public void SetAlpha(float alpha)
        {
            if (alpha <= MinimumAlpha)
            {
                alpha = 0;
            }

            Color temp = _material.GetColor("_TintColor");
            temp.a = alpha;
            _material.SetColor("_TintColor", temp);
        }
    }
}
