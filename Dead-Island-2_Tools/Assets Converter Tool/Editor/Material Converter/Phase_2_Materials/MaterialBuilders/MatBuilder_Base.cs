using UnityEngine;
using UnityEditor;

public abstract class MatBuilder_Base
{
    public abstract void Apply(Material mat, Data_ProcessedMaterial data);

    protected void SetupSurfaceOptions(Material mat, Data_ProcessedMaterial data)
    {
        // 1. Double Sided
        mat.SetFloat("_DoubleSidedEnable", data.isTwoSide ? 1.0f : 0.0f);
        if (data.isTwoSide) mat.EnableKeyword("_DOUBLE_SIDED_ON");
        else mat.DisableKeyword("_DOUBLE_SIDED_ON");

        // 2. Alpha Clipping
        mat.SetFloat("_AlphaCutoffEnable", data.isAlphaClip ? 1.0f : 0.0f);
        if (data.isAlphaClip)
        {
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.SetFloat("_AlphaCutoff", data.alphaCutoff);
        }
        else mat.DisableKeyword("_ALPHATEST_ON");

        // 3. Surface Type (Opaque = 0, Transparent = 1)
        mat.SetFloat("_SurfaceType", data.isTransparent ? 1.0f : 0.0f);
    }
}