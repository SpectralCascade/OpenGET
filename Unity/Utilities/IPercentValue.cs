using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    public interface IPercentValue
    {
        /// <summary>
        /// Returns the value.
        /// </summary>
        float GetValue();

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="v"></param>
        void SetValue(float v);
    }

}
