using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DualContouring
{
    public interface IContourGenerater
    {
        public void Execute(Texture3D field, Material material);
    }
}
