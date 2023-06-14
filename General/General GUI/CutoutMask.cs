using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace RD
{
	public class CutoutMask : Image
	{
		public override Material materialForRendering{
			get { Material material = new Material(base.materialForRendering);
				material.SetFloat("_StencilComp", (float)CompareFunction.NotEqual);
				return material;
			}
		}
	}
}
