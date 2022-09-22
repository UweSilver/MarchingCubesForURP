using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DualContouring
{
    public interface IVolumeGenerater
    {
        public void Generate(ContourGenerater contourGenerater);
    }
}
